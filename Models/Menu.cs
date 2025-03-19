using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models
{
    public class Menu
    {
        public Guid Id { get; set; }

        public string Category { get; set; }

        public string Item { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public int Quantity { get; set; } = 0;

        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }


    }
}
