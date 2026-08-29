using System.ComponentModel.DataAnnotations;

namespace PRIV.DTOs
{
    public class CreateBookingRequestDto
    {
        [Required]
        public string TargetUsername { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // matches BookingType enum name

        public string? CustomTypeLabel { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // 1 to 3 proposed locations, in order.
        [Required, MinLength(1), MaxLength(3)]
        public List<string> LocationOptions { get; set; } = new();
    }
}
