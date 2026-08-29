namespace PRIV.DTOs
{
    public class BlockedTimeDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "Busy";
        public string? DayOfWeek { get; set; } // e.g. "Monday", null if SpecificDate is set
        public DateOnly? SpecificDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
