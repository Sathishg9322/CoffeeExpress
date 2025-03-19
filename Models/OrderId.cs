namespace CoffeeShop.Models
{
    public class OrderId
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Datetime { get; set; }
        public string Item {  get; set; }
        public int Quantity {  get; set; }
        public decimal Price { get; set; }
    }
}
