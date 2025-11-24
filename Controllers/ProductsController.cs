using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_end_cuoi_ky.Dtos;
using back_end_cuoi_ky.Services;

namespace back_end_cuoi_ky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // -----------------------
        // GET ALL PRODUCTS
        // -----------------------
        [HttpGet]
        [Authorize] // Any logged-in user
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // -----------------------
        // GET PRODUCT BY ID
        // -----------------------
        [HttpGet("{id}")]
        [Authorize] // Any logged-in user
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm với ID {id}" });

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        // -----------------------
        // CREATE PRODUCT
        // -----------------------
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var validationErrors = ValidateProductDto(dto);
            if (validationErrors.Any())
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = validationErrors });

            try
            {
                var product = await _productService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = product.Id },
                    new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo sản phẩm", error = ex.Message });
            }
        }

        // -----------------------
        // UPDATE PRODUCT
        // -----------------------
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var validationErrors = ValidateProductDto(dto);
            if (validationErrors.Any())
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = validationErrors });

            try
            {
                var product = await _productService.UpdateAsync(id, dto);
                if (product == null)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm với ID {id}" });

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật sản phẩm", error = ex.Message });
            }
        }

        // -----------------------
        // DELETE PRODUCT
        // -----------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm với ID {id}" });

                return Ok(new { success = true, message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa sản phẩm", error = ex.Message });
            }
        }

        // -----------------------
        // Validation helper
        // -----------------------
        private List<string> ValidateProductDto(dynamic dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 2)
                errors.Add("Tên sản phẩm phải ít nhất 2 ký tự.");

            if (dto.Price == null || dto.Price < 0)
                errors.Add("Giá sản phẩm phải >= 0.");

            if (dto.Stock == null || dto.Stock < 0)
                errors.Add("Số lượng tồn kho phải >= 0.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add("Mô tả không được để trống.");

            return errors;
        }
    }
}
