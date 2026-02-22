using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWash.Data;
using SmartWash.Models;
using SmartWash.Services;

namespace SmartWash.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly UserManager<IdentityUser> _userManager;

        public PaymentsController(ApplicationDbContext context, IInvoiceService invoiceService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _invoiceService = invoiceService;
            _userManager = userManager;
        }

        // GET: Payments/Checkout/5
        public async Task<IActionResult> Checkout(Guid id)
        {
            var order = await _context.LaundryOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            
            // Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && order.Customer?.UserId != user?.Id)
            {
                return Forbid();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(Guid orderId)
        {
            var order = await _context.LaundryOrders
                .Include(o => o.Customer)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            // Ownership check
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && order.Customer?.UserId != user?.Id)
            {
                return Forbid();
            }

            if (order.Payment == null)
            {
                order.Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Amount = order.TotalPrice,
                    PaymentDate = DateTime.UtcNow,
                    Status = "Paid"
                };
            }
            else
            {
                order.Payment.Status = "Paid";
                order.Payment.PaymentDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Payments/DownloadInvoice/5
        public async Task<IActionResult> DownloadInvoice(Guid id)
        {
            var order = await _context.LaundryOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Ownership check
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && order.Customer?.UserId != user?.Id)
            {
                return Forbid();
            }

            var pdf = _invoiceService.GenerateInvoicePdf(order);
            return File(pdf, "application/pdf", $"Invoice-{order.Id.ToString().Substring(0, 8)}.pdf");
        }

        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Manage()
        {
            var payments = await _context.LaundryOrders
                .Include(o => o.Customer)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(payments);
        }
    }
}
