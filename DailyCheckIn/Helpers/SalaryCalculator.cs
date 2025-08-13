using DailyCheckIn.Models.Entities;
using DailyCheckIn.Helpers;
using DailyCheckIn.Models.DTO;

namespace DailyCheckIn.Helpers
{
    public class SalaryCalculator
    {
        public static List<DayReportSalary> CalculateSalary(List<Attendance?> attendancesList, double hourlyRate, DateOnly StatredDay)
        {
            var dayReportsList = new List<DayReportSalary>();
            var dayCounter = 0;
            foreach (Attendance? attendance in attendancesList)
            {
                if (attendance == null)
                {
                    dayReportsList.Add(new DayReportSalary()
                    {
                        DayDate = StatredDay.AddDays(dayCounter),
                    });
                }
                else
                {
                    try
                    {
                        dayReportsList.Add(GenerateDaySalaryReport(attendance, hourlyRate));
                    }
                    catch (Exception ex)
                    {
                        dayReportsList.Add(new DayReportSalary()
                        {
                            DayDate = StatredDay.AddDays(dayCounter),
                            ErrorMessage = ex.Message
                        });
                    }
                }
                dayCounter++;
            }

            return dayReportsList;
        }

        public static DayReportSalary GenerateDaySalaryReport(Attendance attendance, double hourlyRate)
        {
            var date = attendance.Date;
            var minutesWorked = AttendanceCalculator.CalculateDialyMinutesWorked(attendance);
                var overTimeMinutes = minutesWorked - 480;

                if (overTimeMinutes< 0) 
                    overTimeMinutes = 0;

            var moneyEarned = AttendanceCalculator.CalculateDailyMoneyEarned(minutesWorked - overTimeMinutes, overTimeMinutes, hourlyRate);

            var timesOff = attendance.TimeOffsForTheDay
                .Select(t => new TimeOffDTO
                {
                    TimeOffStart = t.TimeOffStart,
                    TimeOffEnd = t.TimeOffEnd
                }).ToList();

            return new DayReportSalary()
            {
                DayDate = date,
                TotalMinutesWorked = minutesWorked,
                TotalOverTimeMinutesWorked = overTimeMinutes,
                TotalMoneyEarned = moneyEarned,
                TimeOffDTOs = timesOff
            };
        }

        public static double CalculateMinutesWorked(List<DayReportSalary> dayReports)
        {
            var minutesWorked = 0.0;

            foreach (var dayReport in dayReports)
            {
                minutesWorked += dayReport.TotalMinutesWorked;
            }

            return minutesWorked;
        }

        public static double CalculateOverTimeWorked(List<DayReportSalary> dayReports)
        {
            var minutesWorked = 0.0;

            foreach (var dayReport in dayReports)
            {
                minutesWorked += dayReport.TotalOverTimeMinutesWorked;
            }

            return minutesWorked;
        }

        public static double CalculateMoneyEarned(List<DayReportSalary> dayReports)
        {
            var moneyEarned = 0.0;

            foreach (var dayReport in dayReports)
            {
                moneyEarned += dayReport.TotalMoneyEarned;
            }

            return moneyEarned;
        }
    }
}
