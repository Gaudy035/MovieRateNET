using backend.Data.Entities;
using backend.DTOs.Auth;

namespace backend.Services;
public interface IUserService
{
    Task<User?> Register(RegisterDto dto);

    Task<bool> ChangeEmail(int userId, ChangeEmailDto dto);

    Task<bool> ChangePassword(int userId, ChangePasswordDto dto);
}