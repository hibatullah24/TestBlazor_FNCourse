using TestBlazor_FNCourse.Data.DTOs;
using TestBlazor_FNCourse.Data.Models;


namespace TestBlazor_FNCourse.Data.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggingService<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = new LoggingService<ProductService>(logger);
        }

        public ServiceResult<string> AddProduct(AddProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PName))
                return ServiceResult<string>.BadRequest("Product name is required.");

            if (string.IsNullOrWhiteSpace(request.Description))
                return ServiceResult<string>.BadRequest("Product description is required.");

            if (request.Price <= 0)
                return ServiceResult<string>.BadRequest("Product price must be greater than zero.");

            if (request.Stock < 0)
                return ServiceResult<string>.BadRequest("Product stock cannot be negative.");

            var product = new Product
            {
                PName = request.PName,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            _logger.LogInfo("Product {ProductId} '{ProductName}' added.", product.PId, product.PName);
            return ServiceResult<string>.Ok("Product added successfully.");
        }

        public ServiceResult<string> UpdateProduct(int id, UpdateProductRequest request)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                _logger.LogWarning("UpdateProduct: Product {ProductId} not found.", id);
                return ServiceResult<string>.NotFound("Product not found.");
            }

            if (request.Price <= 0)
                return ServiceResult<string>.BadRequest("Price must be greater than zero.");

            if (request.Stock < 0)
                return ServiceResult<string>.BadRequest("Stock cannot be negative.");

            product.Price = request.Price;
            product.Stock = request.Stock;
            _context.Products.Update(product);
            _context.SaveChanges();

            _logger.LogInfo("Product {ProductId} updated. New Price: {Price}, New Stock: {Stock}.", product.PId, product.Price, product.Stock);
            return ServiceResult<string>.Ok("Product updated successfully.");
        }

        public ServiceResult<PagedProductsResponse> GetAllProducts(int page)
        {
            const int pageSize = 10;
            int totalCount = _context.Products.Count();

            if (totalCount == 0)
            {
                _logger.LogWarning("GetAllProducts: No products found.");
                return ServiceResult<PagedProductsResponse>.NotFound("No products available.");
            }

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1 || page > totalPages)
                return ServiceResult<PagedProductsResponse>.BadRequest($"Invalid page number. Valid range: 1 - {totalPages}.");

            var products = _context.Products
                .OrderBy(p => p.PName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInfo("GetAllProducts: Returned page {Page} of {TotalPages}.", page, totalPages);

            return ServiceResult<PagedProductsResponse>.Ok(new PagedProductsResponse
            {
                Page = page,
                TotalPages = totalPages,
                Products = products
            });
        }

        public ServiceResult<Product> GetProductById(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                _logger.LogWarning("GetProductById: Product {ProductId} not found.", id);
                return ServiceResult<Product>.NotFound("Product not found.");
            }

            _logger.LogInfo("GetProductById: Product {ProductId} retrieved.", id);
            return ServiceResult<Product>.Ok(product);
        }
    }
}