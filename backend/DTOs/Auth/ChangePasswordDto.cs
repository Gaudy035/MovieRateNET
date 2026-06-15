using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public class ChangePasswordDto
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}