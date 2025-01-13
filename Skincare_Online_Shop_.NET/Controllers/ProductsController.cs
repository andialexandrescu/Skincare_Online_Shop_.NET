using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        /*private static double CalculateAverageRating(Product product)
        {
            if (product.Reviews != null && product.Reviews.Any())
            {
                double totalGrade = 0;
                int reviewCount = 0;

                foreach(var review in product.Reviews)
                {
                    totalGrade += review.Grade;
                    reviewCount++;
                }

                return totalGrade/reviewCount;
            }
            return 0;
        }*/
        [AllowAnonymous]// nu necesita autorizare
        //[Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Index()
        {
            var products = db.Products.Include("Category")
                                      .Include("User")
                                      .Include("Reviews")
                                      .Include("Reviews.User")
                                      .Where(p => p.RequestStatus == Product.Status.Approved);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            //sectiunea motorului de cautare
            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();// eliminam spatiile libere 

                List<int> productIds = db.Products.Where
                                        (
                                         p => p.Name.Contains(search)
                                         || p.Description.Contains(search)
                                        ).Select(product => product.Id).ToList();

                /*implementare care nu se afla in cerinta
                 * List<int> productIdsOfReviewsWithSearchString = db.Reviews
                                        .Where
                                        (
                                         r => r.Content.Contains(search)
                                        ).Select(review => (int)review.ProductId).ToList();

                List<int> mergedIds = productIds.Union(productIdsOfReviewsWithSearchString).ToList();// se formeaza o singura lista formata din toate id-urile selectate anterior

                // lista produselor care contin cuvantul cautat, fie in produs -> Name si Description, fie in review-uri -> Content
                products = db.Products.Where(p => mergedIds.Contains(p.Id))
                                      .Include("Category")
                                      .Include("User");*/

                products = db.Products.Where(p => productIds.Contains(p.Id))
                                      .Include("Category")
                                      .Include("User")
                                      .Include("Reviews")
                                      .Include("Reviews.User")
                                      .Where(p => p.RequestStatus == Product.Status.Approved);
            }
            ViewBag.SearchString = search;

            int initialCount = products.Count();
            Console.WriteLine($"Initial product count: {initialCount}");

            var sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]);// un utilizator selecteaza o optiune de sortare din meniul dropdown din vizualizare, care va fi trimisa inapoi la server folosind get
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select..." },
                new SelectListItem { Value = "price_asc", Text = "Price: Low to High" },
                new SelectListItem { Value = "price_desc", Text = "Price: High to Low" },
                new SelectListItem { Value = "rating_asc", Text = "Rating: Low to High" },
                new SelectListItem { Value = "rating_desc", Text = "Rating: High to Low" }
            };
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "rating_asc":
                    //products = products.OrderBy(p => CalculateAverageRating(p));
                    // varianta in care folosesc metoda CalculateAverageRating va returna la debug sortedCount egal cu 0
                    // noua varianta pastreaza linq aplicat pe obiectele p, intr-un inline lambda expression
                    products = products.OrderBy(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Grade) : 0);// fiecarui p din colectia products ii este verificat join-ul cu Reviews daca intoarce rezultate sau nu  (operator ternar, cond ? instr_cond_adev : instr_cond_fals), daca intoarce se calculeaza un average, altfel e implicit considerat 0 rating-ul produsului si este pus pe o poz coresp in sortarea cu orderby
                    break;
                case "rating_desc":
                    //products = products.OrderByDescending(p => CalculateAverageRating(p));
                    products = products.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Grade) : 0);
                    break;
                default:
                    break;// nu se aplica nicio sortare
            }

            int sortedCount = products.Count();
            Console.WriteLine($"Count after sorting by rating: {sortedCount}");
            foreach (var product in products)
            {
                Console.WriteLine($"Product ID: {product.Id}, Review Count: {product.Reviews.Count()}");
            }

            // sectiunea de afisare paginata
            int _perPage = 4;// 4 produse per pagina
            int totalItems = products.Count();// nr de produse curent din baza de date
            int currentPage = 1;
            var pageQuery = HttpContext.Request.Query["page"];
            if (!string.IsNullOrEmpty(pageQuery) && int.TryParse(pageQuery, out var parsedPage))
            {
                currentPage = Math.Max(1, parsedPage);
            }

            var offset = (currentPage - 1) * _perPage;// offsetul va fi egal cu numarul de produse care au fost deja afisate pe paginile anterioare
            var paginatedProducts = products.Skip(offset).Take(_perPage);// produsele paginate se iau in blocuri de cate 4 cu un anumit offset

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);// ultima pagina
            ViewBag.currentPage = currentPage;
            ViewBag.AllProducts = paginatedProducts;

            // motor de cautare si afisare paginata
            ViewBag.PaginationBaseUrl = $"/Products/Index/?search={search}&sortOrder={sortOrder}&page=";

            return View();
        }

        [AllowAnonymous]
        //[Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Details(int id)// afisarea detaliilor unui singur produs in functie de id
        {
            var product = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                              .Where(p => p.Id == id)
                              .First();

            if (product == null)
            {
                return NotFound();
            }

            if (product.RequestStatus == Product.Status.Unverified && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You do not have permission to view this unverified product";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            SetAccessRights();

            return View(product);
        }

        [HttpGet]
        [Authorize(Roles = "Partner,Admin")]
        public IActionResult Create()
        {
            var categories = db.Categories.ToList();
            if (categories == null || !categories.Any())
            {
                Console.WriteLine("No categories found in the database");
            }
            else
            {
                Console.WriteLine($"Loaded {categories.Count} categories");
            }

            Product product = new Product();
            product.Categ = GetAllCategories();
            return View(product);
        }

        // se adauga articolul in baza de date, iar doar utilizatorii cu rolul Partner si Admin pot adauga articole in platforma
        [HttpPost]
        [Authorize(Roles = "Partner,Admin")]
        public async Task<IActionResult> Create(Product product, IFormFile image)
        {
            // preluam Id-ul utilizatorului care posteaza produsul
            product.UserId = _userManager.GetUserId(User);
            product.DateListed = DateTime.Now;
            if (User.IsInRole("Admin"))
            {
                product.RequestStatus = Product.Status.Approved;
            } else
            {
                product.RequestStatus = Product.Status.Unverified;// e unverified pentru cineva care nu e admin
            }

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
            
            db.Products.Add(product);
            await db.SaveChangesAsync();
            if (User.IsInRole("Admin"))
            {
                TempData["message"] = "The product was added successfully!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "The product has been added successfully and is pending submission approval, until then it won't be visible on our online platform";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Partner,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await db.Products.Include(p => p.Category)
                                           .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
            else
            {
                TempData["message"] = "You are not allowed to edit a product which is not yours!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Partner,Admin")]
        public async Task<IActionResult> Edit(int id, Product requestProduct, IFormFile image)
        {
            try
            {
                Product product = await db.Products.FindAsync(id);

                if (product == null)
                {
                    TempData["message"] = "The product was not found.";
                    return RedirectToAction("Index");
                }

                if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    product.Name = requestProduct.Name;
                    product.Description = requestProduct.Description;
                    product.Ingredients = requestProduct.Ingredients;
                    product.Price = requestProduct.Price;
                    product.Brand = requestProduct.Brand;
                    product.DateListed = DateTime.Now;
                    product.Stock = requestProduct.Stock;
                    product.CategoryId = requestProduct.CategoryId;
                    if (User.IsInRole("Admin"))
                    {
                        product.RequestStatus = Product.Status.Approved;
                    }
                    else
                    {
                        product.RequestStatus = Product.Status.Unverified;
                    }

                    if (image != null && image.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".JPG", ".PNG", ".JPEG" };
                        var fileExtension = Path.GetExtension(image.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Image", "The file must be an image and only .jpg, .jpeg and .png extensions are allowed!");
                            requestProduct.Categ = GetAllCategories();
                            return View(requestProduct);
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
                    /* ModelState.ClearValidationState(nameof(product));
                    if (!TryValidateModel(product))
                    {
                        product.Categ = GetAllCategories();
                        return View(product);
                    }*/

                    await db.SaveChangesAsync();
                    if (User.IsInRole("Admin"))
                    {
                        TempData["message"] = "The product was added successfully!";
                        TempData["messageType"] = "alert-success";
                    } else
                    {
                        TempData["message"] = "The product has been updated successfully and is pending re-approval, until then it won't be visible on our online platform";
                        TempData["messageType"] = "alert-success";
                    }
                    return RedirectToAction("Index");
                }

                TempData["message"] = "You do not have permission to edit a product that does not belong to you";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Partner,Admin")]
        public IActionResult Delete(int id)
        {
            Product product = db.Products.Include("Reviews")
                                         .Where(p => p.Id == id)
                                         .First();

            if(product != null)
            {
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
            } else
            {
                return StatusCode(StatusCodes.Status404NotFound);
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingApproval()
        {
            var pendingProducts = await db.Products
                                           .Include(p => p.User)
                                           .Where(p => p.RequestStatus == Product.Status.Unverified)
                                           .ToListAsync();
            
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View(pendingProducts);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var product = await db.Products.Include(p => p.User)
                                           .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            product.RequestStatus = Product.Status.Approved;
            if(ModelState.IsValid)
            {
                await db.SaveChangesAsync();
                TempData["message"] = $"The product '{product.Name}' posted by '{product.User.UserName}' has been approved successfully!";
                TempData["messageType"] = "alert-success";
            }

            return RedirectToAction("PendingApproval");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var product = await db.Products.Include(p => p.User)
                                           .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            product.RequestStatus = Product.Status.Rejected;
            if (ModelState.IsValid)
            {
                await db.SaveChangesAsync();
                TempData["message"] = $"The product '{product.Name}' posted by '{product.User.UserName}' has been rejected successfully!";
                TempData["messageType"] = "alert-success";
            }

            return RedirectToAction("PendingApproval");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();// generare lista de tipul SelectListItem fara elemente

            var categories = from cat in db.Categories
                             select cat;

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
