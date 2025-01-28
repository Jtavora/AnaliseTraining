using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Models;
using treinamento_estagiarios.Data;
using System.Text.Json;

namespace treinamento_estagiarios.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            _logger.LogInformation("\n\nRequest received: GET /api/products");

            try
            {
                var products = await _context.Products.Include(p => p.User).ToListAsync();

                var productsJson = JsonSerializer.Serialize(products, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation($"Response sent: {productsJson}");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\nError retrieving products: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving products" });
            }
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            _logger.LogInformation($"\n\nRequest received: GET /api/products/{id}");

            try
            {
                var product = await _context.Products.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: ID {id}");
                    return NotFound(new { error = "Product not found" });
                }

                var productJson = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation($"Response sent: {productJson}");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving product {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving product" });
            }
        }
    }
}