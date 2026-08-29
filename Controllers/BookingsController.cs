using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;

namespace PRIV.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequestDto request)
    {
        if (!Enum.TryParse<BookingType>(request.Type, true, out var type))
        {
            return BadRequest(new { message = "Invalid booking type." });
        }

        if (type == BookingType.Custom && string.IsNullOrWhiteSpace(request.CustomTypeLabel))
        {
            return BadRequest(new { message = "CustomTypeLabel is required when Type is Custom." });
        }

        if (request.LocationOptions.Count is < 1 or > 3)
        {
            return BadRequest(new { message = "Provide between 1 and 3 location options." });
        }

        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(new { message = "End time must be after start time." });
        }

        var normalized = request.TargetUsername.Trim().TrimStart('@').ToLowerInvariant();
        var target = await _db.Users.FirstOrDefaultAsync(u => u.UsernameNormalized == normalized);
        if (target is null)
        {
            return NotFound(new { message = "No user with that username." });
        }

        if (target.Id == CurrentUserId)
        {
            return BadRequest(new { message = "You can't book yourself." });
        }

        var connection = await _db.ConnectionRequests.FirstOrDefaultAsync(c =>
            c.RequesterId == CurrentUserId && c.TargetId == target.Id);

        if (connection is null || connection.Status != ConnectionStatus.Approved)
        {
            return StatusCode(403, new { message = "You must be approved by this user before requesting a booking." });
        }

        var booking = new BookingRequest
        {
            RequesterId = CurrentUserId,
            TargetId = target.Id,
            Type = type,
            CustomTypeLabel = type == BookingType.Custom ? request.CustomTypeLabel!.Trim() : null,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = BookingStatus.Pending
        };

        for (int i = 0; i < request.LocationOptions.Count; i++)
        {
            booking.LocationOptions.Add(new BookingLocationOption
            {
                OptionNumber = i + 1,
                Name = request.LocationOptions[i].Trim()
            });
        }

        _db.BookingRequests.Add(booking);
        await _db.SaveChangesAsync();

        return Ok(await ToDto(booking.Id));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, ApproveBookingRequestDto request)
    {
        var booking = await _db.BookingRequests
            .Include(b => b.LocationOptions)
            .FirstOrDefaultAsync(b => b.Id == id && b.TargetId == CurrentUserId);

        if (booking is null) return NotFound();
        if (booking.Status != BookingStatus.Pending)
        {
            return BadRequest(new { message = "This booking has already been responded to." });
        }

        var chosen = booking.LocationOptions.FirstOrDefault(o => o.Id == request.SelectedLocationOptionId);
        if (chosen is null)
        {
            return BadRequest(new { message = "Selected location is not one of the proposed options." });
        }

        booking.Status = BookingStatus.Approved;
        booking.ConfirmedLocationOptionId = chosen.Id;
        booking.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(await ToDto(booking.Id));
    }

    [HttpPost("{id}/decline")]
    public async Task<IActionResult> Decline(int id, DeclineBookingRequestDto request)
    {
        var booking = await _db.BookingRequests
            .FirstOrDefaultAsync(b => b.Id == id && b.TargetId == CurrentUserId);

        if (booking is null) return NotFound();
        if (booking.Status != BookingStatus.Pending)
        {
            return BadRequest(new { message = "This booking has already been responded to." });
        }

        booking.Status = BookingStatus.Declined;
        booking.DeclineReason = request.Reason.Trim();
        booking.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(await ToDto(booking.Id));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancelBookingRequestDto request)
    {
        var booking = await _db.BookingRequests
            .FirstOrDefaultAsync(b => b.Id == id && (b.RequesterId == CurrentUserId || b.TargetId == CurrentUserId));

        if (booking is null) return NotFound();
        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Declined or BookingStatus.Completed)
        {
            return BadRequest(new { message = "This booking can no longer be cancelled." });
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelReason = request.Reason.Trim();
        booking.CancelledByUserId = CurrentUserId;
        booking.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(await ToDto(booking.Id));
    }

    // GET /api/bookings/mine?status=Approved  (status optional)
    [HttpGet("mine")]
    public async Task<ActionResult<List<BookingDto>>> Mine([FromQuery] string? status)
    {
        var query = _db.BookingRequests
            .Include(b => b.Requester)
            .Include(b => b.Target)
            .Include(b => b.LocationOptions)
            .Include(b => b.ConfirmedLocationOption)
            .Where(b => b.RequesterId == CurrentUserId || b.TargetId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(b => b.Status == parsedStatus);
        }

        var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return Ok(bookings.Select(ToDto).ToList());
    }

    private async Task<BookingDto> ToDto(int bookingId)
    {
        var booking = await _db.BookingRequests
            .Include(b => b.Requester)
            .Include(b => b.Target)
            .Include(b => b.LocationOptions)
            .Include(b => b.ConfirmedLocationOption)
            .FirstAsync(b => b.Id == bookingId);

        return ToDto(booking);
    }

    private BookingDto ToDto(BookingRequest booking)
    {
        bool iAmRequester = booking.RequesterId == CurrentUserId;

        return new BookingDto
        {
            Id = booking.Id,
            Type = booking.Type.ToString(),
            CustomTypeLabel = booking.CustomTypeLabel,
            Date = booking.Date,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status.ToString(),
            RequesterId = booking.RequesterId,
            RequesterUsername = booking.Requester.Username,
            TargetId = booking.TargetId,
            TargetUsername = booking.Target.Username,
            OtherUsername = iAmRequester ? booking.Target.Username : booking.Requester.Username,
            Direction = iAmRequester ? "Outgoing" : "Incoming",
            LocationOptions = booking.LocationOptions
                .OrderBy(o => o.OptionNumber)
                .Select(o => new LocationOptionDto { Id = o.Id, OptionNumber = o.OptionNumber, Name = o.Name })
                .ToList(),
            ConfirmedLocation = booking.ConfirmedLocationOption is null ? null : new LocationOptionDto
            {
                Id = booking.ConfirmedLocationOption.Id,
                OptionNumber = booking.ConfirmedLocationOption.OptionNumber,
                Name = booking.ConfirmedLocationOption.Name
            },
            DeclineReason = booking.DeclineReason,
            CancelReason = booking.CancelReason,
            CreatedAt = booking.CreatedAt
        };
    }
}