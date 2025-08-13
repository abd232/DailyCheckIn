namespace DailyCheckIn.Models.DTO
{
    public class AttendanceSeedDTO
    {
        public DateOnly Date { get; set; }
        public TimeOnly CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public Guid UserId { get; set; }
        public List<TimeOffDTO> TimeOffs { get; set; } = [];
    }
}
