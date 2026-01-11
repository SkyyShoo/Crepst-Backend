using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Backend.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Resumer { get; set; } = string.Empty;
        public string Lieu { get; set; } = string.Empty;
        public string Thematique { get; set; } = string.Empty;
        public List<int>? ExtraitId { get; set; } = null!;
        public List<int>? CommentsId { get; set; } = null!;
        [ValidateNever]
        public virtual List<User>? Participants { get; set; } = null!; 
        [ValidateNever]
        public virtual List<Extrait> Extraits { get; set; } = null!;
        [ValidateNever]
        public virtual List<Comment>? Comments { get; set; } = new List<Comment>();
    }
}
