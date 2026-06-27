using System.Security.Claims;
using backend.DTOs;
using backend.DTOs.Auth;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController: ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public AuthController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    private void CreateToken(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(1)
        });
        
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(1)
        });
    }

    private async Task ClearToken()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeToken(refreshToken);
        }
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = await _userService.Register(dto);
        if (user == null)
        {
            return BadRequest( new ErrorResponoseDto { StatusCode = 400, Message= "Adres email jest juz zajety" });
        }
        
        var newUser = new LoginDto
        {
            Email = dto.Email,
            Password = dto.Password
        };
        var tokens = await _authService.Login(newUser);
        CreateToken(tokens!.AccessToken, tokens!.RefreshToken);
        return Ok(new { message = "Zarejestrowano pomyslnie" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var tokens = await _authService.Login(dto);
        if (tokens == null)
        {
            return BadRequest(new ErrorResponoseDto{ StatusCode = 400, Message = "Nieprawidlowy email lub haslo" });
        }

        CreateToken(tokens!.AccessToken, tokens!.RefreshToken);

        return Ok(new { message = "Zalogowano pomyslnie" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await ClearToken();
        return Ok(new { message = "Wylogowano pomyslnie" });
    }

    [Authorize]
    [HttpPatch("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString.IsNullOrEmpty() || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }
        
        var success = await _userService.ChangeEmail(userId, dto);
        
        if (success)
        {
            await ClearToken();
            return Ok(new { message = "Zmiana adresu email pomyslna" });
        } 
        else
        {
            return BadRequest( new ErrorResponoseDto { StatusCode = 400, Message = "Nieprawidlowe haslo lub zajety adres email" } );
        }
    }

    [Authorize]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdString.IsNullOrEmpty() || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

        var success = await _userService.ChangePassword(userId, dto);

        if (success)
        {
            await ClearToken();
            return Ok(new { message = "Zmiana adresu email pomyslna" });
        } 
        else
        {
            return BadRequest(new ErrorResponoseDto { StatusCode = 400, Message = "Nieprawidlowe haslo" });
        }
    }
}