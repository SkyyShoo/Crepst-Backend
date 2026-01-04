using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ExtraitsController : ControllerBase
    {
        private readonly BackendContext _context;
        private readonly UserManager<User> _userManager;

        public ExtraitsController(BackendContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: api/Extraits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Extrait>> GetExtrait(int id)
        {
            var extrait = await _context.Extraits.FindAsync(id);

            if (extrait == null)
            {
                return NotFound();
            }

            return extrait;

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<Extrait>>> GetExtraitsbyEvent(int id)
        {
            var events = await _context.Events.FindAsync(id);

            if (events == null)
            {
                return NotFound();
            }
            // A changer lorsque la création d'extrait et events sera complétement fonctionnel
            if (events.ExtraitId != null)
                foreach (int extraitId in events.ExtraitId)
                {
                    var extrait = await GetExtrait(extraitId);
                    if (extrait.Value != null)
                    {
                        events.Extraits.Add(extrait.Value);
                    }
                }


            return events.Extraits;

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExtraitFile(int id)
        {
            var extrait = await _context.Extraits
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extrait == null)
            {
                Console.WriteLine($"Extrait {id} non trouvé");
                return NotFound(new { Message = "Extrait non trouvé" });
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", extrait.FileName);

            Console.WriteLine($"Recherche du fichier: {filePath}");

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"Fichier non trouvé: {filePath}");
                return NotFound(new { Message = $"Fichier PDF non trouvé: {extrait.FileName}" });
            }

            Console.WriteLine($"Fichier trouvé, taille: {new FileInfo(filePath).Length} bytes");

            // Lire et retourner le fichier
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", extrait.FileName);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutExtrait(int id, Extrait extrait)
        {
            if (id != extrait.Id)
            {
                return BadRequest();
            }

            _context.Entry(extrait).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
             
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<Extrait>> PostExtrait()
        {
            try
            {
                Console.WriteLine("=== DÉBUT POST EXTRAIT ===");

                // Récupérer l'utilisateur
                User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user == null)
                {
                    Console.WriteLine("User non trouvé");
                    return Unauthorized(new { Message = "Utilisateur non authentifié" });
                }
                Console.WriteLine($"User trouvé: {user.UserName}");

                // Lire le formulaire manuellement
                var form = await Request.ReadFormAsync();

                Console.WriteLine($"Formulaire reçu avec {form.Files.Count} fichier(s)");

                // Récupérer le fichier
                var file = form.Files.GetFile("file");
                if (file == null || file.Length == 0)
                {
                    Console.WriteLine("Aucun fichier dans le formulaire");
                    return BadRequest(new { Message = "Aucun fichier reçu" });
                }

                Console.WriteLine($"Fichier: {file.FileName}, Taille: {file.Length} bytes");

                // Vérifier le type
                if (file.ContentType != "application/pdf")
                {
                    Console.WriteLine($"Type incorrect: {file.ContentType}");
                    return BadRequest(new { Message = "Seuls les fichiers PDF sont acceptés" });
                }

                // Récupérer les autres champs
                string auteur = form["Auteur"].ToString();
                string titre = form["Titre"].ToString();
                string traduction = form["Traduction"].ToString();
                string edition = form["Edition"].ToString();
                string numPages = form["NumPages"].ToString();

                if (!int.TryParse(form["EventId"].ToString(), out int eventId))
                {
                    Console.WriteLine("EventId invalide");
                    return BadRequest(new { Message = "EventId invalide" });
                }

                int? anneeParution = null;
                if (!string.IsNullOrEmpty(form["AnneeParution"]))
                {
                    if (int.TryParse(form["AnneeParution"].ToString(), out int annee))
                    {
                        anneeParution = annee;
                    }
                }

                Console.WriteLine($"Données: Auteur={auteur}, Titre={titre}, EventId={eventId}");

                // Créer le dossier uploads
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    Console.WriteLine("Dossier uploads créé");
                }

                // Sauvegarder le fichier
                var fileName = Guid.NewGuid().ToString() + ".pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);

                Console.WriteLine($"Sauvegarde du fichier: {filePath}");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine("Fichier sauvegardé avec succès");

                // Créer l'entité
                var nouvelExtrait = new Extrait
                {
                    Auteur = auteur,
                    Titre = titre,
                    Traduction = traduction,
                    AnneeParution = anneeParution,
                    Edition = edition,
                    NumPages = numPages,
                    FileName = fileName,
                    MimeType = "application/pdf",
                    EventId = eventId,
                    User = user
                };

                Console.WriteLine("Ajout à la base de données...");
                _context.Extraits.Add(nouvelExtrait);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== FIN POST EXTRAIT (SUCCÈS) ===");

                return Ok(nouvelExtrait);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR EXCEPTION ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    Message = "Erreur lors de l'upload",
                    Details = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}
