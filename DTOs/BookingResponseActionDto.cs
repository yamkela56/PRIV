namespace PRIV.DTOs
{
    public class BookingResponseActionDto
    {
        public int BookingId { get; set; }

        // 'Approved', 'Declined', or 'Cancelled'
        public string Action { get; set; } = string.Empty;

        // Mandatory when Action == 'Approved'
        public string? SelectedLocation { get; set; }

        // Mandatory when Action == 'Declined' or 'Cancelled'
        public string? Reason { get; set; }
    }
}