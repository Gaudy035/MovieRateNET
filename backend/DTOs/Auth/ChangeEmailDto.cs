using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public class ChangeEmailDto
{
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}