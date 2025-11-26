using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end_cuoi_ky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public UploadController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        // -----------------------
        // UPLOAD PRODUCT IMAGE
        // -----------------------
        [HttpPost("product-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Không có file được tải lên" });

            // Kiểm tra loại file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { success = false, message = "Chỉ cho phép các file ảnh (jpg, png, gif, webp)" });

            // Kiểm tra kích thước (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { success = false, message = "Kích thước file không vượt quá 5MB" });

            try
            {
                // Tạo thư mục uploads nếu chưa tồn tại
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file duy nhất
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về URL của file
                var fileUrl = $"/uploads/products/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        filename = uniqueFileName,
                        url = fileUrl,
                        message = "Tải lên hình ảnh thành công"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi tải lên file", error = ex.Message });
            }
        }
    }
}
