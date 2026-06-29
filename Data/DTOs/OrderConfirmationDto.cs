namespace TestBlazor_FNCourse.Data.DTOs
{
    public class OrderConfirmationDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}