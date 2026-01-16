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
using System.Security.Claims;
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

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Comment>> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized(new { Message = "Utilisateur non authentifié" });
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { Message = "Commentaire introuvable" });
            }

            User? user = await _userManager.FindByIdAsync(currentUserId);
            bool isOwner = comment.User.Id == user?.Id;

            if (!isOwner)
            {
                return Forbid();
            }

            comment.Text = dto.Text;

            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(comment);
        }

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

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if(user == null) return NotFound("Utilisateur introuvable");

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound("Commentaire introuvable");

            var roles = await _userManager.GetRolesAsync(user);

            if (user.Id == comment.User.Id || roles.Contains("admin") || roles.Contains("moderator"))
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok(new {Message = "Commentaire supprimé avec succès"});
            }
            return Unauthorized("Impossible de supprimer un commentaire qui ne vous appartient pas");
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
