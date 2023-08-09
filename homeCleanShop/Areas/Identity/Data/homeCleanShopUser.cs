using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace homeCleanShop.Areas.Identity.Data;

// Add profile data for application users by adding properties to the homeCleanShopUser class
public class homeCleanShopUser : IdentityUser
{
    [PersonalData]
    public string Name { get; set; }

    [PersonalData]
    public string Address { get; set; }

    [PersonalData]
    public string UserRole { get; set; }

}

