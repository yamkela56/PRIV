using System;

namespace PRIV.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int RequesterId { get; set; }
        public int HostId { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Location1 { get; set; } = string.Empty;
        public string? Location2 { get; set; }
        public string? Location3 { get; set; }
        public string? ConfirmedLocation { get; set; }

        public string Status { get; set; } = "Pending";
        public string? DeclineCancelReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? Requester { get; set; }
        public User? Host { get; set; }
    }
}