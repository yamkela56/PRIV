using System.ComponentModel.DataAnnotations;

namespace PRIV.Models
{
    public class BookingLocationOption
    {
        public int Id { get; set; }

        public int BookingRequestId { get; set; }
        public BookingRequest BookingRequest { get; set; } = null!;

        // 1, 2, or 3 - the order User A proposed the location in.
        public int OptionNumber { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}
