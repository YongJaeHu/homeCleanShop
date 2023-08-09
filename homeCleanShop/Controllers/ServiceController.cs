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

namespace homeCleanShop.Controllers
{
    public class ServiceController : Controller
    {
        private const string s3BucketName = "homecleanshop";
        private readonly homeCleanShopContext _context;

        public ServiceController(homeCleanShopContext context)
        {
            _context = context;
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
            List<Service> servicelist = await _context.ServiceTable.ToListAsync();
            ViewBag.msg = msg;
            return View(servicelist);
        }

        public IActionResult AddForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]//avoid-cross site attack
        public async Task<IActionResult> AddForm(Service service, IFormFile imagefiles)
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
                        Key = "services/" + imagefiles.FileName,
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
                    service.ServicePhoto = "services/" + fileName;
                }

                _context.ServiceTable.Add(service);
                await _context.SaveChangesAsync(); //commit to add the data
                return RedirectToAction("Index", new { msg = "Insert Successfully!" });
            }
            return View(service); //error then keep the current flower info for editting
        }

        public async Task<IActionResult> EditServiceShow(int? ServiceID)
        {

            if (ServiceID == null)
            {
                return NotFound();
            }

            var service = await _context.ServiceTable.FindAsync(ServiceID);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditServiceUpdate(Service service, IFormFile imagefiles)
        {
            var existingService = await _context.ServiceTable.FindAsync(service.ID);

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
                            Key = "services/" + imagefiles.FileName,
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
                            Key = existingService.ServicePhoto,
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

                service.ServicePhoto = "services/" + fileName;
            }
            else
            {
                service.ServicePhoto = existingService.ServicePhoto;
            }

            if (ModelState.IsValid)
            {
                existingService.ServiceName = service.ServiceName;
                existingService.ServiceDescription = service.ServiceDescription;
                existingService.ServicePrice = service.ServicePrice;
                existingService.ServiceTime = service.ServiceTime;
                existingService.ServicePhoto = service.ServicePhoto;

                _context.Update(existingService);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { msg = "Update Successfully!" });
            }
            return View("EditServiceShow", service);

        }
        public async Task<IActionResult> ServiceDelete(int? ServiceID)
        {
            if (ServiceID == null)
            {
                return NotFound();
            }

            var service = await _context.ServiceTable.FindAsync(ServiceID);
            var service_name = service.ServiceName;
            if (service == null)
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
                        Key = service.ServicePhoto,
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

                _context.ServiceTable.Remove(service);
                await _context.SaveChangesAsync();

                return RedirectToAction("index", new { msg = "Service " + ServiceID + ": " + service_name + "of  deletion was successful" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("index", new { msg = "Service " + ServiceID + ": " + service_name + " of  deletion was not successfully" + ex.Message });
            }


        }

        public async Task<IActionResult> ServiceList(string msg)
        {
            List<Service> servicelist = await _context.ServiceTable.ToListAsync();
            ViewBag.msg = msg;
            return View(servicelist);
        }

        public async Task<IActionResult> ServiceDetail(int? ServiceID)
        {

            if (ServiceID == null)
            {
                return NotFound();
            }

            var service = await _context.ServiceTable.FindAsync(ServiceID);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }


    }
}
