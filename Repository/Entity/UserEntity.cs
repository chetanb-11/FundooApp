using System.ComponentModel.DataAnnotations;

namespace Repository.Entity;

public class UserEntity
{
    [Required] [Key] public int Id { get; set; }


    [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression("^[0-9]{10}$")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required] public byte[]? Password { get; set; }
    [Required] public byte[]? Salt { get; set; }
    public ICollection<NoteEntity> Notes { get; init; } = new List<NoteEntity>();
}