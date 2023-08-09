using System.ComponentModel.DataAnnotations;
using homeCleanShop.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace homeCleanShop.Models;

public class Booking
{
    [Key]//primary key 

    public string BookingID { get; set; }

    public int ServiceID { get; set; }

    public string cleanShopUserId { get; set; }

    public string CustomerName { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerPhone { get; set; }

    [Required]
    public DateTime booking_date { get; set; }

    [Required]
    public string payment_method { get; set; }

    public string ServiceName { get; set; }

    public int ServiceTime { get; set; }

    public string BookingStatus { get; set; }


}
