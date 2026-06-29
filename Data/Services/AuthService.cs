using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions;
using TestBlazor_FNCourse;
using TestBlazor_FNCourse.Data;
using TestBlazor_FNCourse.Data.DTOs;
using TestBlazor_FNCourse.Data.Models;
using TestBlazor_FNCourse.Data.Services;
using TestBlazor_FNCourse.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TestBlazor_FNCourse.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly LoggingService<AuthService> _logger;


        public AuthService(ApplicationDbContext context,  EmailService emailService, ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = new LoggingService<AuthService>(logger);
        }

        public RegisterResult Register(RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
                    throw new Exception("Name and email are required.");

                if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new Exception("Invalid email format.");

                var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Register failed: Email {Email} already registered.", request.Email);
                     throw new Exception("Email is already registered.");
                }

                if (string.IsNullOrWhiteSpace(request.Password) ||
                    !Regex.IsMatch(request.Password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$"))
                    throw new Exception("Password must be at least 8 characters with uppercase, lowercase, and a number.");

                var user = new User
                {
                    UName = request.Name,
                    Email = request.Email,
                    Password = HashPassword(request.Password),
                    Phone = request.Phone,
                    Role = "User",
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                _context.SaveChanges();


                _emailService.SendEmail(
                        user.Email,
                        "Welcome to E-Commerce API",
                        $"<h2>Welcome, {user.UName}!</h2><p>Your account has been created successfully.</p>"
                 );

                _logger.LogInfo("User {UserId} registered successfully.", user.UId);
                return new RegisterResult
                {
                    Message = "Registration successful.",
                    UserId = user.UId,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Register failed with an unexpected error.", ex);
                throw new Exception(ex.Message); 
            }
        }

        public LoginResult Login(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    throw new Exception("Email and Password are required.");

                var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
                Console.WriteLine($"Stored hash: {user?.Password}");
                Console.WriteLine($"Input hash:  {HashPassword(request.Password)}");

                if (user == null)
                {
                    _logger.LogWarning("Login failed: Email {Email} not found.", request.Email);
                    throw new Exception("Email not found.");
                }

                bool isPasswordValid = user.Password == HashPassword(request.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for Email {Email}.", request.Email);
                    throw new Exception("Invalid password.");
                }


                _emailService.SendEmail(
                        user.Email,
                        "New Login Detected",
                        $"<h2>Hello, {user.UName}!</h2><p>A new login was detected on your account at {DateTime.Now}.</p>"
                    );

                _logger.LogInfo("User {UserId} logged in successfully.", user.UId);

                return new LoginResult
                {
                    message = $"Welocome, {user.UName}",
                    UserId = user.UId,
                    Role = user.Role,
                    UserName = user.UName

                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Login failed with an unexpected error.", ex);

                throw;
            }
        }

        public string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

    }
}