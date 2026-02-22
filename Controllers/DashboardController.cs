using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWash.Data;
using System.Security.Claims;

namespace SmartWash.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Fetch user profile
            var profile = await _context.CustomerProfiles
                .Include(p => p.Orders)
                    .ThenInclude(o => o.Items)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.CustomerUploads)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Fetch recent orders directly if profile is null (first time user)
            var orders = profile?.Orders.OrderByDescending(o => o.OrderDate).ToList() ?? new List<Models.LaundryOrder>();

            return View(orders);
        }
    }
}
