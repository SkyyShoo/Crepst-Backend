namespace Backend.Models.DTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? Author { get; set; }
        public int EventId { get; set; }
    }
}
