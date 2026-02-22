using System.ComponentModel.DataAnnotations;

namespace SmartWash.ViewModels.Orders
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        [Required]
        public string ItemType { get; set; } = "Shirt"; // Shirt, Pant, Dress, etc.

        [Required]
        public string ServiceType { get; set; } = "Wash"; // Wash, Iron, Dry Clean

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        public decimal PricePerItem { get; set; }
    }
}
