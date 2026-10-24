using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public class UserLoginReq
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
