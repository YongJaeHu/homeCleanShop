using System.ComponentModel.DataAnnotations;

namespace homeCleanShop.Models;

public class Employee
{
    [Key]//primary key 

    public int EmployeeID { get; set; }

    [Required]
    public string Employee_Name { get; set; }
    [Required]
    public int Employee_Experience_Year { get; set; }
    [Required]
    public string Employee_Description { get; set; }

    public string Employee_Photo { get; set; }
}
