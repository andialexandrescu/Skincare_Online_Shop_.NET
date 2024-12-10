using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skincare_Online_Shop_.NET.Models
{
    public class CartProducts
    {
        // tabelul asociativ care face legatura intre Product si Cart
        // un produs face parte din mai multe cosuri de cumparaturi sau niciunul
        // un cos de cumparaturi contine mai multe produse
        public class CartProduct
        {
            // cheie primara compusa (Id, ProductId, CartId)
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public int? ProductId { get; set; }
            public int? CartId { get; set; }

            [Required(ErrorMessage = "The quantity of the product you intend to in the order is required before you can proceed with the changes")]
            [Range(1, 1000, ErrorMessage = "The product quantity must be between 1 and 1000, our online shop has a limited stock")]
            public int Quantity { get; set; }
            public DateTime AddedToCartDate { get; set; }

            public virtual Product? Product { get; set; }
            public virtual Cart? Cart { get; set; }
        }
    }
}
