namespace DailyCheckIn.Models.DTO
{
    public class SalaryDTO
    {
        public int Month { get; set; }
        public int Weak { get; set; }
        public int Year { get; set; }
        public double TotalMinutesWorked { get; set; }
        public double TotalOverTimeMinuteWorked { get; set; }
        public double TotalMoneyEarned { get; set; }
        public List<DayReportSalary> Days { get; set; } = [];
    }
}
