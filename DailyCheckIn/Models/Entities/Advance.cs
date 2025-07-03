using DailyCheckIn.Enums;
using DailyCheckIn.Models.Entities.Base;

namespace DailyCheckIn.Models.Entities
{
    public class Advance : AuditableEntity
    {
        public required Guid Id { get; set; }
        public required int Amount { get; set; }
        public required DateOnly Date { get; set; }
        public required Guid UserId { get; set; }
        public required AppUser User { get; set; }
    }
}
