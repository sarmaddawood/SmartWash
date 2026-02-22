using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartWash.Controllers
{
    [Authorize]
    public class DashboardRedirectController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (User.IsInRole("Staff"))
            {
                return RedirectToAction("Index", "Staff", new { area = "Staff" });
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}
