using System.ComponentModel.DataAnnotations;

namespace homeCleanShop.Models
{
    public class Tip
    {
        [Key]//primary key 

        public int TipID { get; set; }

        [Required]
        public string Tip_Title { get; set; }
        [Required]

        public string Tip_Description { get; set; }

        public string Tip_Photo { get; set; }

    }
}
