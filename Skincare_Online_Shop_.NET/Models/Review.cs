using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Skincare_Online_Shop_.NET.Validations;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Review
    {
        [Key]
        //[MaxLength(255)]
        public int Id { get; set; }
        
        [ForbiddenWords("fuck", "hell", "ew", "eww", "shit", "wtf", "hell")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "The rating must be specified")]
        [Range(1, 5, ErrorMessage = "Value should be a number between 1 and 5")]
        public int Grade { get; set; }
        public DateTime Date { get; set; }
        // [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // fk si proprietate virtuala deoarece un review e postat de un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
