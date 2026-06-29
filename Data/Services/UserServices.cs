using TestBlazor_FNCourse.Data.DTOs;
using TestBlazor_FNCourse.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace TestBlazor_FNCourse.Data.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggingService<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = new LoggingService<UserService>(logger);
        }

        // Admin only — view any user
        public ServiceResult<UserDto> GetUserById(int id, int adminId)
        {
            var requestingUser = _context.Users.Find(adminId);
            if (requestingUser == null || requestingUser.Role != "Admin")
                return ServiceResult<UserDto>.Unauthorized("Access denied.");

            var user = _context.Users.Find(id);
            if (user == null)
                return ServiceResult<UserDto>.NotFound("User not found.");

            var orders = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.UId == id)
                .ToList()
                .Select(o => new UserOrderDto
                {
                    OrderId = o.OId,
                    OrderDate = (DateTime)o.OrderDate,
                    TotalAmount = o.TotalAmount
                }).ToList();

            var dto = new UserDto
            {
                UserId = user.UId,
                Name = user.UName,
                Email = user.Email,
                Phone = user.Phone,
                Orders = orders
            };

            _logger.LogInfo("Admin {AdminId} retrieved User {UserId}.", adminId, id);
            return ServiceResult<UserDto>.Ok(dto);
        }

        // User views own profile — no admin check
        public ServiceResult<UserDto> GetOwnProfile(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return ServiceResult<UserDto>.NotFound("User not found.");

            var orders = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.UId == userId)
                .ToList()
                .Select(o => new UserOrderDto
                {
                    OrderId = o.OId,
                    OrderDate = (DateTime)o.OrderDate,
                    TotalAmount = o.TotalAmount
                }).ToList();

            var dto = new UserDto
            {
                UserId = user.UId,
                Name = user.UName,
                Email = user.Email,
                Phone = user.Phone,
                Orders = orders
            };

            _logger.LogInfo("User {UserId} viewed own profile.", userId);
            return ServiceResult<UserDto>.Ok(dto);
        }

        // Admin only — view all users
        public ServiceResult<List<UserDto>> GetAllUsers()
        {
            var users = _context.Users.ToList().Select(u => new UserDto
            {
                UserId = u.UId,
                Name = u.UName,
                Email = u.Email,
                Phone = u.Phone,
                Orders = _context.Orders
                    .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                    .Where(o => o.UId == u.UId)
                    .ToList()
                    .Select(o => new UserOrderDto
                    {
                        OrderId = o.OId,
                        OrderDate = (DateTime)o.OrderDate,
                        TotalAmount = o.TotalAmount
                    }).ToList()
            }).ToList();

            if (!users.Any())
            {
                _logger.LogWarning("GetAllUsers: No users found.");
                return ServiceResult<List<UserDto>>.NotFound("No users found.");
            }

            _logger.LogInfo("Retrieved all users. Count: {Count}.", users.Count);
            return ServiceResult<List<UserDto>>.Ok(users);
        }
    }
}