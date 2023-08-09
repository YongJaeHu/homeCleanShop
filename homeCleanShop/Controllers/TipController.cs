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

public class TipController : Controller
{
    private const string s3BucketName = "homecleanshop";


    private readonly homeCleanShopContext _context;
    // create constructor for linking your db connection to this file
    public TipController(homeCleanShopContext context)
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
        List<Tip> tiplist = await _context.TipTable.ToListAsync();
        ViewBag.msg = msg;
        return View(tiplist);
    }

    public async Task<IActionResult> TipList(string msg)
    {
        List<Tip> tiplist = await _context.TipTable.ToListAsync();
        ViewBag.msg = msg;
        return View(tiplist);
    }

    public IActionResult AddForm()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddForm(Tip tip, IFormFile imagefiles)
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
                    Key = "tips/" + imagefiles.FileName,
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
                tip.Tip_Photo = "tips/" + fileName;
            }


            _context.TipTable.Add(tip);
            await _context.SaveChangesAsync(); //commit to add the data
            return RedirectToAction("Index", new { msg = "Added Successfully!" });
        }
        return View(tip); //error then keep the current flower info for editting
    }

    public async Task<IActionResult> EditTipShow(int? TipID)
    {

        if (TipID == null)
        {
            return NotFound();
        }

        var tip = await _context.TipTable.FindAsync(TipID);
        if (tip == null)
        {
            return NotFound();
        }
        return View(tip);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTipUpdate(Tip tip, IFormFile imagefile)
    {

        var existingTip = await _context.TipTable.FindAsync(tip.TipID);

        if (imagefile != null)
        {
            string fileName = Path.GetFileName(imagefile.FileName);

            //step1: check the input validation 
            if (imagefile.Length <= 0)
            {
                return BadRequest("Error! File of " + imagefile.FileName + "is an empty image!");
            }
            else if (imagefile.Length > 1048576)
            {
                return BadRequest("Error! File of " + imagefile.FileName + "is > 1MB!");
            }
            else if (imagefile.ContentType.ToLower() != "image/png" && imagefile.ContentType.ToLower() != "image/jpeg")
            {
                return BadRequest("Error! File of " + imagefile.FileName + "is not a valid image we can accept!!!");
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
                        Key = "tips/" + imagefile.FileName,
                        InputStream = imagefile.OpenReadStream(),
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
                        Key = existingTip.Tip_Photo,
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



        if (imagefile != null && imagefile.Length > 0)
        {
            string fileName = Path.GetFileName(imagefile.FileName);

            tip.Tip_Photo = "tips/" + fileName;
        }
        else
        {
            tip.Tip_Photo = existingTip.Tip_Photo;
        }

        if (ModelState.IsValid)
        {
            existingTip.Tip_Title = tip.Tip_Title;
            existingTip.Tip_Description = tip.Tip_Description;
            existingTip.Tip_Photo = tip.Tip_Photo;


            _context.Update(existingTip);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { msg = "Update Successfully!" });
        }
        return View("EditTipShow", tip);


    }

    public async Task<IActionResult> TipDelete(int? TipID)
    {
        if (TipID == null)
        {
            return NotFound();
        }

        var tip = await _context.TipTable.FindAsync(TipID);
        var tip_title = tip.Tip_Title;
        var tip_photo = tip.Tip_Photo;
        if (tip == null)
        {
            return NotFound();
        }
        try
        {
            _context.TipTable.Remove(tip);
            await _context.SaveChangesAsync();

            try
            {
                List<string> keys = getkeys(); // call the getkeys function to get key info from appjson file
                AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = tip_photo,
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


            return RedirectToAction("index", new { msg = "Tip " + TipID + ": " + tip_title + "of  deletion was successful" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("index", new { msg = "Tip " + TipID + ": " + tip_title + " of  deletion was not successfully" + ex.Message });
        }



    }
    public async Task<IActionResult> DetailTipShow(int? TipID)
    {

        if (TipID == null)
        {
            return NotFound();
        }

        var tip = await _context.TipTable.FindAsync(TipID);
        if (tip == null)
        {
            return NotFound();
        }
        return View(tip);
    }
    public async Task<IActionResult> DetailTipUser(int? TipID)
    {

        if (TipID == null)
        {
            return NotFound();
        }

        var tip = await _context.TipTable.FindAsync(TipID);
        if (tip == null)
        {
            return NotFound();
        }
        return View(tip);
    }

}
