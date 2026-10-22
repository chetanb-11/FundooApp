namespace ModelLayer.Model;

public class NoteRequestModel
{
    public required string Title { get; set; } = string.Empty;
    public required string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsTrashed { get; set; }
}

public class NoteDomainModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsTrashed { get; set; }
}
public class NoteResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsTrashed { get; set; }
}
