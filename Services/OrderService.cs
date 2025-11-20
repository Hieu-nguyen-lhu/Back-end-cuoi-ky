using Microsoft.EntityFrameworkCore;
using back_end_cuoi_ky.Data;
using back_end_cuoi_ky.Dtos;
using back_end_cuoi_ky.Models;

namespace back_end_cuoi_ky.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto?> UpdateAsync(int id, UpdateOrderDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapToDto(o));
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
        {
            // Kiểm tra Customer có tồn tại không
            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                throw new InvalidOperationException($"Khách hàng với ID {dto.CustomerId} không tồn tại");
            }

            // Tạo Order
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.Now,
                Status = dto.Status,
                TotalAmount = 0,
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Lưu để có OrderId

            // Xử lý OrderDetails và tính tổng tiền
            decimal totalAmount = 0;
            foreach (var detailDto in dto.OrderDetails)
            {
                // Lấy thông tin sản phẩm
                var product = await _context.Products.FindAsync(detailDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Sản phẩm với ID {detailDto.ProductId} không tồn tại");
                }

                // Kiểm tra tồn kho
                if (product.Stock < detailDto.Quantity)
                {
                    throw new InvalidOperationException($"Sản phẩm '{product.Name}' không đủ hàng trong kho. Còn lại: {product.Stock}");
                }

                // Tạo OrderDetail
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = detailDto.ProductId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = product.Price, // Lấy giá hiện tại của sản phẩm
                    Subtotal = product.Price * detailDto.Quantity,
                    CreatedAt = DateTime.Now
                };

                _context.OrderDetails.Add(orderDetail);
                totalAmount += orderDetail.Subtotal;

                // Giảm tồn kho
                product.Stock -= detailDto.Quantity;
            }

            // Cập nhật tổng tiền cho Order
            order.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();

            // Lấy lại Order với đầy đủ thông tin
            return await GetByIdAsync(order.Id) ?? throw new InvalidOperationException("Không thể tải thông tin đơn hàng vừa tạo");
        }

        public async Task<OrderDto?> UpdateAsync(int id, UpdateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return null;

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return false;

            // Hoàn lại tồn kho trước khi xóa
            foreach (var detail in order.OrderDetails)
            {
                detail.Product.Stock += detail.Quantity;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId)
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapToDto(o));
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.Name ?? "",
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name ?? "",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    Subtotal = od.Subtotal
                }).ToList()
            };
        }
    }
}