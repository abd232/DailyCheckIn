using DailyCheckIn.Models.Entities;

namespace DailyCheckIn.Models.DTO
{
    public class TimeOffDTO
    {
        public Guid Id { get; set; }
        public TimeOnly TimeOffStart { get; set; }
        public TimeOnly? TimeOffEnd { get; set; }
    }
    public class DailyReportDTO
    {
        public TimeOnly? CheckedInTime { get; set; }
        public Double TotalMinutesWorked { get; set; } = 0;
        public Double TotalMoneyEarned { get; set; }
        public TimeOnly? CheckedOutTime { get; set; }
        public List<TimeOffDTO> TimeOffDTOs { get; set; } = [];

        public DailyReportDTO() { }
        
        public DailyReportDTO(TimeOnly CheckedInTime, List<TimeOff> TimeOffsForTheDay)
        {
            this.CheckedInTime = CheckedInTime;

            foreach (var t in TimeOffsForTheDay)
            {
                TimeOffDTOs.Add(new TimeOffDTO
                {
                    Id = t.Id,
                    TimeOffStart = t.TimeOffStart,
                    TimeOffEnd = t.TimeOffEnd
                });
            }
        }

        public DailyReportDTO(TimeOnly CheckedInTime, TimeOnly? CheckedOutTime, double TotalMinutesWorked, double TotalMoneyEarned, IList<TimeOff> TimeOffsForTheDay) 
        {
            this.CheckedInTime = CheckedInTime;
            this.CheckedOutTime = CheckedOutTime;
            this.TotalMinutesWorked = TotalMinutesWorked;
            this.TotalMoneyEarned = TotalMoneyEarned;

            foreach(var t in TimeOffsForTheDay)
            {
                TimeOffDTOs.Add(new TimeOffDTO
                { 
                    Id = t.Id,
                    TimeOffStart = t.TimeOffStart,
                    TimeOffEnd = t.TimeOffEnd
                });
            }
        }
    }
}
