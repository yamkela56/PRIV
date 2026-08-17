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
    [Authorize]
    public class ConnectionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConnectionsController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // 1. Send Connection Request (User A -> User B)
        [HttpPost("request/{targetUserId}")]
        public async Task<IActionResult> RequestConnection(int targetUserId)
        {
            var requesterId = CurrentUserId;

            if (requesterId == targetUserId)
                return BadRequest("You cannot send a connection request to yourself.");

            var targetExists = await _context.Users.AnyAsync(u => u.UserId == targetUserId);
            if (!targetExists)
                return NotFound("Target user not found.");

            var existingConnection = await _context.Connections
                .FirstOrDefaultAsync(c => c.RequesterId == requesterId && c.TargetId == targetUserId);

            if (existingConnection != null)
                return BadRequest($"Connection already exists with status: {existingConnection.Status}");

            var connection = new Connection
            {
                RequesterId = requesterId,
                TargetId = targetUserId,
                Status = "Pending"
            };

            _context.Connections.Add(connection);
            await _context.SaveChangesAsync();

            return Ok("Connection request sent successfully.");
        }

        // 2. Get Pending Requests (For User B to approve/decline)
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var userId = CurrentUserId;

            var pending = await _context.Connections
                .Where(c => c.TargetId == userId && c.Status == "Pending")
                .Select(c => new
                {
                    c.ConnectionId,
                    c.RequesterId,
                    RequesterUsername = c.Requester!.Username,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(pending);
        }

        // 3. Approve or Decline Request
        [HttpPost("respond")]
        public async Task<IActionResult> RespondToRequest([FromBody] ConnectionResponseDto dto)
        {
            var userId = CurrentUserId;

            var connection = await _context.Connections.FirstOrDefaultAsync(c => c.ConnectionId == dto.ConnectionId && c.TargetId == userId);
            if (connection == null)
                return NotFound("Connection request not found.");

            if (dto.Action != "Approved" && dto.Action != "Declined")
                return BadRequest("Action must be 'Approved' or 'Declined'.");

            connection.Status = dto.Action;
            await _context.SaveChangesAsync();

            return Ok($"Connection {dto.Action.ToLower()} successfully.");
        }
    }
}