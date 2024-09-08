using Microsoft.AspNetCore.Mvc;

namespace LogoutHeaders.Controllers
{
    public class LogoutController : Controller
    {
        public IActionResult Index()
        {
            Response.Headers.Append("Clear-Site-Data", "\"*\"");

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return View();
        }
    }
}
