using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TestBlazor_FNCourse.Data.Models
{
    public class Product
    {
        internal double overriddenRating;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PId { get; set; }

        [Required]
        public string PName { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage ="Price must be grater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Stock must be 0 or Positive value")]
        public int Stock {  get; set; }

        [NotMapped]
        public decimal OverallRating
        {
            get
            {
                if (Reviews == null || Reviews.Count == 0)
                    return 0;

                return (decimal)Reviews.Average(r => r.Rating);
            }
        }

        public virtual ICollection<Review> Reviews { get;set; }

        public virtual ICollection<OrderProduct> OrderProducts { get; set; }

 




    }
}
