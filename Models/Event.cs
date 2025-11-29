namespace Backend.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Lieu { get; set; } = string.Empty;
        public virtual List<User> Participants { get; set; } = null!; 
        public virtual List<Livre> livres { get; set; } = null!;
    }
}
