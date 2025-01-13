using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Data;
using Skincare_Online_Shop_.NET.Models;
using System.Security.Claims;

namespace Skincare_Online_Shop_.NET.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(Review review)
        {
            review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("AddReview method called.");
            Console.WriteLine($"ProductId: {review.ProductId}");
            Console.WriteLine($"Content: {review.Content}");
            Console.WriteLine($"Grade: {review.Grade}");
            Console.WriteLine($"UserId: {review.UserId}");

            review.Date = DateTime.Now;
            db.Reviews.Add(review);
            db.SaveChanges();
            Console.WriteLine("Review saved to database.");

            return RedirectToAction("Details", "Products", new { id = review.ProductId });
        }

        // Editarea unei recenzii (GET)
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Edit(int id)
        {
            var review = db.Reviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(review);
            }
            else
            {
                TempData["message"] = "You are not allowed to edit the review!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Edit(Review review)
        {
            var existingReview = db.Reviews.FirstOrDefault(r => r.Id == review.Id);

            if (existingReview == null)
            {
                return NotFound();
            }

            if (existingReview.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    existingReview.Grade = review.Grade;
                    existingReview.Content = review.Content;
                    db.Reviews.Update(existingReview);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Products", new { id = existingReview.ProductId });
                }
                return View(existingReview);
            }
            else
            {
                TempData["message"] = "You are not allowed to edit the review!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);

            if (rev == null)
            {
                TempData["message"] = "We couldn't locate the review in our database. Please check if it was deleted by you, an admin, or if the category or product itself has been deleted";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                db.SaveChanges();
                return Redirect("/Products/Details/" + rev.ProductId);
            }
            else
            {
                TempData["message"] = "You are not allowed to delete the review!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }
    }
}
