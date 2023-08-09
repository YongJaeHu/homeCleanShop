using homeCleanShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace homeCleanShop.Data;

public class homeCleanShopContext : IdentityDbContext<homeCleanShopUser>
{
    public homeCleanShopContext(DbContextOptions<homeCleanShopContext> options)
        : base(options)
    {

    }

    public DbSet< homeCleanShop.Models.Service> ServiceTable { get; set; }

    public DbSet<homeCleanShop.Models.Booking> BookingTable { get; set; }
    public DbSet<homeCleanShop.Models.Employee> EmployeeTable { get; set; }

    public DbSet<homeCleanShop.Models.Tip> TipTable { get; set; }

    public DbSet<homeCleanShop.Models.Review> ReviewTable { get; set; }








    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
