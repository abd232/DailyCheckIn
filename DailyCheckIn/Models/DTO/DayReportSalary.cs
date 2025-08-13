namespace DailyCheckIn.Models.DTO
{
    public class DayReportSalary
    {
        public DateOnly DayDate { get; set; }
        public string ErrorMessage { get; set; } = "";
        public double TotalMinutesWorked { get; set; }
        public double TotalOverTimeMinutesWorked { get; set; }
        public double TotalMoneyEarned { get; set; }
        public List<TimeOffDTO> TimeOffDTOs { get; set; } = [];

    }
}
