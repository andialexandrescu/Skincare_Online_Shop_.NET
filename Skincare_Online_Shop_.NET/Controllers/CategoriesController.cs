using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Data;
using Skincare_Online_Shop_.NET.Models;

namespace Skincare_Online_Shop_.NET.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "Partner,Admin")]
        public ActionResult Index()
        {
            var categories = db.Categories.ToList();
            ViewBag.AllCategories = categories;
            return View();
        }

        [Authorize(Roles = "Partner,Admin")]
        public ActionResult Details(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction("Index", "Categories");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the category: " + ex.Message);
                return View(category); // Revino la formular dacă apare o eroare
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Update(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            Category category = db.Categories.Include("Products")
                                             .Include("Products.Reviews")
                                             .Where(c => c.Id == id)
                                             .First();// stergere in cascada
            if (category != null)
            {
                db.Categories.Remove(category);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
