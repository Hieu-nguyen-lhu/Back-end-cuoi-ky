using Microsoft.AspNetCore.Mvc;
using back_end_cuoi_ky.Dtos;
using back_end_cuoi_ky.Services;
using Microsoft.AspNetCore.Authorization;

namespace back_end_cuoi_ky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tất cả endpoint yêu cầu login
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ---------------------------
        // GET ALL ORDERS
        // ---------------------------
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();

                // Nếu là User, lọc chỉ đơn của mình
                if (User.IsInRole("User"))
                {
                    var currentUser = User.Identity?.Name;
                    orders = orders.Where(o => o.CustomerName == currentUser).ToList();
                }

                return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // ---------------------------
        // GET ORDER BY ID
        // ---------------------------
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { success = false, message = $"Không tìm thấy đơn hàng với ID {id}" });

                if (!UserCanAccessOrder(order))
                    return Forbid();

                return Ok(new { success = true, data = order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // ---------------------------
        // GET ORDERS BY CUSTOMER ID
        // ---------------------------
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> GetByCustomerId(int customerId)
        {
            try
            {
                var orders = await _orderService.GetByCustomerIdAsync(customerId);

                // Nếu là User, chỉ trả đơn của mình
                if (User.IsInRole("User"))
                {
                    var currentUser = User.Identity?.Name;
                    orders = orders.Where(o => o.CustomerName == currentUser).ToList();
                }

                return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // ---------------------------
        // CREATE ORDER
        // ---------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var validationErrors = ValidateCreateOrderDto(dto);
            if (validationErrors.Any())
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = validationErrors });

            try
            {
                var order = await _orderService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, new { success = true, data = order });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo đơn hàng", error = ex.Message });
            }
        }

        // ---------------------------
        // UPDATE ORDER
        // ---------------------------
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
        {
            var validationErrors = ValidateUpdateOrderDto(dto);
            if (validationErrors.Any())
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = validationErrors });

            try
            {
                var order = await _orderService.UpdateAsync(id, dto);
                if (order == null)
                    return NotFound(new { success = false, message = $"Không tìm thấy đơn hàng với ID {id}" });

                return Ok(new { success = true, data = order });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật đơn hàng", error = ex.Message });
            }
        }

        // ---------------------------
        // DELETE ORDER
        // ---------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = $"Không tìm thấy đơn hàng với ID {id}" });

                return Ok(new { success = true, message = "Xóa đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa đơn hàng", error = ex.Message });
            }
        }

        // ---------------------------
        // HELPER: CHECK USER ROLE ACCESS
        // ---------------------------
        private bool UserCanAccessOrder(OrderDto order)
        {
            var currentUser = User.Identity?.Name;
            if (currentUser == null)
                return false;

            if (User.IsInRole("Admin"))
                return true;

            return order.CustomerName == currentUser;
        }

        // ---------------------------
        // VALIDATION HELPERS
        // ---------------------------
        private List<string> ValidateCreateOrderDto(CreateOrderDto dto)
        {
            var errors = new List<string>();

            if (dto.CustomerId <= 0)
                errors.Add("CustomerId phải > 0.");

            if (dto.OrderDetails == null || dto.OrderDetails.Count == 0)
                errors.Add("Đơn hàng phải có ít nhất 1 sản phẩm.");
            else
            {
                for (int i = 0; i < dto.OrderDetails.Count; i++)
                {
                    var item = dto.OrderDetails[i];
                    if (item.ProductId <= 0)
                        errors.Add($"OrderDetails[{i}].ProductId phải > 0.");
                    if (item.Quantity <= 0)
                        errors.Add($"OrderDetails[{i}].Quantity phải > 0.");
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Status))
                errors.Add("Status không được để trống.");

            return errors;
        }

        private List<string> ValidateUpdateOrderDto(UpdateOrderDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Status))
                errors.Add("Status không được để trống.");

            return errors;
        }
    }
}
