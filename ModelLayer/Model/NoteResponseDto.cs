namespace ModelLayer.Model;

public class NoteResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsTrashed { get; set; }
}
