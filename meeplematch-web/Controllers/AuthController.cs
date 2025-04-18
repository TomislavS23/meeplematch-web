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
        //private readonly IAuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiUrl = "auth";
        //public AuthController(IAuthService authService)
        //{
        //    _authService = authService;
        //}
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
            if (!response.IsSuccessStatusCode)
            {
                ViewData["Error"] = "Invalid credentials";
                return View();
            }
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                //Constants.JwtToken = content;
                //HttpContext context = HttpContext.Connection.Cur
                //HttpContext.Session.Set
                HttpContext.Session.SetString(Constants.JwtTokenFromSession, content);


                var jwtToken = HttpContext.Session.GetString(Constants.JwtTokenFromSession);
                var token = JwtUtils.ConvertJwtStringToJwtSecurityToken(jwtToken);
                var payload = JwtUtils.DecodeJwt(token);

                var username1 = payload.FirstOrDefault(x => x.Key.Contains("name")).Value.ToString();
                Console.WriteLine(username1);

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View();
            //var result = await _authService.LoginAsync(username, password);
            //if (result == null)
            //{
            //    ViewData["Error"] = "Invalid credentials";
            //    return View();
            //}

            //return RedirectToAction("Index", "Home");
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
                //Constants.JwtToken = contentResponse;
                HttpContext.Session.SetString(Constants.JwtTokenFromSession, contentResponse);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            //var dto = new RegisterDTO { Username = username, Email = email, Password = password, RoleId = 1 };
            //var result = await _authService.RegisterAsync(dto);
            //if (result == null)
            //{
            //    ViewData["Error"] = "Registration failed";
            //    return View("Login");
            //}

            return View();
        }
    }
}
