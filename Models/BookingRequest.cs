using System.ComponentModel.DataAnnotations;

namespace PRIV.Models
{

    public class BookingRequest
    {
        public int Id { get; set; }

        public int RequesterId { get; set; } // User A
        public User Requester { get; set; } = null!;

        public int TargetId { get; set; } // User B
        public User Target { get; set; } = null!;

        public BookingType Type { get; set; }

        [MaxLength(100)]
        public string? CustomTypeLabel { get; set; } // used when Type == Custom

        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public ICollection<BookingLocationOption> LocationOptions { get; set; } = new List<BookingLocationOption>();

        public int? ConfirmedLocationOptionId { get; set; }
        public BookingLocationOption? ConfirmedLocationOption { get; set; }

        [MaxLength(500)]
        public string? DeclineReason { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        public int? CancelledByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}
