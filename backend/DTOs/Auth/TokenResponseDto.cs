using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public class TokenResponseDto
{
    [Required]
    public string AccessToken = string.Empty;

    [Required]
    public string RefreshToken = string.Empty;
}