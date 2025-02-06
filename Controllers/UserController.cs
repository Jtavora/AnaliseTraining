using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Models;
using treinamento_estagiarios.Data;
using treinamento_estagiarios.Request;
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
                var users = await _context.Users
                    .Include(u => u.UserProducts)
                        .ThenInclude(up => up.Product)
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogWarning("No users found.");
                    return NotFound(new { error = "No users found" });
                }

                var usersDto = users.Select(user => new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Products = user.UserProducts.Select(up => new ProductDTO
                    {
                        Id = up.Product.Id,
                        Name = up.Product.Name,
                        Price = up.Product.Price
                    }).ToList()
                }).ToList();

                // se algum usuario nao estiver produto associado retorna erro
                if (usersDto.Any(u => !u.Products.Any()))
                {
                    _logger.LogWarning($"Some users have no products associated: {JsonSerializer.Serialize(usersDto)}");
                    return StatusCode(500, new { error = "Error!" });
                }

                _logger.LogInformation($"Response sent: {JsonSerializer.Serialize(usersDto)}");

                return Ok(usersDto);
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
                var user = await _context.Users
                    .Include(u => u.UserProducts)
                        .ThenInclude(up => up.Product)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: ID {id}");
                    return NotFound(new { error = "User not found" });
                }

                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Products = user.UserProducts.Select(up => new ProductDTO
                    {
                        Id = up.Product.Id,
                        Name = up.Product.Name,
                        Price = up.Product.Price
                    }).ToList()
                };

                // se usuario nao tiver produto associado retorna erro
                if (!userDto.Products.Any())
                {
                    _logger.LogWarning($"User has no products associated: {JsonSerializer.Serialize(userDto)}");
                    return StatusCode(500, new { error = "Error!" });
                }

                _logger.LogInformation($"Response sent: {JsonSerializer.Serialize(userDto)}");

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving user" });
            }
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] MockUserRequest newUser)
        {
            _logger.LogInformation("\n\nRequest received: POST /api/users");

            try
            {
                if (string.IsNullOrWhiteSpace(newUser.Name) || 
                    string.IsNullOrWhiteSpace(newUser.Email) || 
                    newUser.ProductsId == null || 
                    !newUser.ProductsId.Any())
                {
                    _logger.LogWarning("Required fields missing. Payload: " + JsonSerializer.Serialize(newUser));
                    return BadRequest(new { error = "Payload error!" });
                }

                var emailExists = await _context.Users.AnyAsync(u => u.Email == newUser.Email);
                if (emailExists)
                {
                    _logger.LogWarning($"Email already in use: {newUser.Email}");
                    return Conflict(new { error = "Email already in use" });
                }

                var products = await _context.Products
                    .Where(p => newUser.ProductsId.Contains(p.Id))
                    .ToListAsync();

                if (!products.Any())
                {
                    _logger.LogWarning("No valid products found for user creation.");
                    return BadRequest(new { error = "Invalid products provided" });
                }

                var user = new User
                {
                    Name = newUser.Name,
                    Email = newUser.Email,
                    UserProducts = products.Select(p => new UserProduct { ProductId = p.Id }).ToList()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created: {user.Name} ({user.Email})");

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while creating user" });
            }
        }
    }
}