using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Models;
using treinamento_estagiarios.Data;
using System.Text.Json;

namespace treinamento_estagiarios.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("\n\nRequest received: GET /api/users");

            try
            {
                var users = await _context.Users.Include(u => u.Products).ToListAsync();

                var usersJson = JsonSerializer.Serialize(users, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation($"Response sent: {usersJson}");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving users: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving users" });
            }
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation($"\n\nRequest received: GET /api/users/{id}");

            try
            {
                var user = await _context.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: ID {id}");
                    return NotFound(new { error = "User not found" });
                }

                var userJson = JsonSerializer.Serialize(user, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation($"Response sent: {userJson}");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving user" });
            }
        }
    }
}