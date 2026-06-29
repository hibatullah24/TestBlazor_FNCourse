namespace TestBlazor_FNCourse.Data.DTOs
{
    public class PlaceOrderRequest
    {
        public int UserId { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }
}