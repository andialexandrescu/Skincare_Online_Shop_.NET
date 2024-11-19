using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The product name is required")]
        public string Name { get; set; }
        public float Price { get; set; }

        [Required(ErrorMessage = "The description is required")]
        public string Description { get; set; }
        public string Ingredients { get; set; }// not required
        public DateTime DateListed { get; set; }

        [Required(ErrorMessage = "Adding the product category is required")]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Categ { get; set; }
    }
}
