using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartWash.Models
{
    public class LaundryItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey("OrderId")]
        public LaundryOrder? Order { get; set; }

        [Required]
        public string ItemType { get; set; } = string.Empty; // Shirt, Pant, etc.

        [Required]
        public string ServiceType { get; set; } = string.Empty; // Wash, Iron, Dry Clean

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsCompleted { get; set; } = false;

        public string? Notes { get; set; }
    }
}
