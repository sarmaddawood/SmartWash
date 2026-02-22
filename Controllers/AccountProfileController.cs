using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWash.Data;

namespace SmartWash.Controllers
{
    [Authorize]
    public class AccountProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            // Reusing CustomerProfile table to store Staff/Admin details for simplicity
            ViewBag.Email = user.Email;
            ViewBag.Role = User.IsInRole("Admin") ? "System Administrator" : "Staff Member";

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string phoneNumber, string address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new Models.CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    Email = user.Email
                };
                _context.CustomerProfiles.Add(profile);
            }
            else
            {
                profile.FullName = fullName;
                profile.PhoneNumber = phoneNumber;
                profile.Address = address;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Your profile has been successfully updated.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}
