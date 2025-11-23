using back_end_cuoi_ky.Models;
using back_end_cuoi_ky.Services;
using Microsoft.AspNetCore.Mvc;

namespace back_end_cuoi_ky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwt;

        // Static dictionary lưu Admin & User
        private static readonly Dictionary<string, string> Admins = new()
        {
            { "admin", "123" }, { "admin1","123"}, { "admin2","123"}, { "admin3","123"}
        };

        private static readonly Dictionary<string, string> Users = new()
        {
            { "user","321"}, { "user1","321"}, { "user2","321"}, { "user3","321"}
        };

        // Lưu token hiện tại của mỗi user
        private static readonly Dictionary<string, (string Token, DateTime Expiry)> _userTokens
            = new();

        private const int TokenExpireMinutes = 30;

        public AuthController(JwtService jwt)
        {
            _jwt = jwt;
        }

        // -----------------------
        // REGISTER
        // -----------------------
        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "Username and password are required" });

            // Check trùng username
            if (Admins.ContainsKey(request.Username) || Users.ContainsKey(request.Username))
                return BadRequest(new { success = false, message = "Username already exists" });

            // Thêm user mới
            Users[request.Username] = request.Password;

            return Ok(new { success = true, message = "Register successful" });
        }

        // -----------------------
        // LOGIN
        // -----------------------
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "Username and password are required" });

            string role = null;

            if (Admins.ContainsKey(request.Username) && Admins[request.Username] == request.Password)
                role = "Admin";
            else if (Users.ContainsKey(request.Username) && Users[request.Username] == request.Password)
                role = "User";
            else
                return Unauthorized(new { success = false, message = "Invalid username or password" });

            // Token cũ
            if (_userTokens.ContainsKey(request.Username))
            {
                var (oldToken, expiry) = _userTokens[request.Username];
                if (expiry > DateTime.UtcNow)
                    return BadRequest(new { success = false, message = "User already has a valid token" });
                else
                    _userTokens.Remove(request.Username);
            }

            string token = _jwt.GenerateToken(request.Username, role, TokenExpireMinutes);
            _userTokens[request.Username] = (token, DateTime.UtcNow.AddMinutes(TokenExpireMinutes));

            return Ok(new { success = true, data = new { username = request.Username, role, token } });
        }

        // -----------------------
        // REFRESH TOKEN
        // -----------------------
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { success = false, message = "Username is required" });

            string role = null;
            if (Admins.ContainsKey(request.Username)) role = "Admin";
            else if (Users.ContainsKey(request.Username)) role = "User";
            else return Unauthorized(new { success = false, message = "Invalid username" });

            if (_userTokens.ContainsKey(request.Username))
            {
                var (oldToken, expiry) = _userTokens[request.Username];
                if (expiry > DateTime.UtcNow)
                    return BadRequest(new { success = false, message = "Token still valid, cannot refresh yet" });

                _userTokens.Remove(request.Username);
            }

            string newToken = _jwt.GenerateToken(request.Username, role, TokenExpireMinutes);
            _userTokens[request.Username] = (newToken, DateTime.UtcNow.AddMinutes(TokenExpireMinutes));

            return Ok(new { success = true, data = new { username = request.Username, role, token = newToken } });
        }
    }

    // -----------------------
    // DTO login/register
    // -----------------------
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
