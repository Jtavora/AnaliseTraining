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

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            _logger.LogInformation("\n\nRequest received: POST /api/users");

            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Email already in use: {newUser.Email}");
                    return Conflict(new { error = "Email already in use" });
                }

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created: {newUser.Name} ({newUser.Email})");
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while creating user" });
            }
        }
    }
}