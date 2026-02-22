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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var activeServices = await _context.ServicePrices.Where(s => s.IsActive).ToListAsync();

            var model = new CreateOrderViewModel
            {
                CustomerName = profile?.FullName ?? "",
                Address = profile?.Address ?? "",
                PhoneNumber = profile?.PhoneNumber ?? "",
                Items = new List<OrderItemViewModel> { new OrderItemViewModel() },
                AvailableServices = activeServices
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            var activeServices = await _context.ServicePrices.Where(s => s.IsActive).ToListAsync();
            
            if (!ModelState.IsValid) 
            {
                model.AvailableServices = activeServices;
                return View(model);
            }

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
                CreatedById = user.Id, // Track which Staff/Admin created this
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            decimal totalPrice = 0;

            foreach (var item in model.Items)
            {
                decimal itemPrice = GetPrice(item.ItemType, item.ServiceType, activeServices);
                totalPrice += item.Quantity * itemPrice;

                order.Items.Add(new LaundryItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ItemType = item.ItemType,
                    ServiceType = item.ServiceType,
                    Quantity = item.Quantity,
                    Price = itemPrice
                });
            }
            order.TotalPrice = totalPrice;

            _context.LaundryOrders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }



        private decimal GetPrice(string itemType, string serviceType, List<ServicePrice> currentPrices)
        {
            // Simple item base pricing logic
            decimal basePrice = itemType switch
            {
                "Shirt" => 50,
                "Pant" => 60,
                "Dress" => 100,
                "Suit" => 250,
                _ => 40
            };

            // Dynamic multiplier or flat rate from ServicePrices
            var service = currentPrices.FirstOrDefault(s => s.ServiceName == serviceType);
            decimal serviceRate = service?.PricePerUnit ?? 1.0m; // Default to 1.0 if not found
            
            // Assume ServicePrice is a multiplier if < 10, otherwise it's a flat additive cost
            if (serviceRate < 10)
                return basePrice * serviceRate;
            else
                return basePrice + serviceRate;
        }
    }
}
