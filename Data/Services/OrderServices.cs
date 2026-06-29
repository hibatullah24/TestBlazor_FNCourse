using TestBlazor_FNCourse.Data.DTOs;
using TestBlazor_FNCourse.Data.Models;
using Microsoft.EntityFrameworkCore;
using TestBlazor_FNCourse.Data.Services;

namespace TestBlazor_FNCourse.Data.Services
{


    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggingService<OrderService> _logger;
        private readonly EmailService _emailService;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger, EmailService emailService)
        {
            _context = context;
            _logger = new LoggingService<OrderService>(logger);
            _emailService = emailService;
        }

        public ServiceResult<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request)
        {
            try
            {
                var user = _context.Users.Find(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("PlaceOrder failed: User {UserId} not found.", request.UserId);
                    return ServiceResult<PlaceOrderResponse>.NotFound("User not found.");
                }

                if (request.Items == null || !request.Items.Any())
                {
                    _logger.LogWarning("PlaceOrder failed for User {UserId}: No items provided.", request.UserId);
                    return ServiceResult<PlaceOrderResponse>.BadRequest("Order must contain at least one item.");
                }

                var order = new Order { UId = request.UserId, OrderDate = DateTime.Now };
                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        _logger.LogWarning("PlaceOrder failed: Invalid quantity for Product {ProductId}.", item.ProductId);
                        return ServiceResult<PlaceOrderResponse>.BadRequest($"Invalid quantity for product ID {item.ProductId}.");
                    }

                    var product = _context.Products.Find(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("PlaceOrder failed: Product {ProductId} not found.", item.ProductId);
                        return ServiceResult<PlaceOrderResponse>.NotFound($"Product with ID {item.ProductId} not found.");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        _logger.LogWarning("PlaceOrder failed: Not enough stock for Product {ProductId}.", item.ProductId);
                        return ServiceResult<PlaceOrderResponse>.BadRequest($"Not enough stock for '{product.PName}'. Available: {product.Stock}.");
                    }

                    _context.OrderProducts.Add(new OrderProduct
                    {
                        OId = order.OId,
                        PId = product.PId,
                        Quantity = item.Quantity
                    });

                    product.Stock -= item.Quantity;
                }

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception saveEx)
                {
                    var innerMsg = saveEx.InnerException?.InnerException?.Message
                                ?? saveEx.InnerException?.Message
                                ?? saveEx.Message;
                    _logger.LogError($"SaveChanges failed: {innerMsg}");
                    return ServiceResult<PlaceOrderResponse>.BadRequest($"DB Error: {innerMsg}");
                }

                var fullOrder = _context.Orders
                    .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                    .FirstOrDefault(o => o.OId == order.OId);

                decimal total = fullOrder!.OrderProducts.Sum(op => op.Quantity * op.Product.Price);

                _logger.LogInfo("Order {OrderId} placed successfully for User {UserId}. Total: {Total}.",
                    order.OId, request.UserId, total);

                return ServiceResult<PlaceOrderResponse>.Ok(new PlaceOrderResponse
                {
                    Message = "Order placed successfully.",
                    OrderId = order.OId,
                    TotalAmount = total
                });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.InnerException?.Message
                            ?? ex.InnerException?.Message
                            ?? ex.Message;
                _logger.LogError($"PlaceOrder unexpected error: {innerMsg}");
                return ServiceResult<PlaceOrderResponse>.BadRequest($"Error: {innerMsg}");
            }
        }

        public ServiceResult<List<UserOrderDto>> GetUserOrders(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                _logger.LogWarning("GetUserOrders failed: User {UserId} not found.", userId);
                return ServiceResult<List<UserOrderDto>>.NotFound("User not found.");
            }

            var orders = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.UId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            if (!orders.Any())
            {
                _logger.LogWarning("GetUserOrders: No orders found for User {UserId}.", userId);
                return ServiceResult<List<UserOrderDto>>.NotFound("No orders found.");
            }

            var result = orders.Select(o => new UserOrderDto
            {
                OrderId = o.OId,
                OrderDate = (DateTime)o.OrderDate,
                TotalAmount = o.OrderProducts.Sum(op => op.Quantity * op.Product.Price),
                Items = o.OrderProducts.Select(op => new OrderItemDto
                {
                    ProductId = op.PId,
                    ProductName = op.Product.PName,
                    Quantity = op.Quantity,
                    UnitPrice = op.Product.Price,
                    Subtotal = op.Quantity * op.Product.Price
                }).ToList()
            }).ToList();

            _logger.LogInfo("GetUserOrders: Retrieved {Count} orders for User {UserId}.", orders.Count, userId);
            return ServiceResult<List<UserOrderDto>>.Ok(result);
        }
    }
    
      
    
}