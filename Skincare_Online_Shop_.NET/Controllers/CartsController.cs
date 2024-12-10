using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Data;
using Skincare_Online_Shop_.NET.Models;
using static Skincare_Online_Shop_.NET.Models.CartProducts;

namespace Skincare_Online_Shop_.NET.Controllers
{
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CartsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // fiecare utilizator (partner sau user) vede cosurile de cumparaturi pe care le-a plasat, iar userii cu rolul de Admin pot sa vizualizeze toate cosurile existente
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult ShoppingHistory()
        {
            SetAccessRights();

            if (User.IsInRole("Admin"))
            {
                var allCarts = db.Carts.Include("User").ToList();
                ViewBag.AllCarts = allCarts;
                return View();
            }
            else if (User.IsInRole("Partner") || User.IsInRole("User"))
            {
                var userId = _userManager.GetUserId(User);
                var userCarts = db.Carts.Include("User")
                                .Where(cart => cart.UserId == userId)
                                .ToList();
                ViewBag.AllCarts = userCarts;
                return View();
            }
            else
            {
                TempData["message"] = "You are not allowed to see the shopping history.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult MyCart()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            var userId = _userManager.GetUserId(User);
            Cart latestCart = db.Carts.Include(c => c.CartProducts)
                                       .ThenInclude(cp => cp.Product) // Include related products
                                       .Where(cart => cart.UserId == userId)
                                       .OrderByDescending(cart => cart.Date)
                                       .FirstOrDefault();
            if (latestCart == null)
            {
                TempData["message"] = "No shopping cart created, please add products to your first ever order";
                TempData["messageType"] = "alert-info";
                // intorc un cos de cumparaturi gol
                return View(new Cart { CartProducts = new List<CartProduct>() });// ca si cum as implementa un alt view empty datorita validarii pe model in MyCart
            }
            if (!latestCart.PlacedOrder)
            {
                return View(latestCart);// transmit in view-ul MyCart latestCart care nu a fost plasat ca si comanda
            } else
            {
                TempData["message"] = "You have already placed an order with this cart";
                TempData["messageType"] = "alert-info";
                return Redirect("/Carts/ShoppingHistory");
            }
        }

        // afisarea tuturor produselor pe care utilizatorul le-a salvat in cosul de cumparaturi cu un anumit id
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Partner"))
            {
                var carts = db.Carts
                                  .Include("CartProducts.Product.Category")
                                  .Include("CartProducts.Product.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .FirstOrDefault();

                if (carts == null)
                {
                    TempData["message"] = "The cart's content couldn't be retrived";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Products");
                }

                return View(carts);
            } else if(User.IsInRole("Admin"))
            {
                var carts = db.Carts
                                  .Include("CartProducts.Product.Category")
                                  .Include("CartProducts.Product.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();

                if (carts == null)
                {
                    TempData["message"] = "The carts' contents couldn't be retrived";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Products");
                }

                return View(carts);
            } else
            {
                TempData["message"] = "You don't have access to the cart of any user";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult AddToCart(int productId)
        {

            return View("AddToCart", new CartProduct { ProductId = productId, Quantity = 1 });
        }

        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var userCart = db.Carts
                     .Where(c => c.UserId == userId && !c.PlacedOrder)
                     .OrderByDescending(c => c.Date)
                     .FirstOrDefault();

            if (userCart == null)// daca nu exista shopping cart, se creeaza unul
            {
                userCart = new Cart
                {
                    UserId = userId,
                    Date = DateTime.Now,
                    CartProducts = new List<CartProduct>()
                };
                db.Carts.Add(userCart);
                db.SaveChanges();
            }

            // verific daca produsul e in stoc ca sa il pot adauga la comanda
            var product = db.Products.FirstOrDefault(p => p.Id == productId);// produsul retrived prin specficarea cantitatii via AddToCart, ma bazez pe id-ul primit in metoda
            if (product == null || product.Quantity < quantity)
            {
                TempData["message"] = $"Insufficient stock for {product.Name}";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("MyCart");
            }

            var existingCartItem = db.CartProducts
                                     .FirstOrDefault(cp => cp.ProductId == productId && cp.CartId == userCart.Id);
            if (existingCartItem != null)// daca exista deja produsul adaugam la cantitatea existenta
            {
                existingCartItem.Quantity += quantity;
                db.CartProducts.Update(existingCartItem);
            }
            else
            {
                // se creeza o noua intrare in cartproducts pentru a stabili cantitatea ceruta daca asocierea (userCart si product nu exista)
                var newCartItem = new CartProducts.CartProduct
                {
                    ProductId = productId,
                    CartId = userCart.Id,
                    Quantity = quantity,
                    AddedToCartDate = DateTime.Now
                };
                db.CartProducts.Add(newCartItem);
            }
            db.SaveChanges();

            TempData["message"] = "Product added to your cart successfully!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("MyCart");
        }

        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult RemoveFromCart(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var userCart = db.Carts
                     .Where(c => c.UserId == userId && !c.PlacedOrder)
                     .OrderByDescending(c => c.Date)
                     .FirstOrDefault();

            var cartItemToRemove = db.CartProducts
                                            .FirstOrDefault(cp => cp.ProductId == productId && cp.Cart.UserId == userId);

            if (cartItemToRemove != null)
            {
                db.CartProducts.Remove(cartItemToRemove);
                db.SaveChanges();
                TempData["message"] = "Product removed from your cart";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Product not found in your cart";
                TempData["messageType"] = "alert-warning";
            }

            return RedirectToAction("MyCart");
        }

        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult UpdateProduct(int productId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var userCart = db.Carts
                     .Where(c => c.UserId == userId && !c.PlacedOrder)
                     .OrderByDescending(c => c.Date)
                     .FirstOrDefault();

            var cartItem = db.CartProducts
                             .FirstOrDefault(cp => cp.ProductId == productId && cp.Cart.UserId == userId);
 
            if (cartItem != null)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == productId);
                if (product != null && quantity > product.Quantity)
                {
                    TempData["message"] = $"Insufficient stock for {product.Name}, cannot update quantity";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("MyCart");
                }
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    db.CartProducts.Update(cartItem);
                    TempData["message"] = "Product quantity updated successfully!";
                    TempData["messageType"] = "alert-success";
                }
                db.SaveChanges();
            }
            else
            {
                TempData["message"] = "Product not found in your cart";
                TempData["messageType"] = "alert-warning";
            }
            return RedirectToAction("MyCart");
        }

        [HttpPost]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);
            var userCart = db.Carts
                     .Where(c => c.UserId == userId && !c.PlacedOrder)
                     .OrderByDescending(c => c.Date)
                     .FirstOrDefault();

            var cartProducts = db.CartProducts.Where(cp => cp.CartId == userCart.Id).ToList();

            if (!cartProducts.Any())
            {
                TempData["message"] = "Your cart is empty. Please add products before placing an order";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("MyCart");
            }

            float totalAmount = 0;
            foreach (var item in cartProducts)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null || product.Quantity < item.Quantity)// verific cantitatea de stoc in cazul in care mai multi useri cumpara acelasi produs care ar fi in demand
                {
                    TempData["message"] = $"Insufficient stock for{item.Product.Name}, order cannot be placed";// format string $
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("MyCart");
                }
                totalAmount += item.Quantity * product.Price;// totalul de plata, prefer sa nu fac suma de produse folosind linq deoarece ar insemna sa steze campul inainte de a valida stocul pentru fiecare produs, si probabil ar ramen o valoare reziduala in cazul in care da fail din cazua stocului insuficient
            }
            // se marcheaza totalul de plata si ca va fi plasata comanda
            userCart.PlacedOrder = true;
            userCart.TotalAmount = totalAmount;

            // la acest pas stiu sigur ca toate produsele se afla in stoc, deci scad din stocul din Product
            foreach(var item in cartProducts)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                    db.Products.Update(product);
                }
            }
            db.SaveChanges();

            TempData["message"] = "Your order has been placed successfully!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("ShoppingHistory");
        }

        [HttpGet]
        [Authorize(Roles = "User,Partner,Admin")]
        public IActionResult StartNewOrder()
        {
            var userId = _userManager.GetUserId(User);

            var newCart = new Cart
            {
                UserId = userId,
                Date = DateTime.Now,
                PlacedOrder = false
            };

            db.Carts.Add(newCart);
            db.SaveChanges();

            return RedirectToAction("MyCart");
        }

        // afisare butoane de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Partner") || User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}
