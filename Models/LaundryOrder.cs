using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartWash.Models
{
    public class LaundryOrder
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public CustomerProfile? Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Ready, Delivered

        public string? AssignedToId { get; set; } // Staff User ID

        public DateTime? InProgressDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public ICollection<LaundryItem> Items { get; set; } = new List<LaundryItem>();
        
        public Payment? Payment { get; set; }

        public ICollection<CustomerUpload> CustomerUploads { get; set; } = new List<CustomerUpload>();
    }
}
