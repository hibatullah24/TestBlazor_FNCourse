namespace TestBlazor_FNCourse.Data.DTOs
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}