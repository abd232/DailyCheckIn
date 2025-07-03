using DailyCheckIn.Models.Entities.Base;
using System.Threading;

namespace DailyCheckIn.Models.Entities
{
    public class Attendance : AuditableEntity
    {
        public Guid Id { get; set; }
        public required DateOnly Date { get; set; }
        public required TimeOnly? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public required Guid UserId { get; set; }
        public required AppUser User { get; set; }
        public ICollection<TimeOff> TimeOffsForTheDay { get; set; } = [];
    }
}
