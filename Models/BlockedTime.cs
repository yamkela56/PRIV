using System.ComponentModel.DataAnnotations;

namespace PRIV.Models
{

    // A manually-entered block that removes availability from the owner's public slots.
    // Supports two modes:
    //  - Recurring weekly block: DayOfWeek is set, SpecificDate is null (e.g. "Every Monday 09:00-11:00").
    //  - One-off block on a specific date: SpecificDate is set (e.g. "Friday 25 Sep 14:00-18:00").
    public class BlockedTime
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string Label { get; set; } = "Busy"; // e.g. "Busy" or "Unavailable" (never shown to other users)

        public DayOfWeek? DayOfWeek { get; set; } // set for recurring weekly blocks
        public DateOnly? SpecificDate { get; set; } // set for one-off blocks

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
