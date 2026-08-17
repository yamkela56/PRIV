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
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // 1. GET COMPUTED AVAILABLE SLOTS (Privacy Core)
        [HttpGet("availability/{hostId}")]
        public async Task<IActionResult> GetAvailableSlots(int hostId, [FromQuery] DateTime date)
        {
            var currentUserId = CurrentUserId;

            // Check if connection is approved
            var isConnected = await _context.Connections.AnyAsync(c =>
                ((c.RequesterId == currentUserId && c.TargetId == hostId) ||
                 (c.RequesterId == hostId && c.TargetId == currentUserId)) &&
                c.Status == "Approved");

            if (!isConnected)
            {
                return StatusCode(403, "You must be an approved connection to view availability.");
            }

            // Define standard operating window for requested date (e.g., 08:00 to 18:00)
            var startOfDay = date.Date.AddHours(8);
            var endOfDay = date.Date.AddHours(18);

            // Fetch busy intervals from BlockedTimes
            var blockedTimes = await _context.BlockedTimes
                .Where(b => b.UserId == hostId && b.EndTime > startOfDay && b.StartTime < endOfDay)
                .Select(b => new { b.StartTime, b.EndTime })
                .ToListAsync();

            // Fetch busy intervals from Active Bookings (Pending or Accepted)
            var activeBookings = await _context.Bookings
                .Where(b => b.HostId == hostId &&
                            (b.Status == "Pending" || b.Status == "Accepted") &&
                            b.EndTime > startOfDay && b.StartTime < endOfDay)
                .Select(b => new { b.StartTime, b.EndTime })
                .ToListAsync();

            // Combine all busy intervals
            var busySlots = blockedTimes.Concat(activeBookings)
                .OrderBy(b => b.StartTime)
                .ToList();

            // Compute Free 1-Hour Slots
            var freeSlots = new List<object>();
            var currentTime = startOfDay;

            while (currentTime.AddHours(1) <= endOfDay)
            {
                var slotEnd = currentTime.AddHours(1);

                // Check overlap with any busy slot
                bool isBusy = busySlots.Any(b => currentTime < b.EndTime && slotEnd > b.StartTime);

                if (!isBusy)
                {
                    freeSlots.Add(new
                    {
                        StartTime = currentTime,
                        EndTime = slotEnd
                    });
                }

                currentTime = currentTime.AddHours(1);
            }

            // Returns ONLY available slots without revealing reasons or calendar contents
            return Ok(freeSlots);
        }

        // 2. REQUEST A BOOKING
        [HttpPost("request")]
        public async Task<IActionResult> RequestBooking([FromBody] BookingRequestDto dto)
        {
            var requesterId = CurrentUserId;

            // Verify connection approval
            var isConnected = await _context.Connections.AnyAsync(c =>
                ((c.RequesterId == requesterId && c.TargetId == dto.HostId) ||
                 (c.RequesterId == dto.HostId && c.TargetId == requesterId)) &&
                c.Status == "Approved");

            if (!isConnected)
                return StatusCode(403, "You can only request bookings from approved connections.");

            var booking = new Booking
            {
                RequesterId = requesterId,
                HostId = dto.HostId,
                BookingType = dto.BookingType,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok("Booking request sent successfully.");
        }

        // 3. RESPOND OR CANCEL BOOKING (Accept / Decline / Cancel)
        [HttpPost("respond")]
        public async Task<IActionResult> RespondToBooking([FromBody] BookingResponseActionDto dto)
        {
            var userId = CurrentUserId;
            var booking = await _context.Bookings.FindAsync(dto.BookingId);

            if (booking == null) return NotFound("Booking request not found.");

            if (dto.Action == "Approved")
            {
                if (string.IsNullOrWhiteSpace(dto.SelectedLocation))
                    return BadRequest("You must select one location option to approve.");

                booking.ConfirmedLocation = dto.SelectedLocation;
                booking.Status = "Approved";
            }
            else if (dto.Action == "Declined" || dto.Action == "Cancelled")
            {
                if (string.IsNullOrWhiteSpace(dto.Reason))
                    return BadRequest("A reason is required when declining or cancelling.");

                booking.DeclineCancelReason = dto.Reason;
                booking.Status = dto.Action;
            }

            await _context.SaveChangesAsync();
            return Ok($"Booking status updated to {dto.Action}.");
        }

        // 4. GET CONFIRMED / PENDING BOOKINGS FOR CURRENT USER
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userId = CurrentUserId;

            var bookings = await _context.Bookings
                .Where(b => b.RequesterId == userId || b.HostId == userId)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingType,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.DeclineCancelReason,
                    Requester = b.Requester!.Username,
                    Host = b.Host!.Username,
                    IsHost = b.HostId == userId
                })
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            return Ok(bookings);
        }

        // 5. ADD MANUAL BLOCKED TIME (By Host)
        [HttpPost("block-time")]
        public async Task<IActionResult> BlockTime([FromBody] BlockedTimeDto dto)
        {
            var userId = CurrentUserId;

            var blocked = new BlockedTime
            {
                UserId = userId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Note = dto.Note
            };

            _context.BlockedTimes.Add(blocked);
            await _context.SaveChangesAsync();

            return Ok("Time blocked successfully.");
        }
    }
}