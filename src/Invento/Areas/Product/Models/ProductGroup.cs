using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Product.Models
{
    public class ProductGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Product Group ID")]
        public int ProductGroupID { get; set; }

        [Required]
        [Display(Name = "Product Group Name")]
        public string ProductGroupName { get; set; }

        public bool CheckCase { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }
        public virtual ICollection<Item> Item { get; set; }
    }
}
