using meeplematch_web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace meeplematch_web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiUrl = "auth";
        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var response = await httpClient.GetAsync($"{apiUrl}/login?username={username}&password={password}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return Unauthorized("Invalid credentials");
                }
                ViewData["Error"] = "Invalid credentials";
                TempData["toast_error"] = "Invalid credentials";
                return View();
            }
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString(Constants.JwtTokenFromSession, content);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["toast_success"] = "Login successful";

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            Object registeringUser = new
            {
                Username = username,
                Email = email,
                Password = password,
                RoleId = 1
            };
            var postRegister = JsonSerializer.Serialize(registeringUser);
            var content = new StringContent(
                postRegister,
                System.Text.Encoding.UTF8,
                Application.Json
            );
            var response = await httpClient.PostAsync($"{apiUrl}/register", content);
            if (!response.IsSuccessStatusCode)
            {
                ViewData["Error"] = "Registration failed";
                TempData["toast_error"] = "Registration failed";
                return View("Login");
            }
            if (response.IsSuccessStatusCode)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                var contentResponse = await response.Content.ReadAsStringAsync();
                //Constants.JwtToken = contentResponse;
                HttpContext.Session.SetString(Constants.JwtTokenFromSession, contentResponse);
                TempData["toast_success"] = "Successful registration! You are now logged in.";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View();
        }
    }
}
