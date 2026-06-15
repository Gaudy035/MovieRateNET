using backend.DTOs.Auth;

namespace backend.Services;

public interface IAuthService
{
    Task<string?> Login(LoginDto dto);
}