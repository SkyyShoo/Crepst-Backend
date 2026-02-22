using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
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
    public class EventsController : ControllerBase
    {
        private readonly BackendContext _context;
        private readonly IWebHostEnvironment _env;

        private readonly string _pdfPath;

        public EventsController(BackendContext context, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

            bool isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isAzure)
            {
                _pdfPath = "/home/data/pdfsEvent";
            }
            else
            {
                _pdfPath = Path.Combine(env.ContentRootPath, "Assets", "PDF_Event");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.OrderByDescending(p => p.Date).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        [HttpGet]
        public async Task<ActionResult<Event>> NextEvent()
        {
            return await _context.Events.OrderBy(p=>p.Date).LastAsync();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateEvent(int id, EventDTO @eventDTO)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new { Message = "Event introuvable" });
            }
            
            if (!User.IsInRole("admin"))
            {
                return Unauthorized(new { Message = "L'utilisateur n'a pas accès à modifier un événement" });
            }
            try
            {
                DateTime localDateTime = DateTime.SpecifyKind(@eventDTO.Date, DateTimeKind.Unspecified);
                DateTime localDateExtraitTime = DateTime.SpecifyKind(@eventDTO.DateFinExtrait, DateTimeKind.Unspecified);

                TimeZoneInfo montrealTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");
            
                DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, montrealTimeZone);
                DateTime utcDateExtaitTime = TimeZoneInfo.ConvertTimeToUtc(localDateExtraitTime, montrealTimeZone);


                @event.Titre = @eventDTO.Title;
                @event.Date = utcDateTime;
                @event.Lieu = @eventDTO.Lieu;
                @event.Thematique = @eventDTO.Thematique;
                @event.Resumer = @eventDTO.Resumer;
                @event.DateFinExtrait = utcDateExtaitTime;

                _context.Events.Update(@event);
                await _context.SaveChangesAsync();
                return Ok(new {Message = "Event modifié avec succès"});
            }
            catch
            {
                return BadRequest("Une erreur est arrivé lors de la moficiation");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin,moderator")]
        public async Task<ActionResult<Event>> CreateEvent(EventDTO @eventDTO)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized(new { Message = "L'utilisateur n'a pas accès à créer un événement" });
            }
            DateTime localDateTime = DateTime.SpecifyKind(@eventDTO.Date, DateTimeKind.Unspecified);
            DateTime localDateExtraitTime = DateTime.SpecifyKind(@eventDTO.DateFinExtrait, DateTimeKind.Unspecified);


            TimeZoneInfo montrealTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");
            
            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, montrealTimeZone);
            DateTime utcDateExtraitTime = TimeZoneInfo.ConvertTimeToUtc(localDateExtraitTime, montrealTimeZone);
            Event @event = new Event{Titre = @eventDTO.Title, Date = utcDateTime, Lieu = @eventDTO.Lieu, Resumer = @eventDTO.Resumer, Thematique = eventDTO.Thematique, DateFinExtrait = utcDateExtraitTime };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return Ok(new {message = "Event ajouté !"});
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,moderator")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null && !User.IsInRole("admin") && !User.IsInRole("moderator"))
            {
                return Unauthorized(new { Message = "L'utilisateur n'a pas accès à supprimer un événement" });
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new {Message = "Événement introuvable"});
            }

            if (!string.IsNullOrEmpty(@event.FileName))
            {
                var resumePath = Path.Combine(_pdfPath, @event.FileName);
                if (System.IO.File.Exists(resumePath))
                {
                    try
                    {
                        System.IO.File.Delete(resumePath);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            var extraits = await _context.Extraits.Where(p => p.EventId == id).ToListAsync();
            foreach(var extrait in extraits)
            {
                if (!string.IsNullOrEmpty(extrait.FileName))
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsFolder, extrait.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch(Exception ex)
                        {
                            return BadRequest(new { Message = $"Erreur lors de la suppression du fichier {filePath}: {ex.Message}" });
                        }
                    }
                }
            }
            _context.Extraits.RemoveRange(extraits);

            List<Comment> comments = await _context.Comments.Where(p => p.EventId == id).ToListAsync();
            _context.Comments.RemoveRange(comments);

            _context.Events.Remove(@event);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost("{id}")]
        [Authorize(Roles = "admin")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UploadResume(int id)
        {
            try
            {
                // Vérifier que l'utilisateur est admin
                if (!User.IsInRole("admin"))
                {
                    return Unauthorized(new { Message = "Seuls les administrateurs peuvent ajouter un résumé" });
                }

                // Récupérer l'événement
                var @event = await _context.Events.FindAsync(id);
                if (@event == null)
                {
                    return NotFound(new { Message = "Événement introuvable" });
                }

                // Lire le formulaire
                var form = await Request.ReadFormAsync();
                var file = form.Files.GetFile("file");

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "Aucun fichier reçu" });
                }

                // Vérifier le type
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { Message = "Seuls les fichiers PDF sont acceptés" });
                }

                // Supprimer l'ancien fichier si existant
                if (!string.IsNullOrEmpty(@event.FileName))
                {
                    var oldFilePath = Path.Combine(_pdfPath, @event.FileName);
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

                // Créer le dossier s'il n'existe pas
                if (!Directory.Exists(_pdfPath))
                {
                    Directory.CreateDirectory(_pdfPath);
                }

                // Sauvegarder le nouveau fichier
                var fileName = $"resume_{@event.Id}_{Guid.NewGuid()}.pdf";
                var filePath = Path.Combine(_pdfPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Mettre à jour l'événement
                @event.FileName = fileName;
                @event.MimeType = "application/pdf";

                _context.Entry(@event).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Résumé ajouté avec succès",
                    FileName = fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Erreur lors de l'upload du résumé",
                    Details = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventResume(int id)
        {
            try
            {
                var @event = await _context.Events.FindAsync(id);

                if (@event == null)
                {
                    return NotFound(new { Message = "Événement introuvable" });
                }

                if (string.IsNullOrEmpty(@event.FileName))
                {
                    return NotFound(new { Message = "Aucun résumé disponible pour cet événement" });
                }

                var filePath = Path.Combine(_pdfPath, @event.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { Message = "Fichier introuvable" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, "application/pdf", $"Resume_{@event.Titre}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erreur lors de la récupération du résumé" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteEventResume(int id)
        {
            try
            {
                if (!User.IsInRole("admin"))
                {
                    return Unauthorized(new { Message = "Seuls les administrateurs peuvent supprimer un résumé" });
                }

                var @event = await _context.Events.FindAsync(id);

                if (@event == null)
                {
                    return NotFound(new { Message = "Événement introuvable" });
                }

                if (string.IsNullOrEmpty(@event.FileName))
                {
                    return NotFound(new { Message = "Aucun résumé à supprimer" });
                }

                // Supprimer le fichier
                var filePath = Path.Combine(_pdfPath, @event.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Mettre à jour l'événement
                @event.FileName = null;
                @event.MimeType = null;

                _context.Entry(@event).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Résumé supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erreur lors de la suppression du résumé" });
            }
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
