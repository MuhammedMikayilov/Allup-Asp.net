using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        public double ExTax { get; set; }
        [MaxLength(5)]
        public double Rate { get; set; }
        public bool IsDelete { get; set; }
        public string ProductCode { get; set; }
        public double Discount { get; set; }
        public int SaleCount { get; set; }
        [Required]
        public int CurrentCount { get; set; }
        public string Tags { get; set; }
        public string Brand { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }

    }
}
