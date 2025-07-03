using DailyCheckIn.Enums;
using DailyCheckIn.Models.Entities.Base;

namespace DailyCheckIn.Models.Entities
{
    public class Payroll : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public required AppUser User { get; set; }
        public required MonthsEnum Month { get; set; }
        public required double TotalRegularHours { get; set; }
        public required double TotalTimeOffsHours { get; set; }
        public required double TotalOverTimeHours { get; set; }
        public required int TotalSalary { get; set; }
        public required int TotalAdvances { get; set; }
        public required int RemainingSalary { get; set; }
        public int AnnuaLeavesUsed { get; set; } = 0;
    }
}
