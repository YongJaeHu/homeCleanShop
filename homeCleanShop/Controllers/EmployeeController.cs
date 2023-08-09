using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using homeCleanShop.Models;
using homeCleanShop.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using homeCleanShop.Models;
using homeCleanShop.Data;

namespace homeCleanShop.Controllers;

public class EmployeeController : Controller
{
    private const string s3BucketName = "homecleanshop";


    private readonly homeCleanShopContext _context;
    // create constructor for linking your db connection to this file
    public EmployeeController(homeCleanShopContext context)
    {
        _context = context; //for refering which table you want to use
    }

    private List<string> getkeys()
    {

        //create an empty key list to store back the key values from appsetting.json file
        List<string> keys = new List<string>();

        var builder = new ConfigurationBuilder().
        SetBasePath(Directory.GetCurrentDirectory()).
        AddJsonFile("appsettings.json");
        IConfiguration configure = builder.Build(); // build the file 

        // add your keys to the list store
        keys.Add(configure["keys:key1"]);
        keys.Add(configure["keys:key2"]);
        keys.Add(configure["keys:key3"]);

        return keys;
    }



    public async Task<IActionResult> Index(string msg)
    {
        List<Employee> employeelist = await _context.EmployeeTable.ToListAsync();
        ViewBag.msg = msg;
        return View(employeelist);
    }

    public IActionResult AddForm()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddForm(Employee employee, IFormFile imagefiles)
    {
        if (imagefiles == null)
        {
            return BadRequest("Error! Not Found the image");
        }
        //step1: check the input validation 
        if (imagefiles.Length <= 0)
        {
            return BadRequest("Error! File of " + imagefiles.FileName + "is an empty image!");
        }
        else if (imagefiles.Length > 1048576)
        {
            return BadRequest("Error! File of " + imagefiles.FileName + "is > 1MB!");
        }
        else if (imagefiles.ContentType.ToLower() != "image/png" && imagefiles.ContentType.ToLower() != "image/jpeg" && imagefiles.ContentType.ToLower() != "image/jpg")
        {
            return BadRequest("Error! File of " + imagefiles.FileName + "is not a valid image we can accept!!!");
        }
        else
        { // step 2 all validation pass, submit to S3 
            try
            {
                List<string> keys = getkeys(); // call the getkeys function to get key info from appjson file
                AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                // create request to send file to S3
                PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                {
                    BucketName = s3BucketName,
                    Key = "employees/" + imagefiles.FileName,
                    InputStream = imagefiles.OpenReadStream(),
                    CannedACL = S3CannedACL.PublicRead
                };

                await agent.PutObjectAsync(uploadRequest);



            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);

            }
            catch (Exception ex)
            {
                return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);

            }

        }

        if (ModelState.IsValid)
        {  //if form no issue , then add to table

            string fileName = Path.GetFileName(imagefiles.FileName);

            if (imagefiles != null && imagefiles.Length > 0)
            {
                employee.Employee_Photo = "employees/" + fileName;
            }


            _context.EmployeeTable.Add(employee);
            await _context.SaveChangesAsync(); //commit to add the data
            return RedirectToAction("Index", new { msg = "Added Successfully!" });
        }
        return View(employee); //error then keep the current flower info for editting
    }

    public async Task<IActionResult> EditEmployeeShow(int? EmployeeID)
    {

        if (EmployeeID == null)
        {
            return NotFound();
        }

        var employee = await _context.EmployeeTable.FindAsync(EmployeeID);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmployeeUpdate(Employee employee, IFormFile imagefiles)
    {

        var existingEmployee = await _context.EmployeeTable.FindAsync(employee.EmployeeID);

        if (imagefiles != null)
        {
            string fileName = Path.GetFileName(imagefiles.FileName);

            //step1: check the input validation 
            if (imagefiles.Length <= 0)
            {
                return BadRequest("Error! File of " + imagefiles.FileName + "is an empty image!");
            }
            else if (imagefiles.Length > 1048576)
            {
                return BadRequest("Error! File of " + imagefiles.FileName + "is > 1MB!");
            }
            else if (imagefiles.ContentType.ToLower() != "image/png" && imagefiles.ContentType.ToLower() != "image/jpeg")
            {
                return BadRequest("Error! File of " + imagefiles.FileName + "is not a valid image we can accept!!!");
            }
            else
            { // step 2 all validation pass, submit to S3 
                try
                {
                    List<string> keys = getkeys(); // call the getkeys function to get key info from appjson file
                    AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                    // create request to send file to S3
                    PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                    {
                        BucketName = s3BucketName,
                        Key = "employees/" + imagefiles.FileName,
                        InputStream = imagefiles.OpenReadStream(),
                        CannedACL = S3CannedACL.PublicRead
                    };

                    await agent.PutObjectAsync(uploadRequest);

                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);

                }
                catch (Exception ex)
                {
                    return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);

                }
                try
                {
                    List<string> keys = getkeys(); // call the getkeys function to get key info from appjson file
                    AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = s3BucketName,
                        Key = existingEmployee.Employee_Photo,
                    };

                    Console.WriteLine("Deleting an object");
                    await agent.DeleteObjectAsync(deleteObjectRequest);
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
                }
            }

        }



        if (imagefiles != null && imagefiles.Length > 0)
        {
            string fileName = Path.GetFileName(imagefiles.FileName);

            employee.Employee_Photo = "employees/" + fileName;
        }
        else
        {
            employee.Employee_Photo = existingEmployee.Employee_Photo;
        }

        if (ModelState.IsValid)
        {
            existingEmployee.Employee_Name = employee.Employee_Name;
            existingEmployee.Employee_Experience_Year = employee.Employee_Experience_Year;
            existingEmployee.Employee_Description = employee.Employee_Description;
            existingEmployee.Employee_Photo = employee.Employee_Photo;


            _context.Update(existingEmployee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { msg = "Update Successfully!" });
        }
        return View("EditEmployeeShow", employee);

    }

    public async Task<IActionResult> EmployeeDelete(int? EmployeeID)
    {
        if (EmployeeID == null)
        {
            return NotFound();
        }

        var employee = await _context.EmployeeTable.FindAsync(EmployeeID);
        var employee_name = employee.Employee_Name + " ";
        if (employee == null)
        {
            return NotFound();
        }
        try
        {
            try
            {
                List<string> keys = getkeys();
                AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = employee.Employee_Photo,
                };

                await agent.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
            }

            _context.EmployeeTable.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction("index", new { msg = "Employee " + EmployeeID + ": " + employee_name + "of  deletion was successful" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("index", new { msg = "Employee " + EmployeeID + ": " + employee_name + " of  deletion was not successfully" + ex.Message });
        }


    }









}
