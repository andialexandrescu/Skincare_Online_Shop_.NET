using System.ComponentModel.DataAnnotations;
using static Skincare_Online_Shop_.NET.Models.CartProducts;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
         
        public DateTime Date { get; set; }

        public bool PlacedOrder { get; set; } = false;// daca a fost plasata comanda pt cosul de cumparaturi coresp si la default e false

        [Required(ErrorMessage = "Total amount is required.")]
        public float TotalAmount { get; set; } = 0.0f;

        // relatia many-to-many dintre Product si Cart
        public virtual ICollection<CartProduct>? CartProducts { get; set; }

        // un cos de cumparaturi este creat de catre un user
        // oricand este o relatie intre User si o clasa se adauga fk si proprietatea virtuala pt asocierea cu un user caruia ii apartine cosul de cumparaturi
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
