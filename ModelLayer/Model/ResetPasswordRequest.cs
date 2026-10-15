using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model;

public sealed record ResetPasswordRequest(
    [Required] string Token,
    [Required] string NewPassword);