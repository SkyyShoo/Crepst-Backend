using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services.Interfaces;
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
        private readonly IWebHostEnvironment _env;
        private readonly IEventService _eventService;

        private readonly string _pdfPath;

        public EventsController(UserManager<User> userManager, IWebHostEnvironment env, IEventService eventService)
        {
            _env = env;
            _eventService = eventService;

            if (env.IsDevelopment())
            {
                _pdfPath = Path.Combine(env.ContentRootPath, "Assets", "PDF_Event");
            }
            else
            {
                _pdfPath = "/var/www/crepst-storage/pdfsEvent";
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return Ok(await _eventService.GetEvents());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            Event @event = await _eventService.Get(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        [HttpGet]
        public async Task<ActionResult<Event>> NextEvent()
        {
            Event @event = await _eventService.Next();

            if (@event == null)
                return NotFound();

            return @event;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateEvent(int id, EventDTO @eventDTO)
        {
            Event @event = await _eventService.Get(id);
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
                await _eventService.Update(@event, @eventDTO);
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

            await _eventService.Create(@eventDTO);
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

            Event? deletedEvent = await _eventService.Delete(id);

            if (deletedEvent == null)
                return NotFound(new { Message = "Événement introuvable" });

            // Supprimer le PDF de l'événement
            if (!string.IsNullOrEmpty(deletedEvent.FileName))
            {
                var resumePath = Path.Combine(_pdfPath, deletedEvent.FileName);
                if (System.IO.File.Exists(resumePath))
                {
                    try { System.IO.File.Delete(resumePath); }
                    catch { }
                }
            }

            // Supprimer les fichiers des extraits
            if (deletedEvent.Extraits?.Any() == true)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                foreach (var extrait in deletedEvent.Extraits)
                {
                    if (string.IsNullOrEmpty(extrait.FileName)) continue;

                    var filePath = Path.Combine(uploadsFolder, extrait.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        try { System.IO.File.Delete(filePath); }
                        catch { }
                    }
                }
            }

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
                Event? @event = await _eventService.Get(id);
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
                await _eventService.UpdateResumeFile(id, fileName, "application/pdf");

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
                var @event = await _eventService.Get(id);

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

                var @event = await _eventService.Get(id);

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
                await _eventService.UpdateResumeFile(id, null, null);

                return Ok(new { Message = "Résumé supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erreur lors de la suppression du résumé" });
            }
        }
    }
}
