using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly BackendContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<User> userManager, BackendContext backendContext, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = backendContext;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterDTO register)
        {
            if (register.Password != register.PasswordConfirm)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Les deux mots de passe spécifiés sont différents." });
            }

            User user = new User()
            {
                UserName = register.Username,
                Email = register.Email
            };

            IdentityResult identityResult = await _userManager.CreateAsync(user, register.Password);


            // Si la création a échoué, on retourne une erreur. N'hésitez pas à mettre un breakpoint ici
            // pour inspecter l'objet identityResult si vous avez du mal à créer des utilisateurs.
            if (!identityResult.Succeeded)
            {
                // On inspecte la liste des erreurs renvoyées par Identity
                var errors = identityResult.Errors.ToList();

                // Retourne les erreurs
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "La création de l'utilisateur a échoué.", Details = errors });
            }
            await _userManager.AddToRoleAsync(user, "Utilisateur");
            return Ok(new { Message = "Inscription réussie ! 🥳" });
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO login)
        {
            User? user = await _userManager.FindByNameAsync(login.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                List<Claim> authClaims = new List<Claim>();
                foreach (string role in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                authClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes("REMOVED_JWT_KEY"));
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "https://localhost:7272",
                    audience: "http://localhost:4200",
                    claims: authClaims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    validTo = token.ValidTo,
                    role = roles,
                    username = user.UserName
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Le nom d'utilisateur ou le mot de passe est invalide." });
            }
        }
        [HttpGet]
        public async Task<List<UserDTO>> GetAll()
        {
            // On utilise une requête LINQ pour projeter les données vers le DTO
            var users = await _userManager.Users.Select(user => new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,

                // C'est ici que la magie opère : on va chercher les rôles liés
                Role = _context.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Join(_context.Roles,
                                      ur => ur.RoleId,
                                      r => r.Id,
                                      (ur, r) => r.Name)
                                .FirstOrDefault() ?? "Aucun rôle"
            }).ToListAsync();

            return users;
        }
        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddRoleAdminOrModOrRemove(AddRoleDTO addRoleDTO)
        {
            // Vérification que l'utilisateur est bien un administrateur
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (!await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                    return Forbid();
            }

            User? user = await _context.Users.FindAsync(addRoleDTO.UserId);
            if (user == null) return BadRequest("Utilisateur introuvable");
            if (user == currentUser) return BadRequest("Un utilisateur ne peut pas modifier ou retirer son propre rôle");

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if(!removeResult.Succeeded) return BadRequest("Échec de la suppression des rôles actuels");
            }

            var addResult = await _userManager.AddToRoleAsync(user, addRoleDTO.Role);
            if (!addResult.Succeeded) return BadRequest("Erreur lors de l'ajout du nouveau rôle");

            return Ok( new {Message = "Rôle mis à jour avec succès" });
        }
    }
}