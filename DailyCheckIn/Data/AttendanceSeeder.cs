using System;
using System.Text.Json;
using DailyCheckIn.Models.DTO;
using DailyCheckIn.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyCheckIn.Data
{
    public static class AttendanceSeeder
    {
        public static async Task SeedAttendances(ApplicationDbContext applicationDbContext)
        {
            if (await applicationDbContext.Attendances.AnyAsync()) return;

            var attendancesData = await File.ReadAllTextAsync("Data/AttendancesForAdmin.json");

            if (attendancesData == null) return;

            var attendances = JsonSerializer.Deserialize<List<AttendanceSeedDTO>>(attendancesData);

            if(attendances == null) return;

            foreach (var attendance in attendances)
            {
                var user = await applicationDbContext.Users.FindAsync(attendance.UserId);
                if (user == null || attendance == null) continue;


                var attendanceModel = new Attendance
                {
                    Id = Guid.NewGuid(),
                    CheckIn = attendance.CheckIn,
                    Date = attendance.Date,
                    UserId = attendance.UserId,
                    User = user,
                    CheckOut = attendance.CheckOut,
                    
                };

                var timeOffsForTheDay = attendance.TimeOffs?.Select(to => new TimeOff
                {
                    AttendanceDayId = attendanceModel.Id,
                    TimeOffStart = to.TimeOffStart,
                    TimeOffEnd = to.TimeOffEnd,
                    AttendanceDay=attendanceModel
                }).ToList();

                if(timeOffsForTheDay != null)
                    attendanceModel.TimeOffsForTheDay = timeOffsForTheDay;

                await applicationDbContext.Attendances.AddAsync(attendanceModel);
            }

            // Save all at once after loop
            await applicationDbContext.SaveChangesAsync();
        }
    }
}
