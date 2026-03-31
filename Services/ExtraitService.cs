using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class ExtraitService : IExtraitService
    {
        private readonly BackendContext _context;

        public ExtraitService(BackendContext db)
        {
            _context = db;
        }

        public async Task<Extrait?> Get(int extraitId)
        {
            return await _context.Extraits.AsNoTracking().FirstOrDefaultAsync(e => e.Id == extraitId);
        }

        public async Task<Extrait?> GetWithUserAndEvent(int extraitId)
        {
            return await _context.Extraits.Include(e => e.User).Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == extraitId);
        }

        public async Task<List<Extrait>> GetExtraitsByEvent(int eventId)
        {
            return await _context.Extraits.AsNoTracking().Where(e => e.EventId == eventId).ToListAsync();
        }

        public async Task<Extrait> Add(Extrait extrait)
        {
            ArgumentNullException.ThrowIfNull(extrait);

            _context.Extraits.Add(extrait);
            await _context.SaveChangesAsync();

            return extrait;
        }

        public async Task<Extrait> Delete(Extrait extrait)
        {
            ArgumentNullException.ThrowIfNull(extrait);

            _context.Extraits.Remove(extrait);
            await _context.SaveChangesAsync();

            return extrait;
        }

        public async Task UpdateExtrait(Extrait extrait)
        {
            ArgumentNullException.ThrowIfNull(extrait);

            _context.Entry(extrait).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
