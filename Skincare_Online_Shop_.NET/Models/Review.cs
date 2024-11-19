using System.ComponentModel.DataAnnotations;

namespace Skincare_Online_Shop_.NET.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Must enter the review grade")]
        public int Grade { get; set; }

        public string Comment { get; set; }
        public DateTime Date { get; set; }
        //[ForeignKey]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}