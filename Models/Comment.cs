namespace Backend.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime? Date { get; set; }
        public virtual User User { get; set; }
    }
}
