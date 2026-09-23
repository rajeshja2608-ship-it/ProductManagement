using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Data;
using ProductManagement.API.DTOs;
using ProductManagement.API.Models;

namespace ProductManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        //  [Authorize(Policy = "JwtAndHmac")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                _logger.LogInformation("HTTP GET request received for fetching all products.");
                var product = await _context.Products.OrderByDescending(x => x.ProductId).ToListAsync();
                _logger.LogInformation("Response: {@Products}", product);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occured while processing GET all Product Request.");
                return StatusCode(500, "An Error Occured While fetching products");
            }

        }

        [HttpGet("{id}")]
        //   [Authorize(Policy = "JwtAndHmac")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = "Product Not Found" });
            }
            return Ok(product);
        }

        [HttpPost]
        //   [Authorize(Policy = "JwtAndHmacAdmin")]
        public async Task<IActionResult> CreateProduct(
            [FromBody] ProductDto model)
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Product created successfully",
                ProductId = product.ProductId
            });
        }


        [HttpPut("{id}")]
        //   [Authorize(Policy = "JwtAndHmacAdmin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto model)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Quantity = model.Quantity;
            product.IsActive = model.IsActive;
            product.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Product updated successfully" });
        }


        [HttpDelete("{id}")]
        //  [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted successfully" });
        }
    }
}
