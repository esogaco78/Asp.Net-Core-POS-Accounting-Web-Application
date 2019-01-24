using Invento.Areas.Product.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Purchase.Models
{
    public class PurchaseBillItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseBillItemID { get; set; }
        public int CompanyID { get; set; }

        [Required]
        [Display(Name = "Purchase Bill")]
        [ForeignKey("PurchaseBill")]
        public int PurchaseBillID { get; set; }
        public virtual PurchaseBill PurchaseBill { get; set; }

        [Required]
        [Display(Name = "Item ID")]
        [ForeignKey("Item")]
        public int ItemID { get; set; }
        public virtual Item Item { get; set; }

        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }



        public bool PurchaseBillExtraBool { get; set; }
        public string PurchaseBillExtraString { get; set; }
        public string PurchaseBillExtraString_2 { get; set; }
        public int PurchaseBillExtraInt { get; set; }
        public int PurchaseBillExtraInt_1 { get; set; }
        public int PurchaseBillExtraInt_2 { get; set; }
        public decimal PurchaseBillExtraDecimal { get; set; }
        public decimal PurchaseBillExtraDecimal_1 { get; set; }


    }  
}
