using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public class NoteRequestModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(50, ErrorMessage = "Title cannot exceed 50 characters.")]
    public required string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public required string Description { get; set; } = string.Empty;
}
