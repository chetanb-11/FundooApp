namespace Repository.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Repository.Entity;

public class NoteEntity
{
    [Key] public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))] public UserEntity User { get; set; } = null!;

    [Required, MaxLength(50)] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsTrashed { get; set; }
}