namespace CoffeeShop.Models
{
    public class History
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime Datetime { get; set; }
        public string Name { get; set; }
        public string Payment {  get; set; }
        public int Total {  get; set; }
        public string Item { get; set; }
    }
}
