using Microsoft.AspNetCore.Mvc;

namespace DailyCheckIn.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
