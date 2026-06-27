using backend.DTOs.Auth;

namespace backend.Services;

public interface IAuthService
{
    Task<bool> RevokeToken(string refreshTokenValue);
    Task<LoginResponseDto?> Refresh(string refreshTokenValue);
    Task<LoginResponseDto?> Login(LoginDto dto);
}