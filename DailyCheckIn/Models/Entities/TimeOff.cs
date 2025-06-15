using DailyCheckIn.Models.Entities.Base;

namespace DailyCheckIn.Models.Entities
{
    public class TimeOff : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid AttendanceDayId { get; set; }
        public required Attendance AttendanceDay { get; set; }
        public required TimeOnly TimeOffStart { get; set; }
        public TimeOnly? TimeOffEnd { get; set; }
    }
}
