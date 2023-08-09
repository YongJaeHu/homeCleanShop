using homeCleanShop.Areas.Identity.Data;
using homeCleanShop.Data;
using homeCleanShop.Models;
using homeCleanShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;


namespace cleanshop.Controllers
{
    public class ReviewController : Controller
    {
        private readonly homeCleanShopContext _context;
        private readonly UserManager<homeCleanShopUser> _userManager;
        // create constructor for linking your db connection to this file
        public ReviewController(UserManager<homeCleanShopUser> userManager, homeCleanShopContext context)
        {
            _userManager = userManager;
            _context = context; //for refering which table you want to use

        }

        public async Task<IActionResult> Index()
        {
            List<Review> reviewlist = await _context.ReviewTable.ToListAsync();
            return View(reviewlist);
        }

        public IActionResult AddData()
        {
            string user_id = _userManager.GetUserId(User);

            ViewBag.UserID = user_id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Avoid cross-site attack
        public async Task<IActionResult> AddData(Review review)
        {
            string userId = _userManager.GetUserId(User);
            homeCleanShopUser customer = await _userManager.FindByIdAsync(userId);

            review.CustomerName = customer.Name;
            review.CustomerEmail = customer.Email;
            review.cleanShopUserId = userId;

            if (ModelState.IsValid)
            {
                // If form has no issues, then add to the table
                _context.ReviewTable.Add(review);
                await _context.SaveChangesAsync(); // Commit to add the data
                return RedirectToAction("Index", new { msg = "Insert Successfully!" });
            }
            return View(review); // Error, so keep the current review info for editing
        }

        public async Task<IActionResult> ReviewList(string msg)
        {
            string userId = _userManager.GetUserId(User);
            List<Review> review = await _context.ReviewTable
                .Where(b => b.cleanShopUserId == userId)
                .ToListAsync();

            ViewBag.msg = msg;
            return View(review);
        }

        public async Task<IActionResult> EditReview(int? ReviewID)
        {

            if (ReviewID == null)
            {
                return NotFound();
            }

            var review = await _context.ReviewTable.FindAsync(ReviewID);
            if (review == null)
            {
                return NotFound();
            }
            return View(review);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReviewUpdate(Review review)
        {
            var existingReview = await _context.ReviewTable.FindAsync(review.Id);

            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            _context.ReviewTable.Update(existingReview);
            await _context.SaveChangesAsync();
            return RedirectToAction("ReviewList", new { msg = "Update Successfully!" });
        }
        public async Task<IActionResult> DeleteReview(int? ReviewID)
        {
            if (ReviewID == null)
            {
                return NotFound();
            }

            var review = await _context.ReviewTable.FindAsync(ReviewID);

            if (review == null)
            {
                return NotFound();
            }

            try
            {
                _context.ReviewTable.Remove(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("ReviewList", new { msg = "review " + ReviewID + " of  deletion was successful" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ReviewList", new { msg = "review " + ReviewID + " of  deletion was not successful" + ex.Message });
            }
        }

        public async Task<IActionResult> ReviewListAdmin(string msg)
        {
            List<Review> reviewlist = await _context.ReviewTable.ToListAsync();
            return View(reviewlist);
        }
    }
}

