using DailyCheckIn.Data;
using DailyCheckIn.Models.DTO;
using DailyCheckIn.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace DailyCheckIn.Controllers
{
    [Authorize]
    [Route("Salary")]
    public class SalaryController(ILogger<HomeController> logger, UserManager<AppUser> userManager, ApplicationDbContext DbContext) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ApplicationDbContext _DbContext = DbContext;
        [HttpGet("")]
        public async Task<ActionResult> Index(int? year, int? month, int? weak)
        {
            if (year == null)
                year = DateTime.Now.Year;

            if (month == null)
                month = DateTime.Now.Month;

            if (weak == null)
                weak = (DateTime.Now.Day / 7) + 1;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();

            var monthAttendnceInfo = await GetDaysReportsList(year.Value, month.Value, user, new int?());

            var daysWeakReportsList = GetDaysWeakSalary(year.Value, month.Value, weak.Value, monthAttendnceInfo);

            var salaryDTO = GetSalaryDTO(weak.Value, month.Value, year.Value, monthAttendnceInfo, daysWeakReportsList);

            return View(salaryDTO);
        }

        [HttpGet("DaysWeakSalaryTable")]
        public async Task<ActionResult<List<DayReportSalary>>> DaysWeakSalaryTable(int year, int month, int weak)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();

            var daysWeakSalary = await GetDaysReportsList(year, month, user, weak);

            return PartialView("_DayReportSalaryTablePartial", daysWeakSalary);
        }

        private List<DayReportSalary> GetDaysWeakSalary(int year, int month, int weak, List<DayReportSalary> monthAttendnceInfo)
        {
            var startingDate = new DateOnly(year, month, (weak - 1) * 7 + 1);

            var endingDate = weak == 5 ? new DateOnly(year, month, 1).AddMonths(1) : startingDate.AddDays(7);

            if (startingDate == endingDate)
            {
                weak = 1;
                month++;

                startingDate = new DateOnly(year, month, (weak - 1) * 7 + 1);
                endingDate = startingDate.AddDays(7);
            }

            return monthAttendnceInfo.Where(a => a.DayDate >= startingDate && a.DayDate < endingDate).ToList();
        }

        private async Task<List<DayReportSalary>> GetDaysReportsList(int year, int month, AppUser user, int? weak)
        {
            var startingDate = new DateOnly(year, month, 1);

            var endingDate = startingDate.AddMonths(1);

            if (weak.HasValue)
            {

                startingDate = new DateOnly(year, month, (weak.Value - 1) * 7 + 1);

                endingDate = weak == 5 ? new DateOnly(year, month + 1, 1) : startingDate.AddDays(7);

                if (startingDate == endingDate)
                {
                    weak = 1;
                    month++;

                    startingDate = new DateOnly(year, month, (weak.Value - 1) * 7 + 1);
                    endingDate = startingDate.AddDays(7);
                }
            }
            else
            {

            }
            var attendances = await _DbContext.Attendances
                .Where(a => a.UserId == user.Id && a.Date >= startingDate && a.Date < endingDate && !a.Deleted)
                .Select(a => new Attendance
                {
                    CheckIn = a.CheckIn,
                    Date = a.Date,
                    User = user,
                    UserId = user.Id,
                    Id = a.Id,
                    TimeOffsForTheDay = a.TimeOffsForTheDay,
                    CheckOut = a.CheckOut,
                    CreatedBy = a.CreatedBy,
                    CreatedById = a.CreatedById,
                    CreatedOn = a.CreatedOn,
                    Deleted = a.Deleted,

                }).OrderBy(a => a.Date).ToListAsync();

            List<Attendance?> attendancesForTheWeak = [];

            for (var date = startingDate; date < endingDate; date = date.AddDays(1))
            {
                var attendance = attendances.FirstOrDefault(a => a.Date == date);
                attendancesForTheWeak.Add(attendance);
            }

            return Helpers.SalaryCalculator.CalculateSalary(attendancesForTheWeak, user.HourlyRate, startingDate);
        }

        private SalaryDTO GetSalaryDTO(int weak, int month, int year,List<DayReportSalary> attendances, List<DayReportSalary> daysReportsList) 
        {
            var TotalMinutesWorked = Helpers.SalaryCalculator.CalculateMinutesWorked(attendances);

            var TotalOverTimeWorked = Helpers.SalaryCalculator.CalculateOverTimeWorked(attendances);

            var totalMoneyEarned = Helpers.SalaryCalculator.CalculateMoneyEarned(attendances);

            return new SalaryDTO
            {
                Weak = weak,
                Month = month,
                Year = year,
                Days = daysReportsList,
                TotalMinutesWorked = TotalMinutesWorked,
                TotalOverTimeMinuteWorked = TotalOverTimeWorked,
                TotalMoneyEarned = totalMoneyEarned
            };
        }
    }
}
