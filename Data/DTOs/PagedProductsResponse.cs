

using TestBlazor_FNCourse.Data.Models;

namespace TestBlazor_FNCourse.Data.DTOs
{
    public class PagedProductsResponse
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public List<Product> Products { get; set; }
    }
}
