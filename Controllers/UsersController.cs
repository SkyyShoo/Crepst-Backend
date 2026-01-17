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
                if (!removeResult.Succeeded) return BadRequest("Échec de la suppression des rôles actuels");
            }

            var addResult = await _userManager.AddToRoleAsync(user, addRoleDTO.Role);
            if (!addResult.Succeeded) return BadRequest("Erreur lors de l'ajout du nouveau rôle");

            return Ok(new { Message = "Rôle mis à jour avec succès" });
        }
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> ChangeUsername(changeUsernameDTO changeUsernameDTO)
        {
            if (changeUsernameDTO.NewUsername == null || changeUsernameDTO.NewUsername == "")
                return BadRequest(new { Message = "Le nom d'utilisateur n'est pas valide" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null) return BadRequest(new { Message = "Utilisateur introuvable" });

            var existingUser = await _userManager.FindByNameAsync(changeUsernameDTO.NewUsername);
            if (existingUser != null && existingUser.Id != currentUserId)
            {
                return BadRequest(new { Message = "Ce nom d'utilisateur est déjà utilisé" });
            }

            currentUser.UserName = changeUsernameDTO.NewUsername;
            var result = await _userManager.UpdateAsync(currentUser);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Erreur lors de la mise à jour du nom d'utilisateur" });
            }

            return Ok(new { Message = "Nom d'utilisateur mis à jour avec succès" });
        }
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (changePasswordDTO.NewPassword == null || changePasswordDTO.NewPassword == "")
                return BadRequest(new { Message = "Le mot de passe n'est pas valide" });

            if (changePasswordDTO.CurrentPassword == null || changePasswordDTO.CurrentPassword == "")
                return BadRequest(new { Message = "Le mot de passe actuel est requis" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return BadRequest(new { Message = "Utilisateur introuvable" });

            // Vérifier que le mot de passe actuel est correct
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(currentUser, changePasswordDTO.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                return BadRequest(new { Message = "Le mot de passe actuel est incorrect" });
            }

            // Changer le mot de passe
            var result = await _userManager.ChangePasswordAsync(
                currentUser,
                changePasswordDTO.CurrentPassword,
                changePasswordDTO.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Message = $"Erreur lors du changement de mot de passe : {errors}" });
            }

            return Ok(new { Message = "Mot de passe mis à jour avec succès" });
        }
    }
}