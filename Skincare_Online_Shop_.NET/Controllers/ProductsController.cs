﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Data;
using Skincare_Online_Shop_.NET.Models;

namespace Skincare_Online_Shop_.NET.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;// nu trebuie definit de tip ApplicationUser deoarece exista deja implementat in IdentityRole, insemnand ca nu se modifica nici item-ul scaffolded Register.cshtml 
        public ProductsController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // se afiseaza lista tuturor produselor impreuna cu categoria din care fac parte
        // pentru fiecare produs se afiseaza si userul care a postat produsul respectiv
        // [HttpGet] se executa implicit
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Index()
        {
            var products = db.Products.Include("Category")
                                      .Include("User");

            ViewBag.AllProducts = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        // se afiseaza un singur produs in functie de id-ul impreuna cu categoria din care face parte
        // sunt preluate si toate review-urile asociate unui produs
        // se afiseaza si userul care a postat produsul respectiv
        // [HttpGet] se executa implicit
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                              .Where(p => p.Id == id)
                              .First();
            if (product == null)
            {
                TempData["message"] = "We couldn't locate the product in our database. Please check if it was deleted by you, an admin, or if the category itself has been deleted";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            SetAccessRights();

            return View(product);
        }

        // adaugarea unui review asociat unui produs in baza de date
        // toate rolurile pot adauga review-uri in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Show([FromForm] Review review)
        {
            // preluam Id-ul utilizatorului care posteaza comentariul
            review.UserId = _userManager.GetUserId(User);
            review.Date = DateTime.Now;

            if(ModelState.IsValid)
            {
                db.Reviews.Add(review);
                db.SaveChanges();
                return Redirect("/Products/Show/" + review.ProductId);
            }
            else
            {
                Product product = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                               .Where(p => p.Id == review.ProductId)
                               .First();

                SetAccessRights();

                return View(product);
            }
        }

        // se afiseaza formularul in care se vor completa datele unui produs impreuna cu selectarea categoriei din care face parte
        // doar utilizatorii cu rolul de Partner si Admin pot adauga articole in platforma
        // [HttpGet] se executa implicit
        [Authorize(Roles = "Partner,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            product.Categ = GetAllCategories();

            return View(product);
        }

        // se adauga articolul in baza de date, iar doar utilizatorii cu rolul Partner si Admin pot adauga articole in platforma
        [HttpPost]
        [Authorize(Roles = "Partner,Admin")]
        public async Task<IActionResult> New(Product product, IFormFile image)
        {
            // preluam Id-ul utilizatorului care posteaza produsul
            product.UserId = _userManager.GetUserId(User);
            product.DateListed = DateTime.Now;

            if (image != null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".JPG", ".PNG", ".JPEG" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImage", "The file must be an image and only .jpg, .jpeg and .png extensions are allowed!");
                    product.Categ = GetAllCategories();
                    return View(product);
                }

                var imagesDirectory = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(imagesDirectory))
                {
                    Directory.CreateDirectory(imagesDirectory);
                }
                var storagePath = Path.Combine(imagesDirectory, image.FileName);
                var databaseFileName = "/images/" + image.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(product.Image));
                product.Image = databaseFileName;
            }

            ModelState.ClearValidationState(nameof(product));
            if (!TryValidateModel(product))
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
            
            db.Products.Add(product);// e adaugat si produsul care are inca un request fals, deoarece urmeaza sa fie aprobat sau nu
            await db.SaveChangesAsync();
            if (User.IsInRole("Admin"))
            {
                TempData["message"] = "The product was added successfully!";
            }
            if (User.IsInRole("Partner"))
            {
                TempData["message"] = "The product submission request has been sent to the admin";
            }
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        // se editeaza un produs existent in baza de date impreuna cu categoria din care face parte, iar categoria se selecteaza dintr-un dropdown
        // se afiseaza formularul impreuna cu datele aferente produsului din baza de date
        // doar utilizatorii cu rolul de Editor si Admin pot edita articole, iar adminii pot edita orice articol din baza de date si editorii pot edita doar articolele proprii (cele pe care ei le-au postat)
        // [HttpGet] se executa implicit
        [Authorize(Roles = "Partner,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(p => p.Id == id)
                                         .First();
            product.Categ = GetAllCategories();

            if(product == null)
            {
                TempData["message"] = "We couldn't locate the product in our database. Please check if it was deleted by you, an admin, or if the category itself has been deleted";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "You are not allowed to edit a product which is not yours!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // se adauga articolul modificat in baza de date si se verifica rolul utilizatorilor care au dreptul sa editeze (Editor si Admin)
        [HttpPost]
        [Authorize(Roles = "Partner,Admin")]
        public async Task<IActionResult> Edit(int id, Product requestProduct, IFormFile image)
        {
            Product product = await db.Products.FindAsync(id);

            if (ModelState.IsValid)
            {
                if((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    product.Name = requestProduct.Name;
                    product.Description = requestProduct.Description;
                    product.Ingredients = requestProduct.Ingredients;
                    product.Price = requestProduct.Price;
                    product.Brand = requestProduct.Brand;
                    product.DateListed = DateTime.Now;
                    product.Quantity = requestProduct.Quantity;
                    product.CategoryId = requestProduct.CategoryId;

                    if (image != null && image.Length > 0)
                    {
                        var allowedExtensions = new[] {".jpg", ".jpeg", ".png", ".JPG", ".JPEG", ".PNG"};
                        var fileExtension = Path.GetExtension(image.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ProductImage", "The file must be an image and only .jpg, .jpeg and .png extensions are allowed!");
                            return View(requestProduct);
                        }

                        var storagePath = Path.Combine(_env.WebRootPath, "images", image.FileName);
                        var databaseFileName = "/images/" + image.FileName;

                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }
                        ModelState.Remove(nameof(product.Image));
                        product.Image = databaseFileName;
                    }

                    TempData["message"] = "The product has been modified";
                    TempData["messageType"] = "alert-success";
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You are not allowed to edit a product which is not yours!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }


        // se sterge un articol din baza de date 
        // utilizatorii cu rolul de Partner sau Admin pot sterge articole (colaboratorii pot sterge doar articolele publicate de ei, iar adminii pot sterge orice articol de baza de date
        [HttpPost]
        [Authorize(Roles = "Partner,Admin")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Include("Reviews")
                                         .Where(p => p.Id == id)
                                         .First();

            if(product == null)
            {
                TempData["message"] = "We couldn't locate the product in our database. Please check if it was deleted by you, an admin, or if the category itself has been deleted";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "The product has been deleted";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You are not allowed to delete a product which is not yours!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // conditiile de afisare pentru butoanele de editare si stergere (aflate in view-uri)
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            if(User.IsInRole("Partner"))
            {
                ViewBag.AfisareButoane = true;
            }
            ViewBag.UserCurent = _userManager.GetUserId(User);
            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }
        /*public ActionResult Requests()
        {
            var products = db.Products.Include("Category").Include("User");
            ViewBag.Products = products;
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ValidateRequest(int id)
        {
            Product product = db.Products.Find(id);
            product.Request = true;
            if(ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["message"] = "The product has been approved succesfully!";
            }
            return Redirect("Index");
        }*/

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();// generare lista de tipul SelectListItem fara elemente
            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;
            // iteram prin categorii
            foreach(var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            return selectList;
        }
    }
}
