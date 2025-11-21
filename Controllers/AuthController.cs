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

        // List Admin & User
        private readonly Dictionary<string, string> Admins = new Dictionary<string, string>
        {
            { "admin", "123" }, { "admin1","123"}, { "admin2","123"}, { "admin3","123"}
        };
        private readonly Dictionary<string, string> Users = new Dictionary<string, string>
        {
            { "user","321"}, { "user1","321"}, { "user2","321"}, { "user3","321"}
        };

        // Lưu token hiện tại của mỗi user
        private static Dictionary<string, (string Token, DateTime Expiry)> _userTokens
            = new Dictionary<string, (string Token, DateTime Expiry)>();

        private const int TokenExpireMinutes = 2;

        public AuthController(JwtService jwt)
        {
            _jwt = jwt;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password are required" });

            string role = null;

            if (Admins.ContainsKey(request.Username) && Admins[request.Username] == request.Password)
                role = "Admin";
            else if (Users.ContainsKey(request.Username) && Users[request.Username] == request.Password)
                role = "User";
            else
                return Unauthorized(new { message = "Invalid username or password" });

            // Nếu token cũ hết hạn → remove khỏi dictionary
            if (_userTokens.ContainsKey(request.Username))
            {
                var (oldToken, expiry) = _userTokens[request.Username];
                if (expiry <= DateTime.UtcNow)
                    _userTokens.Remove(request.Username);
                else
                    return BadRequest(new { message = "User already has a valid token" });
            }

            string token = _jwt.GenerateToken(request.Username, role, TokenExpireMinutes);
            _userTokens[request.Username] = (token, DateTime.UtcNow.AddMinutes(TokenExpireMinutes));

            return Ok(new { username = request.Username, role, token });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] LoginRequest request)
        {
            string username = request.Username;
            string role = null;

            if (Admins.ContainsKey(username))
                role = "Admin";
            else if (Users.ContainsKey(username))
                role = "User";
            else
                return Unauthorized(new { message = "Invalid username" });

            // Kiểm tra token cũ
            if (_userTokens.ContainsKey(username))
            {
                var (oldToken, expiry) = _userTokens[username];

                if (expiry <= DateTime.UtcNow)
                {
                    // Token hết hạn → xóa token cũ để tạo token mới
                    _userTokens.Remove(username);
                }
                else
                {
                    return BadRequest(new { message = "Token still valid, cannot refresh yet" });
                }
            }

            string newToken = _jwt.GenerateToken(username, role, TokenExpireMinutes);
            _userTokens[username] = (newToken, DateTime.UtcNow.AddMinutes(TokenExpireMinutes));

            return Ok(new { username, role, token = newToken });
        }
    }
}
