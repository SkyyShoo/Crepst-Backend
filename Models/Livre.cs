using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Backend.Models
{
    public class Livre
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Auteur { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AnneePublication { get; set; }
        public string FileName { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        [ValidateNever]
        public virtual List<User>? Users { get; set; } = null!;
        [ValidateNever]
        public virtual List<Event> Events { get; set; } = null!;
    }
}
