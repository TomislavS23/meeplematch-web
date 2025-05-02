using meeplematch_web.DTO;
using meeplematch_web.Interfaces;
using meeplematch_web.Models;
using meeplematch_web.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult Login2(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login2(string username, string password, string? returnUrl = null)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var response = await httpClient.GetAsync($"{apiUrl}/login?username={username}&password={password}");
            if (!response.IsSuccessStatusCode)
            {
                ViewData["Error"] = "Invalid credentials";
                return View();
            }
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewData["JwtToken"] = content;
                ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index", "Home");

                return View("LoginSuccess");

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
                return View("Login");
            }
            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString(Constants.JwtTokenFromSession, contentResponse);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login2", "Auth");
        }

    }
}
