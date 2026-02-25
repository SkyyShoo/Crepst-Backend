using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class EventService : IEventService
    {
        private readonly BackendContext _context;

        public EventService(BackendContext db)
        {
            _context = db;
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            return await _context.Events.OrderByDescending(p => p.Date).ToListAsync();
        }

        public async Task<Event?> Get(int eventId)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<Event?> Next()
        {
            return await _context.Events.OrderBy(p => p.Date).LastOrDefaultAsync();
        }

        public async Task<Event?> Update(Event @event, EventDTO @eventDTO)
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
            return @event;
        }

        public async Task<Event?> Create(EventDTO @eventDTO)
        {
            DateTime localDateTime = DateTime.SpecifyKind(@eventDTO.Date, DateTimeKind.Unspecified);
            DateTime localDateExtraitTime = DateTime.SpecifyKind(@eventDTO.DateFinExtrait, DateTimeKind.Unspecified);


            TimeZoneInfo montrealTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");

            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, montrealTimeZone);
            DateTime utcDateExtraitTime = TimeZoneInfo.ConvertTimeToUtc(localDateExtraitTime, montrealTimeZone);

            Event @event = new Event { Titre = @eventDTO.Title, Date = utcDateTime, Lieu = @eventDTO.Lieu, Resumer = @eventDTO.Resumer, Thematique = eventDTO.Thematique, DateFinExtrait = utcDateExtraitTime };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task<Event?> Delete(int id)
        {
            var @event = await _context.Events
                .Include(e => e.Extraits)
                .Include(e => e.Comments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (@event == null) return null;

            if (@event.Extraits?.Any() == true)
                _context.Extraits.RemoveRange(@event.Extraits);

            if (@event.Comments?.Any() == true)
                _context.Comments.RemoveRange(@event.Comments);

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return @event;
        }

        public async Task<Event?> UpdateResumeFile(int id, string? fileName, string? mimeType)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return null;

            @event.FileName = fileName;
            @event.MimeType = mimeType;

            _context.Entry(@event).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return @event;
        }
    }
}
