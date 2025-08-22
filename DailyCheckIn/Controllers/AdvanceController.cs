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
    [Route("Advance")]
    public class AdvanceController(ILogger<HomeController> logger, UserManager<AppUser> userManager, ApplicationDbContext DbContext) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ApplicationDbContext _DbContext = DbContext;

        [HttpGet("")]
        public async Task<IActionResult> IndexAsync(int? year, int? month)
        {
            if (year == null)
                year = DateTime.Now.Year;

            if (month == null)
                month = DateTime.Now.Month;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();

            var advances = await GetAdvance(year.Value, month.Value, user);

            return View(new AdvancePageDTO
            {
                Year = year.Value,
                Month = month.Value,
                Advances = advances
            });
        }

        [HttpGet]
        [Route("AdvanceTable")]
        public async Task<IActionResult> AdvanceTable(int year, int month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();

            var advances = await GetAdvance(year, month, user);

            return PartialView("_AdvanceTablePartial", advances);
        }

        [HttpPost]
        [Route("AddNewAdvance")]
        public async Task<ActionResult> AddNewAdvance(DateTime date, int amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();
            try
            {
                await _DbContext.Advances.AddAsync(
                    new Advance
                    {
                        Id = Guid.NewGuid(),
                        Amount = amount,
                        Date = DateOnly.FromDateTime(date),
                        User = user,
                        UserId = user.Id,
                        CreatedBy = user,
                        CreatedById = user.Id,
                    }
                );
                await _DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Something went wrong", Details = ex.Message });
            }
        }

        [HttpPatch]
        [Route("EditAdvance")]
        public async Task<ActionResult> EditAdvance([FromBody] AdvanceDTO advance)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();

            if (advance == null)
                return BadRequest();
            try
            {
                var advanceFromDb = await _DbContext.Advances.FirstOrDefaultAsync(a => a.Id == advance.Id && !a.Deleted);

                if (advanceFromDb == null)
                    return NotFound();

                if (advanceFromDb.UserId != user.Id)
                    return Unauthorized();

                advanceFromDb.Date = advance.Date;

                advanceFromDb.Amount = advance.Amount;

                await _DbContext.SaveChangesAsync();

                return Ok("the advance has been updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Something went wrong", Details = ex.Message });
            }
        }

        [HttpPatch]
        [Route("DeleteAdvance")]
        public async Task<ActionResult> DeleteAdvance(Guid advanceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _DbContext.Users
                 .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.Deleted);

            if (user == null)
                return NotFound();

            if (userId == null)
                return Unauthorized();
            try
            {
                var advanceFromDb = await _DbContext.Advances.FirstOrDefaultAsync(a => a.Id == advanceId && !a.Deleted);

                if (advanceFromDb == null)
                    return NotFound();

                if (advanceFromDb.UserId != user.Id)
                    return Unauthorized();

                advanceFromDb.Deleted = true;

                await _DbContext.SaveChangesAsync();

                return Ok("the advance has been updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Something went wrong", Details = ex.Message });
            }
        }

        private async Task<List<AdvanceDTO>> GetAdvance(int year, int month, AppUser user)
        {
            var startingDate = new DateOnly(year, month, 1);

            var endingDate = startingDate.AddMonths(1);

            var advances = await _DbContext.Advances
                .Where(a => a.UserId == user.Id &&
                        a.Date >= startingDate && a.Date < endingDate && !a.Deleted).
                        Select(a => new AdvanceDTO
                        {
                            Id= a.Id,
                            Date = a.Date,
                            Amount = a.Amount,
                        }).OrderBy(a => a.Date).ToListAsync();

            return advances;
        }
    }
}
