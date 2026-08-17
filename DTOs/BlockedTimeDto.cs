using System;

namespace PRIV.DTOs
{
    public class BlockedTimeDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Note { get; set; }
    }
}