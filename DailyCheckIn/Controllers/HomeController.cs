using DailyCheckIn.Models;
using DailyCheckIn.Models.DTO;
using DailyCheckIn.Models.Entities;
using DailyCheckIn.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using DailyCheckIn.Data;
using Microsoft.VisualBasic;
using Microsoft.EntityFrameworkCore;
using DailyCheckIn.Enums;

namespace DailyCheckIn.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _DbContext;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, ApplicationDbContext DbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _DbContext = DbContext;
        }

        public async Task<ActionResult<DialyReportDTO>> Index(DateOnly? DayDate)
        {
            if (DayDate == null) {
                DayDate = DateOnly.FromDateTime(DateTime.Now);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            var user = await _DbContext.Users
                .FirstOrDefaultAsync(u => u.Id == guidUserId && !u.Deleted);

            if (user == null)
                return NotFound();

            var attendance = _DbContext.Attendances
                .Include(a => a.TimeOffsForTheDay.Where(t => !t.Deleted))
                .FirstOrDefault(a => a.UserId == guidUserId && a.Date == DayDate);

            var forNow = new DateTime();

            if (attendance == null || attendance.CheckOut == null)
                forNow = DateTime.Now;

            var dialyReportDTO = DayReport(DayDate.Value, user, forNow);

            var checkInInformation = CheckInInformation(attendance, DayDate.Value);

            return View(new HomeModelDTO{ DayDate = DayDate.Value, CheckInModelDTO = checkInInformation, DialyReportDTO = dialyReportDTO });
        }

        [HttpPost]
        public async Task<ActionResult> CheckIn(DateTime dateTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null) 
                return NotFound();

            var attendance = await _DbContext.Attendances.FirstOrDefaultAsync(a => (
                a.Date == DateOnly.FromDateTime(dateTime) &&
                a.UserId == user.Id &&
                !a.Deleted
            ));

            if (attendance == null)
            {
                attendance = user.Attendances.FirstOrDefault() ?? new Attendance
                {
                    CheckIn = TimeOnly.FromDateTime(dateTime),
                    Date = DateOnly.FromDateTime(dateTime),
                    UserId = user.Id,
                    User = user,
                    CreatedBy = user,
                    CreatedById = user.Id,
                    CreatedOn = DateTime.Now,
                };

                await _DbContext.Attendances.AddAsync(attendance);
            }
            else
            {
                if(attendance.CheckOut <= dateTime)
                {
                    return BadRequest("the CheckOut can't be less then the checkIn!");
                }

                var latestTimeOffTime = LatestTimeOffTime(attendance);

                if (latestTimeOffTime != null)
                {
                    if (latestTimeOffTime <= TimeOnly.FromDateTime(dateTime)) 
                        return BadRequest("there is timeOff in before the checkIn Time!");
                }

                attendance.CheckIn = TimeOnly.FromDateTime(dateTime);
                _DbContext.Attendances.Update(attendance);
            }
            await _DbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> CheckOut(DateTime dateTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
                return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            var dateOnly = DateOnly.FromDateTime(dateTime);

            var user = await _DbContext.Users
            .FirstOrDefaultAsync(u => u.Id == guidUserId && !u.Deleted);

            if (user == null)
                return NotFound();

            var attendance = _DbContext.Attendances
                .FirstOrDefault(a => a.UserId == guidUserId && !a.Deleted);

            if (attendance == null)
                return NotFound();

            if (attendance.CheckOut != null)
                return BadRequest("You Already Checked Out!");

            if (DateOnly.FromDateTime(dateTime) != attendance.Date
                && DateOnly.FromDateTime(dateTime) != attendance.Date.AddDays(1))
                return BadRequest("Check out date should be the same date of the attendance date or the next day only");

            if(DateOnly.FromDateTime(dateTime) == attendance.Date 
                && TimeOnly.FromDateTime(dateTime) <= attendance.CheckIn) 
                return BadRequest("the check out can't be before check in!");

            attendance.CheckOut = dateTime;

            _DbContext.Attendances.Update(attendance);

            await _DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> StartTimeOff(DateTime dateTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var user = await _DbContext.Users
                 .Include(u => u.Attendances.Where(a => a.Date == DateOnly.FromDateTime(dateTime) && !a.Deleted))
                 .ThenInclude(a => a.TimeOffsForTheDay)
                 .Where(u => u.Id == Guid.Parse(userId) && !u.Deleted)
                 .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            var attendance = user.Attendances.FirstOrDefault();

            if (attendance == null)
                return NotFound("There is no attendance for the date you want to make Timeoff In");

            var lastTimeOff = attendance.TimeOffsForTheDay.LastOrDefault();

            if (lastTimeOff != null && !lastTimeOff.Deleted && !lastTimeOff.TimeOffEnd.HasValue)
                return BadRequest("You have to close the last timeoff before starting a new one");

            var newTimeOff = new TimeOff { AttendanceDay = attendance, AttendanceDayId = attendance.Id,
                TimeOffStart = TimeOnly.FromDateTime(dateTime),
                Date = DateOnly.FromDateTime(dateTime),
                CreatedById = user.Id,
                CreatedBy = user,
                CreatedOn = DateTime.Now,
            };
            _DbContext.TimeOffs.Add(newTimeOff);
            await _DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> EndTimeOff(DateTime dateTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .Include(u => u.Attendances.Where(a => a.Date == DateOnly.FromDateTime(dateTime) && !a.Deleted))
                 .ThenInclude(a => a.TimeOffsForTheDay)
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound("User not found");

            var attendance = user.Attendances.FirstOrDefault();

            if (attendance == null)
                return NotFound("There is no attendance for the date you want to make Timeoff In");

            var lastTimeOff = attendance.TimeOffsForTheDay.LastOrDefault();

            if (lastTimeOff == null || lastTimeOff.Deleted)
                return BadRequest("there is no TimeOff to end it");

            var timeOff = await _DbContext.TimeOffs.FindAsync(lastTimeOff.Id);

            if (timeOff == null)
                return NotFound("we could not find the timeoff!");

            timeOff.TimeOffEnd = TimeOnly.FromDateTime(dateTime);

            await _DbContext.SaveChangesAsync();

            return Ok();
        }

        private CheckInModelDTO CheckInInformation(Attendance? attendance, DateOnly DayDate)
        {
            var LastTimeOff = attendance?.TimeOffsForTheDay?.LastOrDefault();

            UserCheckInStatusEnum userCheckInStatus;

            if(attendance == null)
                userCheckInStatus = UserCheckInStatusEnum.NotCheckedIn;
            else if(LastTimeOff != null && LastTimeOff.TimeOffEnd == null)
                userCheckInStatus = UserCheckInStatusEnum.InTimeOff;
            else if(attendance.CheckOut == null)
                userCheckInStatus = UserCheckInStatusEnum.CheckedIn;
            else
                userCheckInStatus = UserCheckInStatusEnum.NotCheckedIn;

            return new CheckInModelDTO(TimeOnly.FromDateTime(DateTime.Now), userCheckInStatus );
        }

        private DialyReportDTO DayReport(DateOnly DayDate, AppUser user, DateTime? ForNow)
        {
            var attendance = user.Attendances.FirstOrDefault(attendance => attendance.Date == DayDate && !attendance.Deleted);

            if (attendance == null)
                return new DialyReportDTO();

            var minutesWorked = 0.0;

            if (ForNow.HasValue)
                minutesWorked = AttendanceCalculator.CalculateDialyMinutesWorked(attendance, ForNow.Value);
            else
                minutesWorked = AttendanceCalculator.CalculateDialyMinutesWorked(attendance);

            var overTimeWorked = (minutesWorked - 480) < 0 ? 0 : (minutesWorked - 480);

            var moneyEarned = AttendanceCalculator.CalculateDailyMoneyEarned(minutesWorked - overTimeWorked, overTimeWorked, user.HourlyRate);

            var TimeOffsForTheDay = attendance.TimeOffsForTheDay.ToList();

            return new DialyReportDTO(minutesWorked, moneyEarned, TimeOffsForTheDay);
        }

        private TimeOnly? LatestTimeOffTime(Attendance attendance)
        {
            var timeOffsOrderByStartedTime = attendance.TimeOffsForTheDay.OrderBy(a => a.TimeOffStart);

            var latestTimeOff = timeOffsOrderByStartedTime.LastOrDefault();
            if(latestTimeOff == null) return null;

            if(latestTimeOff.TimeOffEnd.HasValue)
                return latestTimeOff.TimeOffEnd.Value;

            return latestTimeOff.TimeOffStart;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
