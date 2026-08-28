namespace ProductManagement.API.DTOs
{
    public class ProductDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}