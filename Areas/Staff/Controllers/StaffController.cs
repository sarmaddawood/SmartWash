using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWash.Data;

namespace SmartWash.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string statusFilter = "Active", string searchTerm = "")
        {
            var query = _context.LaundryOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.CustomerUploads)
                .AsQueryable();

            // If not admin, strictly only show orders they personally created
            if (!User.IsInRole("Admin"))
            {
                var userId = (await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name))?.Id;
                query = query.Where(o => o.CreatedById == userId);
            }

            // Search by Name or ID
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => 
                    (o.Customer != null && o.Customer.FullName.Contains(searchTerm)) || 
                    o.Id.ToString().Contains(searchTerm));
            }

            // Filter by Status
            if (statusFilter == "Active")
            {
                query = query.Where(o => o.Status != "Delivered");
            }
            else if (statusFilter != "All")
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchTerm = searchTerm;
            
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string newStatus)
        {
            var order = await _context.LaundryOrders.FindAsync(id);
            if (order == null) return NotFound();

            // Transition Validation
            bool isValid = (order.Status, newStatus) switch
            {
                ("Pending", "InProgress") => true,
                ("InProgress", "Ready") => true,
                ("Ready", "Delivered") => true,
                _ => false
            };

            if (!isValid)
            {
                TempData["Error"] = $"Invalid status transition from {order.Status} to {newStatus}.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = newStatus;
            
            if (newStatus == "InProgress" && !order.InProgressDate.HasValue)
            {
                order.InProgressDate = DateTime.UtcNow;
            }
            else if (newStatus == "Delivered" && !order.CompletedDate.HasValue)
            {
                order.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Order #{order.Id.ToString().Substring(0, 8)} updated to {newStatus}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleItemCompletion(Guid itemId)
        {
            var item = await _context.LaundryItems.FindAsync(itemId);
            if (item == null) return NotFound();

            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();

            return Ok(new { isCompleted = item.IsCompleted });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItemNotes(Guid itemId, string notes)
        {
            var item = await _context.LaundryItems.FindAsync(itemId);
            if (item == null) return NotFound();

            item.Notes = notes;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
