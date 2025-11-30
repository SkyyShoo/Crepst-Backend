using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Backend.Models
{
    public class Extrait
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Auteur { get; set; } = string.Empty;
        public string MaisonEdition { get; set; } = string.Empty;   
        public int NumPages { get; set; }
        public string FileName { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        [ValidateNever]
        public virtual List<Event> Events { get; set; } = null!;


    }
}
