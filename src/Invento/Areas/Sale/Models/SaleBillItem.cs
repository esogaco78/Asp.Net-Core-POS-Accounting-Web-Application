using Invento.Areas.Product.Models;
using Invento.Areas.Sale.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Sale.Models
{
    public class SaleBillItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleBillItemID { get; set; }

        public int CompanyID { get; set; }

        [Required]
        [Display(Name = "Sale Bill")]
        [ForeignKey("SaleBill")]
        public int SaleBillID { get; set; }
        public virtual SaleBill SaleBill { get; set; }

        [Required]
        [Display(Name = "Item ID")]
        [ForeignKey("Item")]
        public int ItemID { get; set; }
        public virtual Item Item { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal SalePrice { get; set; }


        //for canceled invoice
        public bool SaleBillExtraBool { get; set; }
        public string SaleBillExtraString { get; set; }
        public string SaleExtraString_2 { get; set; }
        public int SaleBillExtraInt { get; set; }
        public int SaleBillExtraInt_1 { get; set; }
        public int SaleBillExtraInt_2 { get; set; }
        public decimal SaleBillExtraDecimal { get; set; }
        public decimal SaleBillExtraDecimal_1 { get; set; }


    }  
}
