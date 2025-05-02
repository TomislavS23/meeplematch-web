using meeplematch_web.Models;
using meeplematch_web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
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
        public ActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
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

                ViewData["JwtToken"] = content;
                ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index", "Home");

                HttpContext.Session.SetString(Constants.JwtTokenFromSession, content);

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", content);

                var pubilcUser = await (await httpClient.GetAsync($"user/public/{username}")).Content.ReadAsAsync<PublicUserViewModel>();
                var user = await (await httpClient.GetAsync($"user/{pubilcUser.IdUser}")).Content.ReadAsAsync<UserViewModel>();
                //var users = await user.Content.ReadAsAsync<List<UserViewModel>>();
                //var userContent = users.FirstOrDefault();

                string role = user.RoleId == 1 ? "User" : "Admin";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["toast_success"] = "Login successful";

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return Ok("Login successful");
                }

                return View("LoginSuccess");
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
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
                HttpContext.Session.SetString(Constants.JwtTokenFromSession, contentResponse);
                TempData["toast_success"] = "Successful registration! You are now logged in.";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login", "Auth");
        }

    }
}
