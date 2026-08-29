namespace PRIV.DTOs
{
    public class CreateBlockedTimeRequest
    {
        public string? DayOfWeek { get; set; } // one of Monday..Sunday, mutually exclusive with SpecificDate
        public DateOnly? SpecificDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Label { get; set; } = "Busy";
    }
}
