using backend.DTOs.Auth;

namespace backend.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> Login(LoginDto dto);
    Task<bool> RevokeToken(string refreshTokenValue);
}