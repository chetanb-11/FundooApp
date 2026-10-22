using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public class UserRegistrationReq
{
    [Required, MaxLength(50)]
    public required string Firstname { get; set; }
    [Required, MaxLength(50)]
    public required string Lastname { get; set; }
    [Required, EmailAddress, MaxLength(50)]
    public required string Email { get; set; }
    [Required, MinLength(3), MaxLength(50)]
    public required string Password { get; set; }
    [Required, RegularExpression(@"^[0-9]{10}")]
    public required string ContactNumber { get; set; }
}