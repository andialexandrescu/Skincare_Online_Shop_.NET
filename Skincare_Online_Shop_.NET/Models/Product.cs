using Microsoft.AspNetCore.Mvc.Rendering;
using Skincare_Online_Shop_.NET.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Skincare_Online_Shop_.NET.Models.CartProducts;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The product name is required")]
        [StringLength(100, ErrorMessage = "The product name cannot exceed 100 characters")]
        [MinLength(3, ErrorMessage = "The product name must exceed 3 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The product image is required")]
        public string? Image {  get; set; }// de tip string pt ca se va salva calea imaginii

        [Required(ErrorMessage = "The product content is required before proceeding with the changes")]
        [StringLength(1000, ErrorMessage = "The product content cannot exceed 1000 characters")]
        public string Description { get; set; }

        [IngredientsSeparatedByCommaValidation]
        public string Ingredients { get; set; }

        [Required(ErrorMessage = "The product price is required before proceeding with the changes")]
        public float Price { get; set; }
        public string Brand { get; set; }
        public DateTime DateListed { get; set; }

        [Range(1, 5, ErrorMessage = "The rating must be between 1 and 5")]
        public float? Rating { get; set; }// va fi computat pe baza unui average de rating-uri de la review-uri
        //public bool Request { get; set; }// niciun mesaj de validare deoarece se atribuie o valoare in functie de rolul utilizatorului la nivel de backend (in metoda New)

        [Required(ErrorMessage = "The quantity of the product you intend to ship to our online shop is required before you can proceed with the changes")]
        [Range(1, 1000, ErrorMessage = "The product quantity must be between 1 and 1000, our online shop has a limited stock")]
        public int Quantity { get; set; }

        // unui produs ii este asociata o categorie
        [Required(ErrorMessage = "Adding the product category is required")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        [NotMapped]// meniu dropdown cu categorii care e populat pe cererea de get de fiecare data (chiar si daca produsul nu a putut fi adugat de prima oara in baza de date)
        public IEnumerable<SelectListItem>? Categ { get; set; }

        // un produs poate avea o colectie de review-uri
        public virtual ICollection<Review>? Reviews { get; set; }

        // relatia many-to-many dintre Product si Cart
        public virtual ICollection<CartProduct>? CartProducts { get; set; }

        // fk impreuna cu proprietatea virtuala de tip ApplicationUser - un produs este adaugat de catre un user (colaboratorul/ partenerul)
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
