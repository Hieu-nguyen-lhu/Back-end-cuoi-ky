using back_end_cuoi_ky.Data;
using back_end_cuoi_ky.Models;
using back_end_cuoi_ky.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end_cuoi_ky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwt;
        private readonly ApplicationDbContext _context;

        public AuthController(IJwtService jwt, ApplicationDbContext context)
        {
            _jwt = jwt;
            _context = context;
        }

        // -----------------------
        // REGISTER
        // -----------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "Username and password are required" });

            // Check trùng username
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
                return BadRequest(new { success = false, message = "Username already exists" });

            // ✅ Tạo Customer tương ứng trong database (nếu register là User)
            Customer? customer = null;
            if (request.Role == "User")
            {
                customer = new Models.Customer
                {
                    Name = request.Username,
                    Email = request.Email ?? $"{request.Username}@example.com",
                    Phone = request.Phone ?? "0000000000",
                    Address = request.Address ?? "",
                    CreatedAt = DateTime.Now
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Tạo User mới
            var newUser = new Models.User
            {
                Username = request.Username,
                Password = request.Password, // ⚠️ Production: hash password with BCrypt!
                Role = request.Role ?? "User",
                Email = request.Email,
                CustomerId = customer?.Id,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                success = true, 
                message = "Register successful",
                userId = newUser.Id,
                customerId = customer?.Id 
            });
        }

        // -----------------------
        // LOGIN
        // -----------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "Username and password are required" });

            // Tìm User trong database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || user.Password != request.Password)
                return Unauthorized(new { success = false, message = "Invalid username or password" });

            int customerId = 0;
            
            // Nếu là User, lấy CustomerId
            if (user.Role == "User" && user.CustomerId.HasValue)
            {
                customerId = user.CustomerId.Value;
            }

            // Tạo token
            string token = _jwt.GenerateToken(request.Username, user.Role, customerId);

            return Ok(new 
            { 
                success = true, 
                data = new 
                { 
                    username = request.Username, 
                    role = user.Role, 
                    token,
                    customerId = user.Role == "User" ? customerId : 0
                } 
            });
        }

        // -----------------------
        // LOGOUT (Optional - client-side xóa token)
        // -----------------------
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // JWT stateless nên logout chủ yếu xử lý ở client (xóa token)
            return Ok(new { success = true, message = "Logout successful" });
        }
    }

    // DTO for login
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // DTO for register
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; } = "User"; // Default: User, can be "Admin"
    }
}
