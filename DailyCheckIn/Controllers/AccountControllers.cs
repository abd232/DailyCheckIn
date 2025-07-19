using DailyCheckIn.Interfaces;
using DailyCheckIn.Models.DTO;
using DailyCheckIn.Models.Entities;
using DailyCheckIn.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DailyCheckIn.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService tokenService;

        public AccountController(UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, 
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.tokenService = tokenService;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] UserLoginDTO userLoginDTO)
        {
            if (userLoginDTO == null)
                return BadRequest("User login information can't be empty!");

            var user = await _userManager.FindByNameAsync(userLoginDTO.UserName);

            if(user == null)
                return BadRequest("User name or Password Is not correct!");

            var attemptToLogin = await _signInManager.PasswordSignInAsync(userLoginDTO.UserName, userLoginDTO.Password, false, false);
            if(!attemptToLogin.Succeeded)
                return BadRequest("User name or Password Is not correct!");

            var token = await tokenService.CreateToken(user);
            if(token == null)
                return BadRequest("Failed to build a token!");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDTO dto)
        {
            if (dto == null)
                return BadRequest("User login information can't be empty!");

            if (await _userManager.FindByNameAsync(dto.UserName) != null ||
                (!string.IsNullOrWhiteSpace(dto.Email) && await _userManager.FindByEmailAsync(dto.Email) != null))
            {
                return BadRequest("Username or email is already taken.");
            }

            // Create new AppUser
            var user = new AppUser
            {
                UserName = dto.UserName,
                ArabicName = dto.ArabicName,
                Name = dto.Name ?? string.Empty,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                HourlyRate = dto.HourlyRate,
                Bonus = dto.Bonus,
                StartDate = dto.StartDate,
                CreatedOn = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            // OPTIONAL: Assign default roles
            await _userManager.AddToRoleAsync(user, "Employee"); // or use roles from request

            // Generate JWT token

            return RedirectToAction("Login", "Account");
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // logs out the cookie
            return RedirectToAction("Index", "Login");
        }
    }
}
