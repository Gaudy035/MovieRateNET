using backend.DTOs.Auth;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend.Data.Entities;
using System.Security.Cryptography;

namespace backend.Services;

public class AuthService: IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    private string GenerateAccessToken(int userId, string userEmail)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var keyStr = _configuration["Jwt:Key"];
        var key = Encoding.UTF8.GetBytes(keyStr!);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, userEmail)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshToken(int userId)
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var tokenValue = Convert.ToBase64String(randomBytes);

        var newRefreshToken = new RefreshToken
        {
            UserId = userId,
            TokenValue = tokenValue,
            IsActive = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();
        return newRefreshToken;
    }

    public async Task<bool> RevokeToken(string refreshTokenValue)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == refreshTokenValue);
        if(refreshToken == null)
        {
            return false;
        }
        
        refreshToken.IsActive = false;
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LoginResponseDto?> Refresh(string refreshTokenValue)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenValue == refreshTokenValue);

        
        if (refreshToken == null || !refreshToken.IsActive)
        {
            return null;
        }
        
        await RevokeToken(refreshTokenValue);

        if (refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }
        
        var newAccessToken = GenerateAccessToken(refreshToken.UserId, refreshToken.User.Email);
        var newRefreshToken = await GenerateRefreshToken(refreshToken.UserId);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.TokenValue
        };
    }

    public async Task<LoginResponseDto?> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            return null;
        }

        if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return null;
        }

        var accessToken = GenerateAccessToken(user.UserId, user.Email);
        var refreshToken = await GenerateRefreshToken(user.UserId);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.TokenValue
        };
    }
}