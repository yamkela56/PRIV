using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;

namespace PRIV.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/users/search?q=yamkela
    // Returns Name, Username, and a Request Access status only - never calendar data.
    [HttpGet("search")]
    public async Task<ActionResult<List<UserSearchResultDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<UserSearchResultDto>());
        }

        var query = q.Trim().TrimStart('@').ToLowerInvariant();

        var users = await _db.Users
            .Where(u => u.DiscoverableInSearch
                        && u.Id != CurrentUserId
                        && u.UsernameNormalized.Contains(query))
            .OrderBy(u => u.UsernameNormalized)
            .Take(25)
            .ToListAsync();

        var connections = await _db.ConnectionRequests
            .Where(c => c.RequesterId == CurrentUserId && users.Select(u => u.Id).Contains(c.TargetId))
            .ToListAsync();

        var results = users.Select(u => new UserSearchResultDto
        {
            Id = u.Id,
            Name = u.Name,
            Username = u.Username,
            ConnectionStatus = connections.FirstOrDefault(c => c.TargetId == u.Id)?.Status.ToString() ?? "None"
        }).ToList();

        return Ok(results);
    }

    // GET /api/users/u/{username}  -> backs the /u/username profile page
    [HttpGet("u/{username}")]
    public async Task<ActionResult<ProfileDto>> GetProfile(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UsernameNormalized == normalized);
        if (user is null)
        {
            return NotFound(new { message = "No user with that username." });
        }

        var connection = await _db.ConnectionRequests
            .FirstOrDefaultAsync(c => c.RequesterId == CurrentUserId && c.TargetId == user.Id);

        return Ok(new ProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Bio = user.Bio,
            IsSelf = user.Id == CurrentUserId,
            ConnectionStatus = user.Id == CurrentUserId ? "Self" : (connection?.Status.ToString() ?? "None"),
            DiscoverableInSearch = user.Id == CurrentUserId ? user.DiscoverableInSearch : null
        });
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name)) user.Name = request.Name.Trim();
        user.Bio = request.Bio;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("me/privacy")]
    public async Task<IActionResult> UpdatePrivacy(UpdatePrivacyRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.DiscoverableInSearch = request.DiscoverableInSearch;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("me/username")]
    public async Task<IActionResult> UpdateUsername(UpdateUsernameRequest request)
    {
        var normalized = request.NewUsername.Trim().ToLowerInvariant();

        bool taken = await _db.Users.AnyAsync(u => u.UsernameNormalized == normalized && u.Id != CurrentUserId);
        if (taken)
        {
            return Conflict(new { message = "That username is already taken." });
        }

        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.Username = request.NewUsername.Trim();
        user.UsernameNormalized = normalized;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}