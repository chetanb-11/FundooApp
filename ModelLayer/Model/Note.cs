namespace ModelLayer.Model;

public class NoteRequestModel
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}

public class NoteDomainModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
