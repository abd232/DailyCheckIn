using DailyCheckIn.Models.Entities;

namespace DailyCheckIn.Helpers
{
    public class AttendanceCalculator
    {
        public static double CalculateDialyMinutesWorked(Attendance attendance)
        {
            if (!attendance.CheckOut.HasValue)
                return -1;

            double TotalTimesOffMinutes = 0;

            if (attendance.TimeOffsForTheDay != null) { 
                foreach (var TimeOff in attendance.TimeOffsForTheDay)
                {
                    if(TimeOff.TimeOffEnd.HasValue)
                        TotalTimesOffMinutes += TimeOff.TimeOffEnd.Value.ToTimeSpan().TotalMinutes - TimeOff.TimeOffStart.ToTimeSpan().TotalMinutes;
                }
            }

            if (attendance.Date == DateOnly.FromDateTime(attendance.CheckOut.Value))
                return attendance.CheckOut.Value.TimeOfDay.TotalMinutes - attendance.CheckIn.ToTimeSpan().TotalMinutes - TotalTimesOffMinutes;
            else if (attendance.Date > DateOnly.FromDateTime(attendance.CheckOut.Value))
                return -1;
            else
            {
                var numberOfDays = DateOnly.FromDateTime(attendance.CheckOut.Value).Day - attendance.Date.Day;
                for (; numberOfDays > 1; numberOfDays--)
                {
                    TotalTimesOffMinutes += 60 * 24;
                }

                var MidNightTime = new TimeOnly(24, 0);

                var TotalOfMinutesAfterMidNight = attendance.CheckOut.Value.TimeOfDay.TotalMinutes;

                var totalOfMinutesTillMidNight = MidNightTime.ToTimeSpan().TotalMinutes - attendance.CheckIn.ToTimeSpan().TotalMinutes;

                return totalOfMinutesTillMidNight + totalOfMinutesTillMidNight;
            }
        }

        public static double CalculateDialyMinutesWorked(Attendance attendance, DateTime forNow)
        {
            double TotalTimesOffMinutes = 0;

            if (attendance.TimeOffsForTheDay != null)
            {
                foreach (var TimeOff in attendance.TimeOffsForTheDay)
                {
                    if (TimeOff.TimeOffEnd.HasValue)
                        TotalTimesOffMinutes += TimeOff.TimeOffEnd.Value.ToTimeSpan().TotalMinutes - TimeOff.TimeOffStart.ToTimeSpan().TotalMinutes;
                }
            }

            if (attendance.Date == DateOnly.FromDateTime(forNow))
                return forNow.TimeOfDay.TotalMinutes - attendance.CheckIn.ToTimeSpan().TotalMinutes - TotalTimesOffMinutes;
            else if (attendance.Date > DateOnly.FromDateTime(forNow))
                return -1;
            else
            {
                var numberOfDays = DateOnly.FromDateTime(forNow).Day - attendance.Date.Day;
                for (; numberOfDays > 1; numberOfDays--)
                {
                    TotalTimesOffMinutes += 60 * 24;
                }

                var MidNightTime = new TimeOnly(24, 0);

                var TotalOfMinutesAfterMidNight = forNow.TimeOfDay.TotalMinutes;

                var totalOfMinutesTillMidNight = MidNightTime.ToTimeSpan().TotalMinutes - attendance.CheckIn.ToTimeSpan().TotalMinutes;

                return totalOfMinutesTillMidNight + totalOfMinutesTillMidNight;
            }
                


        }

        public static double CalculateDailyMoneyEarned(double MinutesWorked, double HourlyRate)
        {

            return MinutesWorked * HourlyRate / 60;
        }
    }
}
