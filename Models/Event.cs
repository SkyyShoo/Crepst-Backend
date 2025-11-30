namespace Backend.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Resumer { get; set; } = string.Empty;
        public string Lieu { get; set; } = string.Empty;
        public virtual List<User>? Participants { get; set; } = null!; 
        public virtual List<Extrait> Extraits { get; set; } = null!;
    }
}
