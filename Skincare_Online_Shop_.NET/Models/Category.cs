using System.ComponentModel.DataAnnotations;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The category name must be specified")]
        [StringLength(100, ErrorMessage = "The category name cannot exceed 100 characters")]
        public string CategoryName { get; set; }

        // proprietatea virtuala - dintr-o categorie fac parte mai multe produse
        public virtual ICollection<Product>? Products { get; set; }
    }
}