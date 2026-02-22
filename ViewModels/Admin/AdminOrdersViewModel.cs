using SmartWash.Models;
using Microsoft.AspNetCore.Identity;

namespace SmartWash.ViewModels.Admin
{
    public class AdminOrdersViewModel
    {
        public List<LaundryOrder> Orders { get; set; } = new();
        public List<IdentityUser> StaffUsers { get; set; } = new();
        public string CurrentStatus { get; set; } = "All";
    }
}
