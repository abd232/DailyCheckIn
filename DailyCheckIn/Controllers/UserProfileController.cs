using DailyCheckIn.Data;
using DailyCheckIn.Models.DTO;
using DailyCheckIn.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DailyCheckIn.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _DbContext;

        public UserProfileController(UserManager<AppUser> userManager, ApplicationDbContext DbContext)
        {
            _userManager = userManager;
            _DbContext = DbContext;
        }

        [HttpGet("{username}")]
        public IActionResult Index(string userName)
        {
            var userProfileDto = _DbContext.Users.Where(user => user.UserName == userName).Select(
                user => new UserProfileDTO
                { UserName = user.UserName ?? "",
                  Name = user.UserName,
                  ArabicName = user.ArabicName,
                  Email = user.Email,
                  PhoneNumber = user.PhoneNumber,
                  HourlyRate = user.HourlyRate,
                  Bonus = user.Bonus,
                  StartDate = user.StartDate
                }
            ).FirstOrDefault();
            if (userProfileDto == null)
                return NotFound();

            return View(userProfileDto);
        }
    }
}
