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

            var user = await _DbContext.Users
                 .Include(u => u.Attendances.Where(a => a.Date == DayDate.Value && !a.Deleted))
                 .ThenInclude(a => a.TimeOffsForTheDay)
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();

            var forNow = new DateTime();

            if (user.Attendances.FirstOrDefault() == null || user.Attendances.First().CheckOut == null)
                forNow = DateTime.Now;

            var dialyReportDTO = DayReport(DayDate.Value, user, forNow);

            var checkInInformation = CheckInInformation(user, DayDate.Value);

            return View(new HomeModelDTO{ DayDate = DayDate.Value, CheckInModelDTO = checkInInformation, DialyReportDTO = dialyReportDTO });
        }

        [HttpPost]
        public async Task<ActionResult> CheckIn(DateTime dateTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .Include(u => u.Attendances.Where(a => a.Date == DateOnly.FromDateTime(dateTime) && !a.Deleted))
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null) 
                return NotFound();

            var attendance = user.Attendances.FirstOrDefault() ?? new Attendance { CheckIn = TimeOnly.FromDateTime(dateTime),
                Date = DateOnly.FromDateTime(dateTime),
                UserId = user.Id,User = user,
                CreatedBy = user,
                CreatedById = user.Id,
                CreatedOn = DateTime.Now,
            };

            return Ok();
        }

        [HttpPatch]
        public async Task<ActionResult> CheckOut(DateTime dateTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .Include(u => u.Attendances.Where(a => a.Date == DateOnly.FromDateTime(dateTime)  &&  !a.Deleted))
                 .ThenInclude(a => a.TimeOffsForTheDay)
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            var attendance = user.Attendances.FirstOrDefault();

            if (attendance == null)
                return BadRequest();

            var lastTimeOff = attendance.TimeOffsForTheDay.LastOrDefault();

            if (lastTimeOff != null && lastTimeOff.TimeOffEnd != null)
                return BadRequest();

            attendance.CheckOut = dateTime;

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> TimeOffStarted(DateTime dateTime)
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

        [HttpPatch]
        public async Task<ActionResult> TimeOffEnds(DateTime dateTime)
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

        [HttpGet]
        private CheckInModelDTO CheckInInformation(AppUser user, DateOnly DayDate)
        {
            var attendance = user.Attendances.FirstOrDefault(attendance => attendance.Date == DayDate && !attendance.Deleted);

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
