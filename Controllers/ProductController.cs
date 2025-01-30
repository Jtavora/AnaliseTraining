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
        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            _logger.LogInformation($"\n\nRequest received: PUT /api/products/{id}");

            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: ID {id}");
                    return NotFound(new { error = "Product not found" });
                }

                // Validations
                if (updatedProduct.Price < 0)
                {
                    _logger.LogWarning($"Invalid price: {updatedProduct.Price}");
                    return BadRequest(new { error = "Price cannot be negative" });
                }

                if (string.IsNullOrEmpty(updatedProduct.Name) || updatedProduct.Name.Length < 3)
                {
                    _logger.LogWarning($"Invalid product name: {updatedProduct.Name}");
                    return BadRequest(new { error = "Product name must be at least 3 characters long" });
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

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation($"\n\nRequest received: DELETE /api/products/{id}");

            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: ID {id}");
                    return NotFound(new { error = "Product not found" });
                }

                // Verifica se o produto tem dependências
                if (product.UserId == 0)  // Exemplo de validação fictícia de dependência
                {
                    _logger.LogWarning($"Cannot delete product with no user assigned: ID {id}");
                    return Conflict(new { error = "Cannot delete product with no user assigned" });
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

        // GET: api/products/search?name=some_search_term
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string name)
        {
            _logger.LogInformation("\n\nRequest received: GET /api/products/search");

            try
            {
                // Verifica se o termo de pesquisa foi fornecido
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Search name must not be empty" });
                }

                // Realiza a busca com LIKE
                var products = await _context.Products
                                            .Where(p => p.Name.Contains(name))
                                            .ToListAsync();

                if (products == null || !products.Any())
                {
                    return NotFound(new { message = "No products found matching the search name" });
                }

                // Retorna os resultados
                _logger.LogInformation($"Response sent: {products.Count} products found");
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