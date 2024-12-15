using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Data;
using Skincare_Online_Shop_.NET.Models;

namespace Skincare_Online_Shop_.NET.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        public async Task<ActionResult> Details(string id)// string generat folosind guid
        {
            ApplicationUser user = db.Users.Find(id);

            if(user == null)
            {
                TempData["message"] = "We couldn't locate the user in our database";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;
            ViewBag.UserCurent = await _userManager.GetUserAsync(User);

            return View(user);
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            if (user == null)
            {
                TempData["message"] = "We couldn't locate the user in our database";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            ViewBag.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); // lista de nume de roluri

            // cautam id-ul rolului in baza de date, numele rolyului creat (admin, partner sau user)
            ViewBag.UserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .First(); // selectam 1 singur rol

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser user = db.Users.Find(id);

            if (user == null)
            {
                TempData["message"] = "We couldn't locate the user in our database";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            user.AllRoles = GetAllRoles();

            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.DateOfBirth = newData.DateOfBirth;
                user.PhoneNumber = newData.PhoneNumber;

                // cautam toate rolurile din baza de date
                var roles = db.Roles.ToList();
                foreach (var role in roles)
                {
                    // scoatem userul din rolurile anterioare
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                // adaugam noul rol selectat
                var roleName = await _roleManager.FindByIdAsync(newRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                db.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = db.Users
                         .Include("Products")
                         .Include("Reviews")
                         .Include("Carts")
                         .Where(u => u.Id == id)
                         .First();// nu se sterg in cascada datorita lipsei de implementare din ApplicationDbContext
            if (user.Reviews.Count > 0)
            {
                foreach (var comment in user.Reviews)
                {
                    db.Reviews.Remove(comment);
                }
            }
            if (user.Carts.Count > 0)
            {
                foreach (var bookmark in user.Carts)
                {
                    db.Carts.Remove(bookmark);
                }
            }
            if (user.Products.Count > 0)
            {
                foreach (var article in user.Products)
                {
                    db.Products.Remove(article);
                }
            }

            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
