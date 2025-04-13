using meeplematch_web.DTO;
using meeplematch_web.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace meeplematch_web.Controllers
{
    public class AuthhController : Controller
    {
        private readonly IAuthService _authService;
        public AuthhController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _authService.LoginAsync(username, password);
            if (result == null)
            {
                ViewData["Error"] = "Invalid credentials";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            var dto = new RegisterDTO { Username = username, Email = email, Password = password, RoleId = 1 };
            var result = await _authService.RegisterAsync(dto);
            if (result == null)
            {
                ViewData["Error"] = "Registration failed";
                return View("Login");
            }

            return RedirectToAction("Login");
        }
    }
}
