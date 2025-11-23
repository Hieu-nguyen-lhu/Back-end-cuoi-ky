using Microsoft.AspNetCore.Mvc;

namespace back_end_cuoi_ky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestRoleController : ControllerBase
    {
        // Danh sách Admin / User giống AuthController
        private readonly Dictionary<string, string> Admins = new()
        {
            { "admin", "123" }, { "admin1","123"}, { "admin2","123"}, { "admin3","123"}
        };

        private readonly Dictionary<string, string> Users = new()
        {
            { "user","321"}, { "user1","321"}, { "user2","321"}, { "user3","321"}
        };

        // Model nhận input
        public class TestRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private string GetRole(string username, string password)
        {
            if (Admins.ContainsKey(username) && Admins[username] == password)
                return "Admin";

            if (Users.ContainsKey(username) && Users[username] == password)
                return "User";

            return null;
        }

        // =============== TEST ADMIN ONLY ===============
        [HttpPost("adminTest")]
        public IActionResult AdminTest([FromBody] TestRequest req)
        {
            string role = GetRole(req.Username, req.Password);

            if (role == null)
                return Unauthorized("Invalid username or password");

            if (role != "Admin")
                return Forbid("You are not Admin");

            return Ok($"Admin access OK! Welcome {req.Username}");
        }

        // =============== TEST USER ONLY ===============
        [HttpPost("userTest")]
        public IActionResult UserTest([FromBody] TestRequest req)
        {
            string role = GetRole(req.Username, req.Password);

            if (role == null)
                return Unauthorized("Invalid username or password");

            if (role != "User")
                return Forbid("You are not User");

            return Ok($"User access OK! Welcome {req.Username}");
        }

        // =============== TEST ADMIN + USER đều vào được ===============
        [HttpPost("bothTest")]
        public IActionResult BothTest([FromBody] TestRequest req)
        {
            string role = GetRole(req.Username, req.Password);

            if (role == null)
                return Unauthorized("Invalid username or password");

            return Ok($"Both allowed. You are {role}");
        }
    }
}
