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

    private void ClearToken()
    {
        Response.Cookies.Delete("access_token");
    }

    private void CreateToken(string token)
    {
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(1)
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = await _userService.Register(dto);
        if (user == null)
        {
            return BadRequest( new ErrorResponoseDto { StatusCode = 400, Message= "Adres email jest juz zajety" });
        }
        else
        {
            var newUser = new LoginDto
            {
                Email = dto.Email,
                Password = dto.Password
            };
            var token = await _authService.Login(newUser);
            CreateToken(token!);
            return Ok(new { message = "Zarejestrowano pomyslnie" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.Login(dto);
        if (token == null)
        {
            return BadRequest(new ErrorResponoseDto{ StatusCode = 400, Message = "Nieprawidlowy email lub haslo" });
        }

        CreateToken(token);

        return Ok(new { message = "Zalogowano pomyslnie" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        ClearToken();
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
            ClearToken();
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
            ClearToken();
            return Ok(new { message = "Zmiana adresu email pomyslna" });
        } 
        else
        {
            return BadRequest(new ErrorResponoseDto { StatusCode = 400, Message = "Nieprawidlowe haslo" });
        }
    }
}