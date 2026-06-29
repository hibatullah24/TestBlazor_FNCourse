

namespace TestBlazor_FNCourse.Data.DTOs
{
    public class ProductReviewsResponse
    {
        public double OverallRating { get; set; }
        public int TotalReviews { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public List<ReviewDto> Reviews { get; set; }
    }
}
