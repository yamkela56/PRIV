using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PRIV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username is already taken.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email is already registered.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                IsDiscoverable = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = CreateToken(user);
            return Ok(new AuthResponseDto { UserId = user.UserId, Username = user.Username, Token = token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password.");

            var token = CreateToken(user);
            return Ok(new AuthResponseDto { UserId = user.UserId, Username = user.Username, Token = token });
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}