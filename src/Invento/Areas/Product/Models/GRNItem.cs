using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Product.Models
{
    public class GRNItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GRNItemID { get; set; }
        public int CompanyID { get; set; }

        [Required]
        [Display(Name = "GRN Bill")]
        [ForeignKey("GRN")]
        public int GRNID { get; set; }
        public virtual GRN GRN { get; set; }

        [Required]
        [Display(Name = "Item ID")]
        [ForeignKey("Item")]
        public int ItemID { get; set; }
        public virtual Item Item { get; set; }

        public decimal Quantity { get; set; }       
    }
}
