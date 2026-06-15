using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Auth;
using backend.Data.Entities;

namespace backend.Services;

public class UserService: IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> Register(RegisterDto dto)
    {
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);
        if (emailTaken)
        {
            return null;
        }

        string hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newUser = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = hashed,
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<bool> ChangeEmail(int userId, ChangeEmailDto dto)
    {
        var user = await _context.Users
            .FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.NewEmail);
        if (emailTaken)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return false;
        }

        user.Email = dto.NewEmail;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users
            .FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return false;
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
        return true;
    }
}