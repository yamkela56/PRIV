using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;
using PRIV.Services;

namespace PRIV.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var normalized = request.Username.Trim().ToLowerInvariant();

        bool usernameTaken = await _db.Users.AnyAsync(u => u.UsernameNormalized == normalized);
        if (usernameTaken)
        {
            return Conflict(new { message = "That username is already taken." });
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            bool emailTaken = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (emailTaken)
            {
                return Conflict(new { message = "That email is already registered." });
            }
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Username = request.Username.Trim(),
            UsernameNormalized = normalized,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Name = user.Name
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var identifier = request.UsernameOrEmail.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.UsernameNormalized == identifier || u.Email == request.UsernameOrEmail);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username/email or password." });
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Name = user.Name
        });
    }
}
