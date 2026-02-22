using System.ComponentModel.DataAnnotations;

namespace SmartWash.ViewModels.Admin
{
    public class CreateStaffViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;
    }
}
