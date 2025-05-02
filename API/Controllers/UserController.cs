using Microsoft.AspNetCore.Mvc;
using Tubes_1_KPL.Model;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static List<User> users = new List<User>
        {
            new User { Id = 1, Username = "admin", Password = "admin123", IsLoggedIn = false },
            new User { Id = 2, Username = "user", Password = "user123", IsLoggedIn = false }
        };

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists");

            user.Id = users.Count + 1;
            user.IsLoggedIn = false;
            users.Add(user);

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var existingUser = users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (existingUser == null)
                return Unauthorized();

            existingUser.IsLoggedIn = true;
            return Ok("Login successful");
        }

        [HttpPost("logout/{username}")]
        public IActionResult Logout(string username)
        {
            var existingUser = users.FirstOrDefault(u => u.Username == username);
            if (existingUser == null || !existingUser.IsLoggedIn)
                return BadRequest("User not logged in or does not exist");

            existingUser.IsLoggedIn = false;
            return Ok("Logout successful");
        }

        [HttpGet("all")]
        public IActionResult GetAll() => Ok(users);
    }
}
