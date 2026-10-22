using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}