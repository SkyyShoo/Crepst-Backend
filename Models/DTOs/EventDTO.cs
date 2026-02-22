namespace Backend.Models.DTOs;

public class EventDTO
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public DateTime DateFinExtrait { get; set; }
    public string Lieu { get; set; }
    public string Resumer  { get; set; }
    public string Thematique { get; set; }
}