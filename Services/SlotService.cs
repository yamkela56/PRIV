using PRIV.Data;
using PRIV.DTOs;
using PRIV.Models;
using Microsoft.EntityFrameworkCore;

namespace PRIV.Services
{
    public class SlotService : ISlotService
    {
        private readonly AppDbContext _db;

        public SlotService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<DaySlotsDto>> GetAvailableSlotsAsync(int targetUserId, DateOnly fromDate, DateOnly toDate)
        {
            var user = await _db.Users.FindAsync(targetUserId)
                ?? throw new KeyNotFoundException("User not found.");

            if (toDate < fromDate)
            {
                (fromDate, toDate) = (toDate, fromDate);
            }

            var blockedTimes = await _db.BlockedTimes
                .Where(b => b.UserId == targetUserId)
                .ToListAsync();

            // Any booking that currently occupies a slot on this user's calendar (either as requester or target), and is not declined/cancelled.
            var busyBookings = await _db.BookingRequests
                .Where(b => (b.RequesterId == targetUserId || b.TargetId == targetUserId)
                            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved)
                            && b.Date >= fromDate && b.Date <= toDate)
                .Select(b => new { b.Date, b.StartTime, b.EndTime })
                .ToListAsync();

            var result = new List<DaySlotsDto>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var daySlots = new DaySlotsDto { Date = date };

                var increment = TimeSpan.FromMinutes(Math.Max(user.SlotIncrementMinutes, 15));
                for (var t = user.WorkDayStart; t + increment <= user.WorkDayEnd; t += increment)
                {
                    var slotStart = t;
                    var slotEnd = t + increment;

                    bool blockedByManualEntry = blockedTimes.Any(b =>
                        Overlaps(slotStart, slotEnd, b.StartTime, b.EndTime) &&
                        ((b.SpecificDate.HasValue && b.SpecificDate.Value == date) ||
                         (!b.SpecificDate.HasValue && b.DayOfWeek.HasValue &&
                          b.DayOfWeek.Value == date.DayOfWeek)));

                    bool blockedByBooking = busyBookings.Any(b =>
                        b.Date == date && Overlaps(slotStart, slotEnd, b.StartTime, b.EndTime));

                    daySlots.Slots.Add(new SlotDto
                    {
                        StartTime = slotStart,
                        EndTime = slotEnd,
                        Available = !blockedByManualEntry && !blockedByBooking
                    });
                }

                result.Add(daySlots);
            }

            return result;
        }

        private static bool Overlaps(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd)
        {
            return aStart < bEnd && bStart < aEnd;
        }
    }
}
