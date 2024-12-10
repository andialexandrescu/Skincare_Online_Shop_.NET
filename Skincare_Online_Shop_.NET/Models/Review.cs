using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The rating must be specified")]
        [Range(1, 5, ErrorMessage = "The rating must be between 1 and 5")]
        public int Rating { get; set; }

        public string? Content { get; set; }
        public DateTime Date { get; set; }

        // fk si proprietate virtuala deoarece un review apartine unui produs
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // fk si proprietate virtuala deoarece un review e postat de un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
