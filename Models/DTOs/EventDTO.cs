namespace Backend.Models.DTOs;

public class EventDTO
{
    public int? Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DateFinExtrait { get; set; }
    public string Lieu { get; set; } = string.Empty;
    public string Resumer  { get; set; } = string.Empty;
    public string Thematique { get; set; } = string.Empty;
}