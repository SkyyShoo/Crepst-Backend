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
        private readonly IWebHostEnvironment _env;

        private readonly string _pdfPath;

        public ExtraitsController(BackendContext context, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;

            bool isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isAzure)
            {
                _pdfPath = "/home/data/pdfs";
            }
            else
            {
                _pdfPath = Path.Combine(env.ContentRootPath, "Assets", "PDF_Extrait");
            }
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


            var filePath = Path.Combine(_pdfPath, extrait.FileName);

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
                // Récupérer l'utilisateur
                User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user == null)
                {
                    return Unauthorized(new { Message = "Utilisateur non authentifié" });
                }

                // Lire le formulaire manuellement
                var form = await Request.ReadFormAsync();

                Console.WriteLine($"Formulaire reçu avec {form.Files.Count} fichier(s)");

                // Récupérer le fichier
                var file = form.Files.GetFile("file");
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "Aucun fichier reçu" });
                }

                Console.WriteLine($"Fichier: {file.FileName}, Taille: {file.Length} bytes");

                // Vérifier le type
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { Message = "Seuls les fichiers PDF sont acceptés" });
                }

                // Récupérer les autres champs
                string auteur = form["Auteur"].ToString();
                string titre = form["Titre"].ToString();
                string chapitre = form["Chapitre"].ToString();
                string traduction = form["Traduction"].ToString();
                string edition = form["Edition"].ToString();
                string numPages = form["NumPages"].ToString();

                if (!int.TryParse(form["EventId"].ToString(), out int eventId))
                {
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
                string uploadsFolder = _pdfPath;
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
                    Chapitre = chapitre,
                    Traduction = traduction,
                    AnneeParution = anneeParution,
                    Edition = edition,
                    NumPages = numPages,
                    FileName = fileName,
                    MimeType = "application/pdf",
                    EventId = eventId,
                    User = user,
                    OwnerName = user.UserName
                };

                _context.Extraits.Add(nouvelExtrait);
                await _context.SaveChangesAsync();

                return Ok(nouvelExtrait);
            }
            catch (Exception ex)
            {
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
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteExtrait(int id)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur connecté
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { Message = "Utilisateur non authentifié" });
                }

                // Récupérer l'extrait en base de données
                var extrait = await _context.Extraits
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (extrait == null)
                {
                    return NotFound(new { Message = "Extrait introuvable" });
                }

                // Vérification des droits (Sécurité)
                if (extrait.User == null || extrait.User.Id != currentUserId && !User.IsInRole("admin") && !User.IsInRole("moderator"))
                {
                    return StatusCode(403, new { Message = "Vous n'avez pas le droit de supprimer cet extrait." });
                }

                // 4. Supprimer le fichier physique du dossier uploads
                if (!string.IsNullOrEmpty(extrait.FileName))
                {
                    string uploadsFolder = _pdfPath;
                    string filePath = Path.Combine(uploadsFolder, extrait.FileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception ioEx)
                        {
                            // On log l'erreur mais on ne bloque pas la suppression en BDD 
                            // (sinon on se retrouve avec un enregistrement impossible à supprimer)
                            Console.WriteLine($"Erreur suppression fichier physique: {ioEx.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Le fichier physique n'existait pas (déjà supprimé ?)");
                    }
                }

                // 5. Supprimer l'entrée en base de données
                _context.Extraits.Remove(extrait);
                await _context.SaveChangesAsync();


                return Ok(new { Message = "Extrait supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Erreur lors de la suppression",
                    Details = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<Extrait>> UpdateExtrait(int id)
        {
            try
            {
                // Récupérer l'utilisateur
                User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user == null)
                {
                    return Unauthorized(new { Message = "Utilisateur non authentifié" });
                }

                // Récupérer l'extrait existant
                var extrait = await _context.Extraits.FindAsync(id);
                if (extrait == null)
                {
                    return NotFound(new { Message = "Extrait introuvable" });
                }

                // Vérifier les permissions (propriétaire, admin ou modérateur)
                bool isOwner = extrait.OwnerName == user.UserName;
                bool isAdmin = User.IsInRole("admin");
                bool isModerator = User.IsInRole("moderator");

                if (!isOwner && !isAdmin && !isModerator)
                {
                    return Unauthorized(new {Message = "Vous n'avez pas les accès pour modifier cet extrait"});
                }

                var form = await Request.ReadFormAsync();

                extrait.Auteur = form["Auteur"].ToString();
                extrait.Titre = form["Titre"].ToString();
                extrait.Chapitre = form["Chapitre"].ToString();
                extrait.Traduction = form["Traduction"].ToString();
                extrait.Edition = form["Edition"].ToString();
                extrait.NumPages = form["NumPages"].ToString();

                if (!string.IsNullOrEmpty(form["AnneeParution"]))
                {
                    if (int.TryParse(form["AnneeParution"].ToString(), out int annee))
                    {
                        extrait.AnneeParution = annee;
                    }
                }

                var file = form.Files.GetFile("file");
                if (file != null && file.Length > 0)
                {
                    if (file.ContentType != "application/pdf")
                    {
                        return BadRequest(new { Message = "Seuls les fichiers PDF sont acceptés" });
                    }

                    // Supprimer l'ancien fichier
                    if (!string.IsNullOrEmpty(extrait.FileName))
                    {
                        var oldFilePath = Path.Combine(
                            _pdfPath,
                            extrait.FileName
                        );

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }

                    // Sauvegarder le nouveau fichier
                    string uploadsFolder = _pdfPath;

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + ".pdf";
                    var filePath = Path.Combine(uploadsFolder, fileName);


                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }


                    // Mettre à jour le nom du fichier
                    extrait.FileName = fileName;
                    extrait.MimeType = "application/pdf";
                }
                else
                {
                }

                // Sauvegarder les modifications
                _context.Entry(extrait).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(extrait);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(409, new { Message = "Conflit lors de la mise à jour" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Erreur lors de la modification",
                    Details = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> DownloadPdf(int id)
        {
            var extrait = await _context.Extraits.FindAsync(id);
            if (extrait == null)
            {
                return NotFound(new { Message = "Extrait introuvable" });
            }

            var filePath = _pdfPath;

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", extrait.Titre);
        }

    }
}
