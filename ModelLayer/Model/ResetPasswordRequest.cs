using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public sealed record ResetPasswordRequest(
    [Required] string Token,
    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long."), StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")] string NewPassword);
