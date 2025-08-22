namespace DailyCheckIn.Models.DTO
{
    public class AdvancePageDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<AdvanceDTO> Advances { get; set; } = [];
    }
}
