using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skincare_Online_Shop_.NET.Models
{
    // extind prin mostenire clasa de baza (adica vor exista si alte proprietati pe langa cele definite prin Entity Framework)
    public class ApplicationUser: IdentityUser // are ca campuri care sunt utilizabile si folositoare noua UserName, Email si PhoneNumber, care vor aparea in controller-ul UserController
    {
        // un user poate posta mai multe comentarii
        public virtual ICollection<Review>? Reviews { get; set; }

        // un user poate adauga mai multe produse in magazinul online
        public virtual ICollection<Product>? Products { get; set; }

        // un user poate sa aibe mai multe cosuri de cumparaturi
        public virtual ICollection<Cart>? Carts { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        // variabila in care vom retine rolurile existente in baza de date pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
