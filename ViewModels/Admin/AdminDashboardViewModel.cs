using SmartWash.Models;

namespace SmartWash.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Metric Cards
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveOrders { get; set; } // InProgress + Ready
        
        // Status Distribution for Bar/Pie Chart
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public Dictionary<string, int> PaymentsByStatus { get; set; } = new();
        
        // Service distribution
        public Dictionary<string, int> ServiceTypeDistribution { get; set; } = new();

        // Revenue Trend (Last 7 Months)
        public List<RevenueTrendPoint> RevenueTrend { get; set; } = new();

        public List<LaundryOrder> RecentOrders { get; set; } = new();
        public List<StaffPerformanceViewModel> StaffPerformance { get; set; } = new();
        public List<IdentityUser> StaffUsers { get; set; } = new();
    }

    public class StaffPerformanceViewModel
    {
        public string StaffName { get; set; } = string.Empty;
        public int CompletedOrders { get; set; }
        public double AverageCompletionTimeMinutes { get; set; }
    }

    public class RevenueTrendPoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class UserManagementViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLockedOut { get; set; }
    }
}
