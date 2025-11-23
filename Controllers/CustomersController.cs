using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_end_cuoi_ky.Dtos;
using back_end_cuoi_ky.Services;
using System.Text.RegularExpressions;

namespace back_end_cuoi_ky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Chỉ Admin CRUD khách hàng
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // -----------------------
        // GET: api/Customers
        // -----------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                return Ok(new { success = true, data = customers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // -----------------------
        // GET: api/Customers/{id}
        // -----------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                    return NotFound(new { success = false, message = $"Không tìm thấy khách hàng với ID {id}" });

                return Ok(new { success = true, data = customer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // -----------------------
        // POST: api/Customers
        // -----------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var validationErrors = ValidateCustomerDto(dto);
            if (validationErrors.Any())
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = validationErrors });

            try
            {
                var customer = await _customerService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = customer.Id },
                    new { success = true, data = customer });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo khách hàng", error = ex.Message });
            }
        }

        // -----------------------
        // PUT: api/Customers/{id}
        // -----------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            var validationErrors = ValidateCustomerDto(dto);
            if (validationErrors.Any())
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = validationErrors });

            try
            {
                var customer = await _customerService.UpdateAsync(id, dto);
                if (customer == null)
                    return NotFound(new { success = false, message = $"Không tìm thấy khách hàng với ID {id}" });

                return Ok(new { success = true, data = customer });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật khách hàng", error = ex.Message });
            }
        }

        // -----------------------
        // DELETE: api/Customers/{id}
        // -----------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _customerService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = $"Không tìm thấy khách hàng với ID {id}" });

                return Ok(new { success = true, message = "Xóa khách hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa khách hàng", error = ex.Message });
            }
        }

        // -----------------------
        // Validation helper
        // -----------------------
        private List<string> ValidateCustomerDto(dynamic dto)
        {
            var errors = new List<string>();

            // Name
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 2)
                errors.Add("Tên khách hàng phải ít nhất 2 ký tự.");

            // Email
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                !Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Email không hợp lệ.");

            // Phone
            if (string.IsNullOrWhiteSpace(dto.Phone) ||
                !Regex.IsMatch(dto.Phone, @"^\+?\d{9,15}$"))
                errors.Add("Số điện thoại phải từ 9 đến 15 chữ số, có thể bắt đầu bằng +.");

            // Address
            if (string.IsNullOrWhiteSpace(dto.Address))
                errors.Add("Địa chỉ không được để trống.");

            return errors;
        }
    }
}
