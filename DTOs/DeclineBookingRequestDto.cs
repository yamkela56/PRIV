using System.ComponentModel.DataAnnotations;

namespace PRIV.DTOs
{
    public class DeclineBookingRequestDto
    {
        [Required, MinLength(1), MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
