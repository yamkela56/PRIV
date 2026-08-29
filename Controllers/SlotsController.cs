using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;
using PRIV.Services;

namespace PRIV.Controllers;

[ApiController]
[Route("api/slots")]
[Authorize]
public class SlotsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISlotService _slotService;

    public SlotsController(AppDbContext db, ISlotService slotService)
    {
        _db = db;
        _slotService = slotService;
    }

    // GET /api/slots/{targetUsername}? from=2026-08-25& to=2026-08-31
    // Only returns data once the target has approved a connection from the caller.
    [HttpGet("{targetUsername}")]
    public async Task<ActionResult<List<DaySlotsDto>>> GetSlots(
        string targetUsername, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        var normalized = targetUsername.Trim().ToLowerInvariant();
        var target = await _db.Users.FirstOrDefaultAsync(u => u.UsernameNormalized == normalized);
        if (target is null)
        {
            return NotFound(new { message = "No user with that username." });
        }

        var connection = await _db.ConnectionRequests.FirstOrDefaultAsync(c =>
            c.RequesterId == CurrentUserId && c.TargetId == target.Id);

        if (connection is null || connection.Status != ConnectionStatus.Approved)
        {
            return StatusCode(403, new { message = "You must be approved by this user before viewing their availability." });
        }

        if (to < from || (to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).TotalDays > 31)
        {
            return BadRequest(new { message = "Date range must be valid and no more than 31 days." });
        }

        var slots = await _slotService.GetAvailableSlotsAsync(target.Id, from, to);
        return Ok(slots);
    }
}