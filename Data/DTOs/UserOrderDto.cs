namespace TestBlazor_FNCourse.Data.DTOs
{
    public class UserOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } 
   }
}
