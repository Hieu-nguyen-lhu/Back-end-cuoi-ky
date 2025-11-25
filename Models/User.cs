using System.ComponentModel.DataAnnotations;

namespace back_end_cuoi_ky.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3-50 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc")]
        [StringLength(255)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role là bắt buộc")]
        [StringLength(50)]
        public string Role { get; set; } // "Admin" hoặc "User"

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key to Customer (optional, chỉ cho User role)
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
