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
    public class UploadsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;
        private readonly UserManager<IdentityUser> _userManager;

        public UploadsController(ApplicationDbContext context, IStorageService storageService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _storageService = storageService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMedia(IFormFile file, Guid? orderId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Validation (e.g., 10MB limit)
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["Error"] = "File size exceeds the 10MB limit.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            if (profile == null) return NotFound("User profile not found.");

            // Ownership check for the order
            if (orderId.HasValue)
            {
                var order = await _context.LaundryOrders.FindAsync(orderId.Value);
                if (order != null && order.CustomerId != profile.Id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }
            }

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var fileUrl = await _storageService.UploadFileAsync(file.OpenReadStream(), fileName, file.ContentType);

                var upload = new CustomerUpload
                {
                    Id = Guid.NewGuid(),
                    CustomerId = profile.Id,
                    OrderId = orderId,
                    FileUrl = fileUrl,
                    FileType = file.ContentType.StartsWith("video") ? "Video" : "Image",
                    UploadedAt = DateTime.UtcNow
                };

                _context.CustomerUploads.Add(upload);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Media uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Upload failed: {ex.Message}";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMedia(Guid id)
        {
            var upload = await _context.CustomerUploads.FindAsync(id);
            if (upload == null) return NotFound();

            // Authorization check
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            if (profile == null || upload.CustomerId != profile.Id)
            {
                return Forbid();
            }

            _context.CustomerUploads.Remove(upload);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
