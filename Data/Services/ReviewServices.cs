using Microsoft.EntityFrameworkCore;
using TestBlazor_FNCourse.Data.DTOs;
using TestBlazor_FNCourse.Data.Models;


namespace TestBlazor_FNCourse.Data.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggingService<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = new LoggingService<ReviewService>(logger);
        }

        public ServiceResult<AddReviewResponse> AddReview(AddReviewRequest request)
        {
            var product = _context.Products.Find(request.PId);
            if (product == null)
            {
                _logger.LogWarning("AddReview failed: Product {ProductId} not found.", request.PId);
                return ServiceResult<AddReviewResponse>.NotFound("Product not found.");
            }

            var user = _context.Users.Find(request.UId);
            if (user == null)
                return ServiceResult<AddReviewResponse>.NotFound("User not found.");

            var existingReview = _context.Reviews.FirstOrDefault(r => r.PId == request.PId && r.UId == request.UId);
            if (existingReview != null)
            {
                _logger.LogWarning("AddReview failed: User {UserId} already reviewed Product {ProductId}.", request.UId, request.PId);
                return ServiceResult<AddReviewResponse>.BadRequest("You have already reviewed this product.");
            }

            var hasPurchased = _context.OrderProducts.Any(op => op.PId == request.PId && op.Order.UId == request.UId);
            if (!hasPurchased)
            {
                _logger.LogWarning("AddReview failed: User {UserId} has not purchased Product {ProductId}.", request.UId, request.PId);
                return ServiceResult<AddReviewResponse>.BadRequest("You can only review products you have purchased.");
            }

            if (request.Rating < 1 || request.Rating > 5)
                return ServiceResult<AddReviewResponse>.BadRequest("Rating must be between 1 and 5.");

            if (string.IsNullOrWhiteSpace(request.Comment))
                return ServiceResult<AddReviewResponse>.BadRequest("Comment is required.");

            var review = new Review
            {
                UId = request.UId,
                PId = request.PId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();
            RecalculateProductRating(request.PId);
            _context.SaveChanges();

            _logger.LogInfo("User {UserId} added review for Product {ProductId}.", request.UId, request.PId);
            return ServiceResult<AddReviewResponse>.Ok(new AddReviewResponse
            {
                Message = "Review added successfully.",
                ReviewId = review.RId
            });
        }

        public ServiceResult<string> EditReview(int reviewId, int userId, EditReviewRequest request)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.RId == reviewId);
            if (review == null)
            {
                _logger.LogWarning("EditReview failed: Review {ReviewId} not found.", reviewId);
                return ServiceResult<string>.NotFound("Review not found.");
            }

            if (review.UId != userId)
            {
                _logger.LogWarning("EditReview failed: User {UserId} tried to edit Review {ReviewId} they don't own.", userId, reviewId);
                return ServiceResult<string>.Unauthorized("You can only edit your own reviews.");
            }

            if (request.Rating < 1 || request.Rating > 5)
                return ServiceResult<string>.BadRequest("Rating must be between 1 and 5.");

            if (string.IsNullOrWhiteSpace(request.Comment))
                return ServiceResult<string>.BadRequest("Comment is required.");

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.ReviewDate = DateTime.Now;
            _context.Reviews.Update(review);
            RecalculateProductRating(review.PId);
            _context.SaveChanges();

            _logger.LogInfo("User {UserId} edited Review {ReviewId}.", userId, reviewId);
            return ServiceResult<string>.Ok("Review updated successfully.");
        }

        public ServiceResult<string> DeleteReview(int reviewId, int userId)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.RId == reviewId);
            if (review == null)
            {
                _logger.LogWarning("DeleteReview failed: Review {ReviewId} not found.", reviewId);
                return ServiceResult<string>.NotFound("Review not found.");
            }

            if (review.UId != userId)
            {
                _logger.LogWarning("DeleteReview failed: User {UserId} tried to delete Review {ReviewId} they don't own.", userId, reviewId);
                return ServiceResult<string>.Unauthorized("You can only delete your own reviews.");
            }

            int productId = review.PId;
            _context.Reviews.Remove(review);
            RecalculateProductRating(productId);
            _context.SaveChanges();

            _logger.LogInfo("User {UserId} deleted Review {ReviewId}.", userId, reviewId);
            return ServiceResult<string>.Ok("Review deleted successfully.");
        }

        public ServiceResult<ProductReviewsResponse> GetProductReviews(int productId, int page)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                _logger.LogWarning("GetProductReviews: Product {ProductId} not found.", productId);
                return ServiceResult<ProductReviewsResponse>.NotFound("Product not found.");
            }

            const int pageSize = 5;
            int total = _context.Reviews.Count(r => r.PId == productId);

            if (total == 0)
            {
                _logger.LogWarning("GetProductReviews: No reviews for Product {ProductId}.", productId);
                return ServiceResult<ProductReviewsResponse>.NotFound("No reviews found for this product.");
            }

            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (page < 1 || page > totalPages)
                return ServiceResult<ProductReviewsResponse>.BadRequest($"Invalid page. Valid range: 1 - {totalPages}.");

            var reviews = _context.Reviews
                .Where(r => r.PId == productId)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.RId,
                    UserId = r.UId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                }).ToList();

            _logger.LogInfo("GetProductReviews: Returned page {Page} for Product {ProductId}.", page, productId);

            return ServiceResult<ProductReviewsResponse>.Ok(new ProductReviewsResponse
            {
                OverallRating = (double)product.OverallRating,
                TotalReviews = total,
                Page = page,
                TotalPages = totalPages,
                Reviews = reviews
            });
        }

        private void RecalculateProductRating(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return;

            var reviews = _context.Reviews.Where(r => r.PId == productId).ToList();
            product.overriddenRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
            _context.Products.Update(product);
        }


        public ServiceResult<List<ReviewDto>> GetAllReviewsForAdmin()
        {
            var reviews = _context.Reviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.RId,
                    UserId = r.UId,
                    ProductName = r.Product.PName,
                    Rating = r.Rating,
                    Comment = r.Comment ?? "",
                    ReviewDate = r.ReviewDate ?? DateTime.Now
                })
                .ToList();

            _logger.LogInfo("Admin fetched all reviews. Count: {Count}.", reviews.Count);
            return ServiceResult<List<ReviewDto>>.Ok(reviews);
        }
    }
}