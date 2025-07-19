using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyCheckIn.Controllers
{
    [Authorize]
    public class SalaryController : Controller
    {
        public IActionResult Index(string month, string weak)
        {

            return View();
        }
    }
}
