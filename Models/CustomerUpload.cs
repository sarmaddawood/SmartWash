using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartWash.Models
{
    public class CustomerUpload
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public CustomerProfile? Customer { get; set; }

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = "Image"; // Image or Video

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Optional: Link to a specific order
        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public LaundryOrder? Order { get; set; }
    }
}
