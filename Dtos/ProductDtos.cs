using System.ComponentModel.DataAnnotations;

namespace back_end_cuoi_ky.Dtos
{
    // DTO cho Request tạo Product
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được quá 200 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được quá 1000 ký tự")]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải >= 0")]
        public int Stock { get; set; }
    }

    // DTO cho Request update Product (partial update - chỉ update những field được gửi)
    public class UpdateProductDto
    {
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được quá 200 ký tự")]
        public string? Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal? Price { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được quá 1000 ký tự")]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải >= 0")]
        public int? Stock { get; set; }
    }

    // DTO cho Response Product
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}