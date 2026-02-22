using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartWash.Models
{
    public class ServicePrice
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string ServiceName { get; set; } = string.Empty; // Wash, Iron, Dry Clean

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
