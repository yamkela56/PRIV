using PRIV.DTOs;

namespace PRIV.Services
{
    public interface ISlotService
    {
        // Computes Available/Unavailable slots for `targetUserId` across the given date range. Never returns event names or the reason a slot is unavailable
        Task<List<DaySlotsDto>> GetAvailableSlotsAsync(int targetUserId, DateOnly fromDate, DateOnly toDate);
    }
}
