using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;
using System.Security.Claims;

namespace PRIV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires JWT authentication header
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // 1. Search Users (Filtering out non-discoverable users)
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<UserSearchDto>());

            var currentId = CurrentUserId;

            var users = await _context.Users
                .Where(u => u.IsDiscoverable && u.UserId != currentId && u.Username.Contains(query))
                .Select(u => new UserSearchDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    ConnectionStatus = _context.Connections
                        .Where(c => (c.RequesterId == currentId && c.TargetId == u.UserId) ||
                                    (c.RequesterId == u.UserId && c.TargetId == currentId))
                        .Select(c => c.Status)
                        .FirstOrDefault() ?? "None"
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. Fetch Profile by Username (For shareable link: /u/username)
        [HttpGet("u/{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
                return NotFound("User not found.");

            var currentId = CurrentUserId;

            var connectionStatus = await _context.Connections
                .Where(c => (c.RequesterId == currentId && c.TargetId == targetUser.UserId) ||
                            (c.RequesterId == targetUser.UserId && c.TargetId == currentId))
                .Select(c => c.Status)
                .FirstOrDefaultAsync() ?? "None";

            return Ok(new
            {
                targetUser.UserId,
                targetUser.Username,
                ConnectionStatus = connectionStatus
            });
        }

        // 3. Toggle Discovery Settings (Allow user to hide/show themselves in search)
        [HttpPost("toggle-discovery")]
        public async Task<IActionResult> ToggleDiscovery([FromBody] bool isDiscoverable)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            user.IsDiscoverable = isDiscoverable;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Discoverability updated to {isDiscoverable}", user.IsDiscoverable });
        }
    }
}