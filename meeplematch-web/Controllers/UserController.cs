using Microsoft.AspNetCore.Mvc;

namespace meeplematch_web.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
