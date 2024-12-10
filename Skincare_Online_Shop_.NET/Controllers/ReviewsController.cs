using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Skincare_Online_Shop_.NET.Data;
using Skincare_Online_Shop_.NET.Models;

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

        // stergerea unui review asociat unui produs din baza de date
        // se poate sterge review-ul doar de catre userii cu rolul de Admin  sau de catre utilizatorii cu rolul de User sau Editor, doar daca  acel review a fost postat de catre acestia
        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);

            if(rev == null)
            {
                TempData["message"] = "We couldn't locate the review in our database. Please check if it was deleted by you, an admin, or if the category or product itself has been deleted";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                db.SaveChanges();
                return Redirect("/Products/Show/" + rev.ProductId);
            }
            else
            {
                TempData["message"] = "You are not allowed to delete the review!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        // se poate edita un review doar de catre utilizatorul care a postat review-ul respectiv, iar adminii pot edita orice review, chiar daca nu a fost postat de ei
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);

            if(rev == null)
            {
                TempData["message"] = "We couldn't locate the review in our database. Please check if it was deleted by you, an admin, or if the category or product itself has been deleted";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(rev);
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
        public IActionResult Edit(int id, Review requestReview)
        {
            Review rev = db.Reviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    rev.Rating = requestReview.Rating;
                    rev.Content = requestReview.Content;
                    db.SaveChanges();
                    return Redirect("/Products/Show/" + rev.ProductId);
                }
                else
                {
                    //return Redirect("/Products/Edit/" + rev.ProductId);
                    return View(requestReview);
                }
            }
            else
            {
                TempData["message"] = "You are not allowed to edit the review!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }
    }
}
