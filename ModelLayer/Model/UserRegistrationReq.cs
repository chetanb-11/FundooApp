using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public class UserRegistrationReq
{
    [Required, StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public required string Firstname { get; set; }

    [Required, StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public required string Lastname { get; set; }

    [Required, EmailAddress, StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
    public required string Email { get; set; }

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long."), StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
    public required string Password { get; set; }

    [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
    public required string ContactNumber { get; set; }
}
