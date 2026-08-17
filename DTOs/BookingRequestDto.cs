using System;

namespace PRIV.DTOs
{
    public class BookingRequestDto
    {
        public int HostId { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location1 { get; set; } = string.Empty;
        public string? Location2 { get; set; }
        public string? Location3 { get; set; }
    }
}