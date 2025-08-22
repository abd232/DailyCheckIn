namespace DailyCheckIn.Models.DTO
{
    public class AdvanceDTO
    {
        public required Guid Id { get; set; }
        public required int Amount { get; set; }
        public required DateOnly Date { get; set; }
    }
}
