using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWash.Data;
using SmartWash.ViewModels.Admin;
using SmartWash.Models;
using System.Text;

namespace SmartWash.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
            var model = new AdminDashboardViewModel
            {
                TotalCustomers = await _context.CustomerProfiles.CountAsync(),
                TotalOrders = await _context.LaundryOrders.CountAsync(),
                TotalRevenue = await _context.LaundryOrders.SumAsync(o => o.TotalPrice),
                PendingOrders = await _context.LaundryOrders.CountAsync(o => o.Status == "Pending"),
                ActiveOrders = await _context.LaundryOrders.CountAsync(o => o.Status == "InProgress" || o.Status == "Ready"),
                
                RecentOrders = await _context.LaundryOrders
                    .Include(o => o.Customer)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync()
            };

            // Orders by Status
            var statusCounts = await _context.LaundryOrders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            model.OrdersByStatus = statusCounts.ToDictionary(x => x.Status, x => x.Count);

            // Payments by Status
            var paymentCounts = await _context.Payments
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            model.PaymentsByStatus = paymentCounts.ToDictionary(x => x.Status, x => x.Count);

            // Service Type Distribution
            var serviceCounts = await _context.LaundryItems
                .GroupBy(i => i.ServiceType)
                .Select(g => new { Service = g.Key, Count = g.Sum(x => x.Quantity) })
                .ToListAsync();
            model.ServiceTypeDistribution = serviceCounts.ToDictionary(x => x.Service, x => x.Count);

            // Revenue Trend (Last 6 months)
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                var monthLabel = targetDate.ToString("MMM");
                var revenue = await _context.LaundryOrders
                    .Where(o => o.OrderDate.Month == targetDate.Month && o.OrderDate.Year == targetDate.Year)
                    .SumAsync(o => o.TotalPrice);
                
                model.RevenueTrend.Add(new RevenueTrendPoint { Month = monthLabel, Amount = revenue });
            }

            // Staff Performance
            var staffInRole = await _userManager.GetUsersInRoleAsync("Staff");
            model.StaffUsers = staffInRole.ToList();

            foreach (var staff in staffInRole)
            {
                var completedOrders = await _context.LaundryOrders
                    .Where(o => o.AssignedToId == staff.Id && o.Status == "Delivered" && o.CompletedDate.HasValue && o.InProgressDate.HasValue)
                    .ToListAsync();

                if (completedOrders.Any())
                {
                    var avgTime = completedOrders.Average(o => (o.CompletedDate.Value - o.InProgressDate.Value).TotalMinutes);
                    var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == staff.Id);
                    model.StaffPerformance.Add(new StaffPerformanceViewModel
                    {
                        StaffName = profile?.FullName ?? staff.Email ?? "Staff",
                        CompletedOrders = completedOrders.Count,
                        AverageCompletionTimeMinutes = avgTime
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignStaff(Guid orderId, string staffId)
        {
            var order = await _context.LaundryOrders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.AssignedToId = staffId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Staff assigned to order.";
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateStatus(Guid[] orderIds, string newStatus)
        {
            if (orderIds == null || orderIds.Length == 0) return RedirectToAction(nameof(Orders));

            var orders = await _context.LaundryOrders.Where(o => orderIds.Contains(o.Id)).ToListAsync();
            foreach (var order in orders)
            {
                order.Status = newStatus;
                if (newStatus == "InProgress" && !order.InProgressDate.HasValue) order.InProgressDate = DateTime.UtcNow;
                if (newStatus == "Delivered" && !order.CompletedDate.HasValue) order.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Bulk updated {orders.Count} orders to {newStatus}.";
            return RedirectToAction(nameof(Orders));
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var modelList = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                
                modelList.Add(new UserManagementViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FullName = profile?.FullName ?? "N/A",
                    Role = string.Join(", ", roles),
                    IsLockedOut = await _userManager.IsLockedOutAsync(user)
                });
            }

            return View(modelList);
        }

        public async Task<IActionResult> Orders(string status = "All")
        {
            var query = _context.LaundryOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .AsQueryable();

            if (status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            var staff = await _userManager.GetUsersInRoleAsync("Staff");

            var model = new AdminOrdersViewModel
            {
                Orders = orders,
                StaffUsers = staff.ToList(),
                CurrentStatus = status
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Logic to lock user out (set LockoutEnd to far future)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> ReactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> ExportOrdersCsv()
        {
            var orders = await _context.LaundryOrders.Include(o => o.Customer).ToListAsync();
            var csv = new StringBuilder();
            csv.AppendLine("OrderID,Date,Customer,Amount,Status");

            foreach (var o in orders)
            {
                csv.AppendLine($"{o.Id},{o.OrderDate},{o.Customer?.FullName},{o.TotalPrice},{o.Status}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Orders_Export_{DateTime.Now:yyyyMMdd}.csv");
        }

        // --- Staff Management ---

        [HttpGet]
        public IActionResult CreateStaff() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Staff");
                    
                    var profile = new CustomerProfile
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        PhoneNumber = "N/A"
                    };
                    _context.CustomerProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Staff account created successfully.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Cannot delete an Admin account.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                TempData["Success"] = "User deleted successfully.";
            else
                TempData["Error"] = "Failed to delete user.";

            return RedirectToAction(nameof(Users));
        }

        // --- Pricing Management ---

        public async Task<IActionResult> Pricing()
        {
            var prices = await _context.ServicePrices.ToListAsync();
            return View(prices);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrice(Guid id, decimal newPrice)
        {
            var price = await _context.ServicePrices.FindAsync(id);
            if (price == null) return NotFound();

            price.PricePerUnit = newPrice;
            price.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Price for {price.ServiceName} updated.";
            return RedirectToAction(nameof(Pricing));
        }
    }
}
