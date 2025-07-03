using DailyCheckIn.Enums;

namespace DailyCheckIn.Models.DTO
{
    public class CheckInModelDTO
    {
        public TimeOnly TimeNow { get; set; } = TimeOnly.FromDateTime(DateTime.Now);
        public UserCheckInStatusEnum UserStatus { get; set; }
    }
}
