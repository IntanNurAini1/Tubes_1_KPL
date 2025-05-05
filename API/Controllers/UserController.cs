using Microsoft.AspNetCore.Mvc;
using Tubes_1_KPL.Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            var users = UserDataHelper.LoadUsers();

            if (users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists");

            user.Id = users.Count + 1;
            user.IsLoggedIn = false;
            users.Add(user);

            UserDataHelper.SaveUsers(users);
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var users = UserDataHelper.LoadUsers();

            var existingUser = users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (existingUser == null)
                return Unauthorized();

            existingUser.IsLoggedIn = true;
            UserDataHelper.SaveUsers(users);
            return Ok("Login successful");
        }

        [HttpPost("logout/{username}")]
        public IActionResult Logout(string username)
        {
            var users = UserDataHelper.LoadUsers();

            var existingUser = users.FirstOrDefault(u => u.Username == username);
            if (existingUser == null || !existingUser.IsLoggedIn)
                return BadRequest("User not logged in or does not exist");

            existingUser.IsLoggedIn = false;
            UserDataHelper.SaveUsers(users);
            return Ok("Logout successful");
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var users = UserDataHelper.LoadUsers();
            return Ok(users);
        }
    }
}