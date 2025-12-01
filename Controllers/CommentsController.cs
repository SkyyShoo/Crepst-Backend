using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly BackendContext _context;

        private readonly UserManager<User> _userManager;

        public CommentsController(BackendContext context , UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<CommentDTO>>> GetCommentsbyEvent(int id)
        {
            var comments = await _context.Comments.Where(c => c.EventId == id).ToListAsync();

            if (comments == null)
            {
                return NotFound();
            }
            List<CommentDTO> commentDTOs = new List<CommentDTO>();
                foreach (Comment comment in comments)
                {
                        CommentDTO commentDTO = new()
                        {
                            Text = comment.Text,
                            Id = comment.Id,
                            Date = comment.Date,
                            Author = comment.User?.UserName ?? "Anonyme",
                            EventId = id
                        };
                        commentDTOs.Add(commentDTO);
                    
                }

            return commentDTOs;
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> PostComment(CommentDTO commentDTO)
        {
            User? user = await _userManager.FindByNameAsync(commentDTO.Author);

            Comment comment = new Comment
            {
                Text = commentDTO.Text,
                Date = commentDTO.Date ?? DateTime.Now,
                User = user,
                EventId = commentDTO.EventId
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(commentDTO);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
