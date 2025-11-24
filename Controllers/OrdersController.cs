using Microsoft.AspNetCore.Mvc;
using back_end_cuoi_ky.Dtos;
using back_end_cuoi_ky.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace back_end_cuoi_ky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ Yêu cầu đăng nhập
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Orders (CHỈ ADMIN xem tất cả)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // GET: api/Orders/5 (Admin xem tất cả, User chỉ xem order của mình)
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });
                }

                // ✅ Kiểm tra quyền: User chỉ xem được order của mình
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "User")
                {
                    var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                    if (customerIdClaim == null || order.CustomerId != int.Parse(customerIdClaim))
                    {
                        return Forbid(); // 403 Forbidden
                    }
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // GET: api/Orders/customer/5 (Admin xem của bất kỳ ai, User chỉ xem của mình)
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(int customerId)
        {
            try
            {
                // ✅ Kiểm tra quyền: User chỉ xem được order của mình
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "User")
                {
                    var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                    if (customerIdClaim == null || customerId != int.Parse(customerIdClaim))
                    {
                        return Forbid(); // 403 Forbidden
                    }
                }

                var orders = await _orderService.GetByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // GET: api/Orders/my (User xem order của chính mình)
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            try
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (customerIdClaim == null)
                {
                    return BadRequest(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                int customerId = int.Parse(customerIdClaim);
                var orders = await _orderService.GetByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // POST: api/Orders (Admin tạo cho bất kỳ ai, User chỉ tạo cho mình)
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // ✅ Kiểm tra quyền: User chỉ tạo được order cho chính mình
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "User")
                {
                    var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                    if (customerIdClaim == null || dto.CustomerId != int.Parse(customerIdClaim))
                    {
                        return Forbid(); // 403 Forbidden
                    }
                }

                var order = await _orderService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo đơn hàng", error = ex.Message });
            }
        }

        // PUT: api/Orders/5 (CHỈ ADMIN cập nhật trạng thái)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] UpdateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _orderService.UpdateAsync(id, dto);
                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật đơn hàng", error = ex.Message });
            }
        }

        // DELETE: api/Orders/5 (CHỈ ADMIN xóa được)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });
                }

                return Ok(new { message = "Xóa đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa đơn hàng", error = ex.Message });
            }
        }
    }
}
