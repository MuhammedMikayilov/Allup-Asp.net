using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        [Required]
        public string Image { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }


    }
}
