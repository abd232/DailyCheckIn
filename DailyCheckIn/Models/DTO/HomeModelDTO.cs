namespace DailyCheckIn.Models.DTO
{
    public class HomeModelDTO
    {
        public required DateOnly DayDate { get; set; }
        public CheckInModelDTO? CheckInModelDTO { get; set; }
        public DialyReportDTO? DialyReportDTO { get; set; }
    }
}
