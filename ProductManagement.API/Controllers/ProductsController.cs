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

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var product = await _context.Products.OrderByDescending(x => x.ProductId).ToListAsync();
            return Ok(product);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product==null)
            {
                return NotFound(new {Message="Product Not Found"});
            }
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto model)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new {Message = "Product not found"});
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new {Message = "Product not found"});
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new {Message = "Product deleted successfully"});
        }
    }
}
