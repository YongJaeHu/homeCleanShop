using System.ComponentModel.DataAnnotations;

namespace homeCleanShop.Models
{
    public class Review
    {
        [Key]//primary key 
        public int Id { get; set; }
        public string cleanShopUserId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
