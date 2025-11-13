using DailyCheckIn.Data;
using DailyCheckIn.Enums;
using DailyCheckIn.Helpers;
using DailyCheckIn.Models;
using DailyCheckIn.Models.DTO;
using DailyCheckIn.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Security.Claims;

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

        public async Task<ActionResult<DailyReportDTO>> Index(DateOnly? DayDate)
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
                .FirstOrDefault(a => a.UserId == guidUserId && a.Date == DayDate && !a.Deleted);

            DateTime? forNow = null;
            var TimeOffsForTheDay = new List<TimeOff>();

            if (attendance != null)
                TimeOffsForTheDay = _DbContext.TimeOffs
                    .Where(t => t.AttendanceDayId == attendance.Id && !t.Deleted)
                    .OrderBy(t => t.TimeOffStart).ToList();

            if (attendance == null 
                || (attendance.CheckOut == null 
                && attendance.Date == DateOnly.FromDateTime(DateTime.Now)))
                forNow = DateTime.Now;


            var DailyReportDTO = DayReport(DayDate.Value, attendance, TimeOffsForTheDay, user.HourlyRate,forNow);

            var checkInInformation = CheckInInformation(attendance, TimeOffsForTheDay, DayDate.Value);

            return View(new HomeModelDTO{ DayDate = DayDate.Value, CheckInModelDTO = checkInInformation, DailyReportDTO = DailyReportDTO });
        }

        public async Task<ActionResult<DailyReportDTO>> DayReportPartialView(DateOnly? DayDate)
        {
            if (DayDate == null)
            {
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
                .FirstOrDefault(a => a.UserId == guidUserId && a.Date == DayDate && !a.Deleted);

            DateTime? forNow = null;
            var TimeOffsForTheDay = new List<TimeOff>();

            if (attendance != null)
                TimeOffsForTheDay = _DbContext.TimeOffs
                    .Where(t => t.AttendanceDayId == attendance.Id && !t.Deleted)
                    .OrderBy(t => t.TimeOffStart).ToList();

            if (attendance == null
                || (attendance.CheckOut == null
                && attendance.Date == DateOnly.FromDateTime(DateTime.Now)))
                forNow = DateTime.Now;

            var DailyReportDTO = DayReport(DayDate.Value, attendance,TimeOffsForTheDay,user.HourlyRate,forNow);
            return PartialView("_DayReportPartialView", DailyReportDTO);
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

                var kk = await _DbContext.Attendances.AddAsync(attendance);
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
                .FirstOrDefault(a => a.UserId == guidUserId && !a.Deleted && a.Date == dateOnly);

            if (attendance == null)
                return NotFound();

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

            var guidUserId = Guid.Parse(userId);

            var dateOnly = DateOnly.FromDateTime(dateTime);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id == guidUserId && !u.Deleted);

            if (user == null)
                return NotFound("User not found");

            var attendance = await _DbContext.Attendances
                .FirstOrDefaultAsync(a => a.UserId == guidUserId && a.Date == dateOnly && !a.Deleted);

            if (attendance == null)
                return NotFound("There is no attendance for the date you want to make Timeoff In");

            var lastTimeOff = await _DbContext.TimeOffs
                .Where(t => t.AttendanceDayId == attendance.Id && !t.Deleted)
                .OrderByDescending(t => t.TimeOffStart)
                .FirstOrDefaultAsync();

            if (lastTimeOff != null && !lastTimeOff.TimeOffEnd.HasValue)
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
        public async Task<ActionResult> TimeOff(DateTime dateTime,TimeOnly timeOffStart, TimeOnly? timeOffEnd,Guid? Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            var dateOnly = DateOnly.FromDateTime(dateTime);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id == guidUserId && !u.Deleted);

            if (user == null)
                return NotFound("User not found");

            var attendance = await _DbContext.Attendances
                .FirstOrDefaultAsync(a => a.UserId == guidUserId && a.Date == dateOnly && !a.Deleted);

            if (attendance == null)
                return BadRequest("There is no attendance for the date you want to make Timeoff In");

            Models.Entities.TimeOff? timeoff;

            if (Id.HasValue)
            {
                timeoff = await _DbContext.TimeOffs.FindAsync(Id.Value);
                if (timeoff == null)
                    return NotFound("the TimeOff did not found");
            }
            else
            {
                timeoff = new TimeOff
                {
                    Id = new Guid(),
                    AttendanceDay = attendance,
                    AttendanceDayId = attendance.Id,
                    CreatedBy = user,
                    CreatedById = user.Id,
                    CreatedOn = DateTime.Now,
                    Date = DateOnly.FromDateTime(dateTime),
                    TimeOffStart = timeOffStart,
                    TimeOffEnd = timeOffEnd
                };
            }

            if (!TimeOffDuringTheAttendanceTime(attendance, timeoff))
                return BadRequest("the time off should be in the checkIn time!");

            var timeOffsInThatDay = await _DbContext.TimeOffs
                .Where(t => t.AttendanceDayId == attendance.Id && !t.Deleted)
                .OrderBy(t => t.TimeOffStart)
                .ToListAsync();

            if (TimeOffDuringOtherTimeOff(timeOffsInThatDay, timeoff))
                return BadRequest("the timeOff can't be in other timeoffs");
            if (!Id.HasValue)
            {
                await _DbContext.TimeOffs.AddAsync(timeoff);
            }
            else
            {
                timeoff.TimeOffStart = timeOffStart;
                if(timeOffEnd.HasValue)
                    timeoff.TimeOffEnd = timeOffEnd;

                timeoff.ModifiedBy = user;
                timeoff.ModifiedById = user.Id;
                timeoff.ModifiedOn = DateTime.Now;
            }

            int result = await _DbContext.SaveChangesAsync();

            if(result <= 0)
                return StatusCode(500, "Insert failed!");

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

        private CheckInModelDTO CheckInInformation(Attendance? attendance,List<TimeOff> TimeOffsForTheDay, DateOnly DayDate)
        {
            var LastTimeOff = TimeOffsForTheDay.LastOrDefault();

            UserCheckInStatusEnum userCheckInStatus;

            if(attendance == null)
                userCheckInStatus = UserCheckInStatusEnum.NotCheckedIn;
            else if(LastTimeOff != null && LastTimeOff.TimeOffEnd == null)
                userCheckInStatus = UserCheckInStatusEnum.InTimeOff;
            else if(attendance.CheckOut == null)
                userCheckInStatus = UserCheckInStatusEnum.CheckedIn;
            else
                userCheckInStatus = UserCheckInStatusEnum.CheckedOut;

            return new CheckInModelDTO(TimeOnly.FromDateTime(DateTime.Now), userCheckInStatus );
        }

        private DailyReportDTO DayReport(DateOnly DayDate, Attendance? attendance,List<TimeOff> TimeOffsForTheDay, double HourlyRate, DateTime? ForNow)
        {
            if (attendance == null)
                return new DailyReportDTO();

            var minutesWorked = 0.0;

            if (ForNow.HasValue)
                minutesWorked = AttendanceCalculator.CalculateDialyMinutesWorked(attendance, ForNow.Value);
            else if (attendance.CheckOut.HasValue)
                minutesWorked = AttendanceCalculator.CalculateDialyMinutesWorked(attendance);
            else
                return new DailyReportDTO(attendance.CheckIn, TimeOffsForTheDay);

                var overTimeWorked = (minutesWorked - 480) < 0 ? 0 : (minutesWorked - 480);

            var moneyEarned = AttendanceCalculator.CalculateDailyMoneyEarned(minutesWorked - overTimeWorked, overTimeWorked, HourlyRate);

            TimeOnly? checkoutTimeOnly = attendance.CheckOut.HasValue ?
                    TimeOnly.FromDateTime(attendance.CheckOut.Value) : null;

            return new DailyReportDTO(attendance.CheckIn, checkoutTimeOnly, minutesWorked, moneyEarned, TimeOffsForTheDay);
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

        private bool TimeOffDuringTheAttendanceTime(Attendance attendance, TimeOff timeoff)
        {
            if (timeoff.TimeOffStart <= attendance.CheckIn)
                return false;

            if(attendance.CheckOut.HasValue 
                && timeoff.TimeOffEnd.HasValue
                && DateOnly.FromDateTime(attendance.CheckOut.Value) == attendance.Date
                && TimeOnly.FromDateTime(attendance.CheckOut.Value) <= timeoff.TimeOffEnd.Value)
                return false;

            return true;
        }

        private bool TimeOffDuringOtherTimeOff(List<TimeOff> timesOffInThatDay, TimeOff timeoff) 
        {
            foreach(var t in timesOffInThatDay)
            {
                if(t.Id == timeoff.Id)
                    continue;

                if(!t.TimeOffEnd.HasValue)
                    return true;

                if (timeoff.TimeOffEnd.HasValue
                    && timeoff.TimeOffStart < t.TimeOffStart
                    && timeoff.TimeOffEnd <= t.TimeOffEnd)
                    return false;

                if (timeoff.TimeOffEnd.HasValue
                    && timeoff.TimeOffStart > t.TimeOffEnd.Value)
                    return false;

                if (!timeoff.TimeOffEnd.HasValue
                    && timeoff.TimeOffStart >= t.TimeOffEnd)
                    return false; ;
            }

            return false;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
