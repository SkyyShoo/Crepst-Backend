using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public EventsController(BackendContext context)
        {
            _context = context;
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
            
                TimeZoneInfo montrealTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");
            
                DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, montrealTimeZone);
                
                @event.Titre = @eventDTO.Title;
                @event.Date = utcDateTime;
                @event.Lieu = @eventDTO.Lieu;
                @event.Thematique = @eventDTO.Thematique;
                @event.Resumer = @eventDTO.Resumer;

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
            
            TimeZoneInfo montrealTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");
            
            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, montrealTimeZone);
            Event @event = new Event{Titre = @eventDTO.Title, Date = utcDateTime, Lieu = @eventDTO.Lieu, Resumer = @eventDTO.Resumer, Thematique = eventDTO.Thematique};
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

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
