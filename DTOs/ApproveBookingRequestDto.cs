using System.ComponentModel.DataAnnotations;

namespace PRIV.DTOs
{
    public class ApproveBookingRequestDto
    {
        [Required]
        public int SelectedLocationOptionId { get; set; }
    }
}
