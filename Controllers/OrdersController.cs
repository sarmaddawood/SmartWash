using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWash.Data;
using SmartWash.Models;
using SmartWash.ViewModels.Orders;
using System.Security.Claims;

namespace SmartWash.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders/Create
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var model = new CreateOrderViewModel
            {
                CustomerName = profile?.FullName ?? "",
                Address = profile?.Address ?? "",
                PhoneNumber = profile?.PhoneNumber ?? "",
                Items = new List<OrderItemViewModel> { new OrderItemViewModel() }
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Customer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Ensure profile exists
            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = model.CustomerName,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Email = user.Email
                };
                _context.CustomerProfiles.Add(profile);
            }

            var order = new LaundryOrder
            {
                Id = Guid.NewGuid(),
                CustomerId = profile.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalPrice = model.Items.Sum(i => i.Quantity * GetPrice(i.ItemType, i.ServiceType))
            };

            foreach (var item in model.Items)
            {
                order.Items.Add(new LaundryItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ItemType = item.ItemType,
                    ServiceType = item.ServiceType,
                    Quantity = item.Quantity,
                    Price = GetPrice(item.ItemType, item.ServiceType)
                });
            }

            _context.LaundryOrders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }



        private decimal GetPrice(string itemType, string serviceType)
        {
            // Simple pricing logic
            decimal basePrice = itemType switch
            {
                "Shirt" => 50,
                "Pant" => 60,
                "Dress" => 100,
                "Suit" => 250,
                _ => 40
            };

            decimal multiplier = serviceType switch
            {
                "Wash" => 1.0m,
                "Iron" => 0.5m,
                "Dry Clean" => 2.0m,
                _ => 1.0m
            };

            return basePrice * multiplier;
        }
    }
}
