using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;

namespace PRIV.Controllers;

[ApiController]
[Route("api/connections")]
[Authorize]
public class ConnectionsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public ConnectionsController(AppDbContext db)
    {
        _db = db;
    }

    // Incoming = I approve/decline these.
    [HttpGet("incoming")]
    public async Task<ActionResult<List<ConnectionRequestDto>>> Incoming()
    {
        var items = await _db.ConnectionRequests
            .Include(c => c.Requester)
            .Where(c => c.TargetId == CurrentUserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectionRequestDto
            {
                Id = c.Id,
                OtherUserId = c.RequesterId,
                OtherUsername = c.Requester.Username,
                OtherName = c.Requester.Name,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    // Outgoing = requests I sent to other users.
    [HttpGet("outgoing")]
    public async Task<ActionResult<List<ConnectionRequestDto>>> Outgoing()
    {
        var items = await _db.ConnectionRequests
            .Include(c => c.Target)
            .Where(c => c.RequesterId == CurrentUserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectionRequestDto
            {
                Id = c.Id,
                OtherUserId = c.TargetId,
                OtherUsername = c.Target.Username,
                OtherName = c.Target.Name,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("request")]
    public async Task<ActionResult<ConnectionRequestDto>> SendRequest(SendConnectionRequestDto request)
    {
        var normalized = request.TargetUsername.Trim().TrimStart('@').ToLowerInvariant();
        var target = await _db.Users.FirstOrDefaultAsync(u => u.UsernameNormalized == normalized);
        if (target is null)
        {
            return NotFound(new { message = "No user with that username." });
        }

        if (target.Id == CurrentUserId)
        {
            return BadRequest(new { message = "You can't request access to your own schedule." });
        }

        var existing = await _db.ConnectionRequests
            .FirstOrDefaultAsync(c => c.RequesterId == CurrentUserId && c.TargetId == target.Id);

        if (existing is not null)
        {
            if (existing.Status == ConnectionStatus.Declined)
            {
                // Allow re-requesting after a decline.
                existing.Status = ConnectionStatus.Pending;
                existing.CreatedAt = DateTime.UtcNow;
                existing.RespondedAt = null;
                await _db.SaveChangesAsync();
            }
            return Ok(new ConnectionRequestDto
            {
                Id = existing.Id,
                OtherUserId = target.Id,
                OtherUsername = target.Username,
                OtherName = target.Name,
                Status = existing.Status.ToString(),
                CreatedAt = existing.CreatedAt
            });
        }

        var connection = new ConnectionRequest
        {
            RequesterId = CurrentUserId,
            TargetId = target.Id,
            Status = ConnectionStatus.Pending
        };
        _db.ConnectionRequests.Add(connection);
        await _db.SaveChangesAsync();

        return Ok(new ConnectionRequestDto
        {
            Id = connection.Id,
            OtherUserId = target.Id,
            OtherUsername = target.Username,
            OtherName = target.Name,
            Status = connection.Status.ToString(),
            CreatedAt = connection.CreatedAt
        });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var connection = await _db.ConnectionRequests
            .FirstOrDefaultAsync(c => c.Id == id && c.TargetId == CurrentUserId);

        if (connection is null) return NotFound();
        if (connection.Status != ConnectionStatus.Pending)
        {
            return BadRequest(new { message = "This request has already been responded to." });
        }

        connection.Status = ConnectionStatus.Approved;
        connection.RespondedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/decline")]
    public async Task<IActionResult> Decline(int id)
    {
        var connection = await _db.ConnectionRequests
            .FirstOrDefaultAsync(c => c.Id == id && c.TargetId == CurrentUserId);

        if (connection is null) return NotFound();
        if (connection.Status != ConnectionStatus.Pending)
        {
            return BadRequest(new { message = "This request has already been responded to." });
        }

        connection.Status = ConnectionStatus.Declined;
        connection.RespondedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}