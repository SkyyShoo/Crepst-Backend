using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtraitsController : ControllerBase
    {
        private readonly BackendContext _context;

        public ExtraitsController(BackendContext context)
        {
            _context = context;
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

        // PUT: api/Extraits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
                if (!ExtraitExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Extraits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Extrait>> PostExtrait(Extrait extrait)
        {
            _context.Extraits.Add(extrait);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExtrait", new { id = extrait.Id }, extrait);
        }

        // DELETE: api/Extraits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExtrait(int id)
        {
            var extrait = await _context.Extraits.FindAsync(id);
            if (extrait == null)
            {
                return NotFound();
            }

            _context.Extraits.Remove(extrait);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExtraitExists(int id)
        {
            return _context.Extraits.Any(e => e.Id == id);
        }
    }
}
