using DailyCheckIn.Enums;

namespace DailyCheckIn.Models.DTO
{
    public class CheckInModelDTO
    {
        public TimeOnly TimeNow { get; set; } = TimeOnly.FromDateTime(DateTime.Now);
        public UserCheckInStatusEnum UserStatus { get; set; }
        public CheckInModelDTO() { }
        public CheckInModelDTO(TimeOnly TimeNow, UserCheckInStatusEnum UserStatus) 
        {
            this.TimeNow = TimeNow;
            this.UserStatus = UserStatus;
        }
    }
}
