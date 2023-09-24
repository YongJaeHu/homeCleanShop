using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using homeCleanShop.Models;
using Microsoft.AspNetCore.Identity;
using homeCleanShop.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using homeCleanShop.Models;
using homeCleanShop.Data;

namespace homeCleanShop.Controllers;

public class BookingController : Controller
{
    private readonly UserManager<homeCleanShopUser> _userManager;
    private readonly homeCleanShopContext _context;

    public BookingController(UserManager<homeCleanShopUser> userManager, homeCleanShopContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // Booking List for Customer 
    public async Task<IActionResult> BookingList(string msg, string searchString)
    {
        string userId = _userManager.GetUserId(User);
        List<Booking> bookings = await _context.BookingTable
            .Where(b => b.cleanShopUserId == userId)
            .ToListAsync();

        if (!string.IsNullOrEmpty(searchString))
        {
            bookings = bookings.Where(s => s.BookingID.Contains(searchString)).ToList();
        }

        ViewBag.msg = msg;
        return View(bookings);
    }

    // Booking List for Admin
    public async Task<IActionResult> BookingListAdmin(string msg, string searchString)
    {

        List<Booking> booking = await _context.BookingTable.ToListAsync();

        if (!string.IsNullOrEmpty(searchString))
        {
            booking = booking.Where(s => s.BookingID.Contains(searchString)).ToList();
        }
        ViewBag.msg = msg;
        return View(booking);
    }




    public async Task<IActionResult> BookingForm(int? ServiceID)
    {

        string user_id = _userManager.GetUserId(User);

        ViewBag.UserID = user_id;

        Service service = await _context.ServiceTable
        .Where(s => s.ID == ServiceID)
        .SingleOrDefaultAsync();

        ViewBag.ServiceID = ServiceID;
        ViewBag.ServiceName = service.ServiceName;
        ViewBag.ServicePrice = service.ServicePrice;
        ViewBag.ServiceTime = service.ServiceTime;

        return View();
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookingForm(Booking booking)
    {

        string userId = _userManager.GetUserId(User);
        homeCleanShopUser customer = await _userManager.FindByIdAsync(userId);

        int serviceID = booking.ServiceID;
        Service service = await _context.ServiceTable.FindAsync(serviceID);

        string uniqueId = Guid.NewGuid().ToString();
        booking.BookingID = uniqueId;
        booking.BookingStatus = "Pending";
        booking.CustomerName = customer.Name;
        booking.CustomerEmail = customer.Email;
        booking.CustomerPhone = customer.PhoneNumber;

        if (service != null)
        {
            booking.ServiceName = service.ServiceName;
            booking.ServiceTime = service.ServiceTime;
        }


        if (ModelState.IsValid)
        {
            _context.BookingTable.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction("ServiceList","Service", new { msg = "Booking Completed Successfully!" });
        }
        return View(booking); //error then keep the current flower info for editting


    }

    public async Task<IActionResult> EditBooking(string? BookingID)
    {

        if (BookingID == null)
        {
            return NotFound();
        }

        var booking = await _context.BookingTable.FindAsync(BookingID);
        if (booking == null)
        {
            return NotFound();
        }
        return View(booking);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBookingUpdate(Booking booking)
    {
        var existingBooking = await _context.BookingTable.FindAsync(booking.BookingID);

            existingBooking.booking_date = booking.booking_date;
            _context.BookingTable.Update(existingBooking);
            await _context.SaveChangesAsync();
            return RedirectToAction("BookingList", new { msg = "Update Successfully!" });
    }

    public async Task<IActionResult> CancelBooking(string? BookingID)
    {
        if (BookingID == null)
        {
            return NotFound();
        }

        var booking = await _context.BookingTable.FindAsync(BookingID);

        if (booking == null)
        {
            return NotFound();
        }

        booking.BookingStatus = "Cancelled";

        try
        {
            _context.BookingTable.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingList", new { msg = "booking " + BookingID + " of  cancel was successful" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("BookingList", new { msg = "booking " + BookingID + " of  cancel was not successful" + ex.Message });
        }
    }

    public async Task<IActionResult> AcceptBooking(string? BookingID)
    {
        if (BookingID == null)
        {
            return NotFound();
        }

        var booking = await _context.BookingTable.FindAsync(BookingID);

        if (booking == null)
        {
            return NotFound();
        }

        booking.BookingStatus = "Accepted";

        try
        {
            _context.BookingTable.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingListAdmin", new { msg = "booking " + BookingID + " of  accept was successful" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("BookingListAdmin", new { msg = "booking " + BookingID + " of accept was not successful" + ex.Message });
        }
    }

    public async Task<IActionResult> RejectBooking(string? BookingID)
    {
        if (BookingID == null)
        {
            return NotFound();
        }

        var booking = await _context.BookingTable.FindAsync(BookingID);

        if (booking == null)
        {
            return NotFound();
        }

        booking.BookingStatus = "Rejected";

        try
        {
            _context.BookingTable.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingListAdmin", new { msg = "booking " + BookingID + " of  reject was successful" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("BookingListAdmin", new { msg = "booking " + BookingID + " of  reject was not successful" + ex.Message });
        }
    }

    public async Task<IActionResult> DeleteBooking(string? BookingID)
    {
        if (BookingID == null)
        {
            return NotFound();
        }

        var booking = await _context.BookingTable.FindAsync(BookingID);

        if (booking == null)
        {
            return NotFound();
        }

        try
        {
            _context.BookingTable.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingList", new { msg = "booking " + BookingID + " of  deletion was successful" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("BookingList", new { msg = "booking " + BookingID + " of  deletion was not successful" + ex.Message });
        }
    }

    public async Task<IActionResult> DeleteBookingAdmin(string? BookingID)
    {

        if (BookingID == null)
        {
            return NotFound();
        }

        var booking = await _context.BookingTable.FindAsync(BookingID);
        var user = await _userManager.GetUserAsync(User);
        var userRole = await _userManager.GetRolesAsync(user);
        if (booking == null)
        {
            return NotFound();
        }

        try
        {
            _context.BookingTable.Remove(booking);
            await _context.SaveChangesAsync();

            if (userRole.Contains("Admin"))
            {
                return RedirectToAction("BookingListAdmin", new { msg = "booking " + BookingID + " deletion was successful" });
            }
            else
            {
                return RedirectToAction("BookingList", new { msg = "booking " + BookingID + " deletion was successful" });
            }
        }
        catch (Exception ex)
        {
            if (userRole.Contains("Admin"))
            {
                return RedirectToAction("BookingListAdmin", new { msg = "booking " + BookingID + " of  deletion was not successful" + ex.Message });
            }
            else
            {
                return RedirectToAction("BookingList", new { msg = "booking " + BookingID + " deletion was not successful" });
            }
            }
    }




}
