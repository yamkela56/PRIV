namespace PRIV.DTOs
{
    // Returned to User A once User B has approved the connection.
    // Deliberately contains nothing but Available/Unavailable - no event names,
    // no reasons, no indication of *why* a time is unavailable.
    public class DaySlotsDto
    {
        public DateOnly Date { get; set; }
        public List<SlotDto> Slots { get; set; } = new();
    }
}
