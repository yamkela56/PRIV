using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;

namespace PRIV.Controllers;

[ApiController]
[Route("api/blocked-times")]
[Authorize]
public class BlockedTimesController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public BlockedTimesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<BlockedTimeDto>>> GetMine()
    {
        var items = await _db.BlockedTimes
            .Where(b => b.UserId == CurrentUserId)
            .OrderBy(b => b.SpecificDate)
            .ThenBy(b => b.DayOfWeek)
            .ThenBy(b => b.StartTime)
            .Select(b => new BlockedTimeDto
            {
                Id = b.Id,
                Label = b.Label,
                DayOfWeek = b.DayOfWeek.HasValue ? b.DayOfWeek.Value.ToString() : null,
                SpecificDate = b.SpecificDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<BlockedTimeDto>> Create(CreateBlockedTimeRequest request)
    {
        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(new { message = "End time must be after start time." });
        }

        bool hasDayOfWeek = !string.IsNullOrWhiteSpace(request.DayOfWeek);
        bool hasSpecificDate = request.SpecificDate.HasValue;

        if (hasDayOfWeek == hasSpecificDate)
        {
            return BadRequest(new { message = "Provide exactly one of DayOfWeek or SpecificDate." });
        }

        DayOfWeek? dow = null;
        if (hasDayOfWeek)
        {
            if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var parsed))
            {
                return BadRequest(new { message = "Invalid day of week." });
            }
            dow = parsed;
        }

        var blocked = new BlockedTime
        {
            UserId = CurrentUserId,
            Label = string.IsNullOrWhiteSpace(request.Label) ? "Busy" : request.Label,
            DayOfWeek = dow,
            SpecificDate = request.SpecificDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _db.BlockedTimes.Add(blocked);
        await _db.SaveChangesAsync();

        return Ok(new BlockedTimeDto
        {
            Id = blocked.Id,
            Label = blocked.Label,
            DayOfWeek = blocked.DayOfWeek?.ToString(),
            SpecificDate = blocked.SpecificDate,
            StartTime = blocked.StartTime,
            EndTime = blocked.EndTime
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var blocked = await _db.BlockedTimes.FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
        if (blocked is null) return NotFound();

        _db.BlockedTimes.Remove(blocked);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}