using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Models;
using treinamento_estagiarios.Data;
using System.Text.Json;
using treinamento_estagiarios.Request;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            _logger.LogInformation("\n\nRequest received: GET /api/products");

            try
            {
                var products = await _context.Products.Include(p => p.UserProducts).ThenInclude(up => up.User).ToListAsync();

                if (!products.Any())
                {
                    _logger.LogWarning("No products found");
                    return NotFound(new { error = "No products found!" });
                }

                foreach (var product in products)
                {
                    if (product.Price < 0)
                    {
                        _logger.LogWarning($"Invalid price for product {product.Id}: {product.Price}");
                        return BadRequest(new { error = "Error!" });
                    }
                }

                _logger.LogInformation($"Response sent: {JsonSerializer.Serialize(products)}");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving products: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving products" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            _logger.LogInformation($"\n\nRequest received: GET /api/products/{id}");

            try
            {
                var product = await _context.Products.Include(p => p.UserProducts).ThenInclude(up => up.User).FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: ID {id}");
                    return NotFound(new { error = "Product not found!" });
                }
                else if (product.Price < 0)
                {
                    _logger.LogWarning($"Invalid price: {product.Price}");
                    return BadRequest(new { error = "Error!" });
                }

                _logger.LogInformation($"Response sent: {JsonSerializer.Serialize(product)}");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving product {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while retrieving product" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] MockProductResquest updatedProduct)
        {
            _logger.LogInformation($"\n\nRequest received: PUT /api/products/{id}");

            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: ID {id}");
                    return NotFound(new { error = "Product not found" });
                }

                if (updatedProduct.Price < 0 || string.IsNullOrEmpty(updatedProduct.Name) || updatedProduct.Name.Length < 3)
                {
                    _logger.LogWarning("Invalid product data");
                    return BadRequest(new { error = "Invalid product data!" });
                }

                product.Name = updatedProduct.Name;
                product.Price = updatedProduct.Price;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product updated: {product.Name} ({product.Price:C})");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while updating product" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation($"\n\nRequest received: DELETE /api/products/{id}");

            try
            {
                var product = await _context.Products.Include(p => p.UserProducts).FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: ID {id}");
                    return NotFound(new { error = "Product not found" });
                }

                if (product.UserProducts.Any())
                {
                    _logger.LogWarning($"Cannot delete product assigned to users: ID {id}");
                    return Conflict(new { error = "Cannot delete product assigned to users" });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product deleted: {product.Name} ({product.Price:C})");
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while deleting product" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string name)
        {
            _logger.LogInformation("\n\nRequest received: GET /api/products/search");

            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Search name must not be empty" });
                }

                var products = await _context.Products
                    .Where(p => p.Name.Contains(name))
                    .ToListAsync();

                if (!products.Any())
                {
                    return NotFound(new { message = "No products found matching the search name" });
                }

                _logger.LogInformation($"Response sent: {JsonSerializer.Serialize(products)}");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while searching products" });
            }
        }
    }
}