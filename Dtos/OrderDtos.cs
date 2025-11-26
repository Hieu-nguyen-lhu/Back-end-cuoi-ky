using System.ComponentModel.DataAnnotations;

namespace back_end_cuoi_ky.Dtos
{
    // DTO cho Request tạo Order
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer ID là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Customer ID phải > 0")]
        public int CustomerId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Chi tiết đơn hàng là bắt buộc")]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 sản phẩm")]
        public List<CreateOrderDetailDto> OrderDetails { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        public string DeliveryAddress { get; set; }
    }

    // DTO cho Request update Order (partial update - chỉ update những field được gửi)
    public class UpdateOrderDto
    {
        [StringLength(50)]
        public string? Status { get; set; }
    }

    // DTO cho Response Order
    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderDetailDto>? OrderDetails { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryAddress { get; set; }
    }

    // DTO chi tiết cho Create OrderDetail
    public class CreateOrderDetailDto
    {
        [Required(ErrorMessage = "Product ID là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID phải > 0")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        public int Quantity { get; set; }
    }

    // DTO cho Response OrderDetail
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}