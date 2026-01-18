using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Extrait
    {
        public int Id { get; set; }
        public string Auteur { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string Chapitre { get; set; } = string.Empty;
        public string Traduction { get; set; } = string.Empty;
        public int? AnneeParution { get; set; } = null!;
        public string Edition { get; set; } = string.Empty;   
        public string NumPages { get; set; }
        public string FileName { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public int EventId { get; set; }
        [ValidateNever]
        public virtual Event Event { get; set;  } = null!;
        public string OwnerName { get; set; } = null!;
        [JsonIgnore]
        public virtual User User { get; set; } = null!;


    }
}
