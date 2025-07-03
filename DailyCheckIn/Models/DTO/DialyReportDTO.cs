using DailyCheckIn.Models.Entities;

namespace DailyCheckIn.Models.DTO
{
    public class TimeOffDTO
    {
        public TimeOnly TimeOffStart { get; set; }
        public TimeOnly? TimeOffEnd { get; set; }
    }
    public class DialyReportDTO
    {
        public TimeOnly CheckedInTime { get; set; }
        public Double TotalMinutesWorked { get; set; } = 0;
        public Double TotalMoneyEarned { get; set; }
        public TimeOnly CheckedOutTime { get; set; }
        public List<TimeOffDTO> TimeOffDTOs { get; set; } = [];

        public DialyReportDTO() { }
        
        public DialyReportDTO(double TotalMinutesWorked, double TotalMoneyEarned, IList<TimeOff> TimeOffsForTheDay) 
        {
            this.TotalMinutesWorked = TotalMinutesWorked;
            this.TotalMoneyEarned = TotalMoneyEarned;

            foreach(var t in TimeOffsForTheDay)
            {
                TimeOffDTOs.Add(new TimeOffDTO
                {
                    TimeOffStart = t.TimeOffStart,
                    TimeOffEnd = t.TimeOffEnd
                });
            }
        }
    }
}
