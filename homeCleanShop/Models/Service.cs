using System.ComponentModel.DataAnnotations;

namespace homeCleanShop.Models
{
    public class Service
    {
        [Key]//primary key 

        public int ID { get; set; }

        [Required]
        public string ServiceName { get; set; }
        [Required]

        public string ServiceDescription { get; set; }
        [Required]

        public int ServicePrice { get; set; }
        [Required]

        public int ServiceTime { get; set; }

        public string ServicePhoto { get; set; }
    }
}
