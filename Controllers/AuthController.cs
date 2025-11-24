using back_end_cuoi_ky.Data;
using back_end_cuoi_ky.Models;
using back_end_cuoi_ky.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using back_end_cuoi_ky.Services;

namespace back_end_cuoi_ky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwt;
        private readonly ApplicationDbContext _context;

        // Static dictionary lưu Admin & User (dùng tạm, production nên dùng database)
        private static readonly Dictionary<string, string> Admins = new()
        {
            { "admin", "123" }, 
            { "admin1", "123" }, 
            { "admin2", "123" }, 
            { "admin3", "123" }
        };

        private static readonly Dictionary<string, string> Users = new()
        {
            { "user", "321" }, 
            { "user1", "321" }, 
            { "user2", "321" }, 
            { "user3", "321" }
        };

        public AuthController(IJwtService jwt, ApplicationDbContext context)
        {
            _jwt = jwt;
            _context = context;
        }

        // -----------------------
        // REGISTER
        // -----------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "Username and password are required" });

            // Check trùng username
            if (Admins.ContainsKey(request.Username) || Users.ContainsKey(request.Username))
                return BadRequest(new { success = false, message = "Username already exists" });

            // ✅ Tạo Customer tương ứng trong database
            var customer = new Models.Customer
            {
                Name = request.Username,
                Email = $"{request.Username}@example.com", // Email mặc định
                Phone = "0000000000", // Phone mặc định
                Address = "",
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Thêm user mới
            Users[request.Username] = request.Password;

            return Ok(new 
            { 
                success = true, 
                message = "Register successful",
                customerId = customer.Id 
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

            string role = null;
            int customerId = 0;

            if (Admins.ContainsKey(request.Username) && Admins[request.Username] == request.Password)
            {
                role = "Admin";
            }
            else if (Users.ContainsKey(request.Username) && Users[request.Username] == request.Password)
            {
                role = "User";
                
                // ✅ Lấy CustomerId từ database
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Name == request.Username);
                
                if (customer == null)
                {
                    return Unauthorized(new { success = false, message = "Customer not found in database" });
                }
                
                customerId = customer.Id;
            }
            else
            {
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }

            // ✅ Tạo token với customerId
            string token = _jwt.GenerateToken(request.Username, role, customerId);

            return Ok(new 
            { 
                success = true, 
                data = new 
                { 
                    username = request.Username, 
                    role, 
                    token,
                    customerId = role == "User" ? customerId : 0
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

    // DTO login/register
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
