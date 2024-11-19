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
        public ProductsController(ApplicationDbContext context)
        {
            db = context;
        }

        // HttpGet implicit
        public IActionResult Index()// se afiseaza lista tuturor produselor impreuna cu categoria lor
        {
            var products = db.Products.Include("Category");

            ViewBag.AllProducts = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        // HttpGet implicit
        public IActionResult Show(int id)// se afiseaza un singur produs in functie de id-ul sau impreuna cu categoria din care face parte si toate review-urile lui
        {

            Product product = db.Products.Include("Category").Include("Reviews").Where(a => a.Id == id).First();// .First() este necesara pentru a obtine un singur articol din colectia returnata de metoda Where.
            // sau from art in ... where art.id == id select art;

            //ViewBag.Category(ViewBag.UnNume) = article.Category (proprietatea Category);

            return View(product);
        }

        // HttpGet implicit
        public IActionResult New()// se afiseaza formularul in care se vor completa datele unui articol impreuna cu selectarea categoriei din care face parte
        {
            Product product = new Product();
            product.Categ = GetAllCategories();
            return View(product);
        }
        [HttpPost]
        public IActionResult New(Product product)// se adauga articolul in baza de date
        {
            product.DateListed = DateTime.Now;
            try
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "The product has been added to the catalogue successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }
    }
}
