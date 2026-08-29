namespace PRIV.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? CustomTypeLabel { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;

        public int RequesterId { get; set; }
        public string RequesterUsername { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string TargetUsername { get; set; } = string.Empty;

        // Whichever user is NOT the current caller - convenience field for the UI.
        public string OtherUsername { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty; // "Incoming" | "Outgoing"

        public List<LocationOptionDto> LocationOptions { get; set; } = new();
        public LocationOptionDto? ConfirmedLocation { get; set; }

        public string? DeclineReason { get; set; }
        public string? CancelReason { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
