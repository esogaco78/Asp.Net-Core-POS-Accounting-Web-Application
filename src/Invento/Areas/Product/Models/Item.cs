using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Product.Models
{
    // Product Table
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Item ID")]
        public int ItemID { get; set; }        
       
        [Display(Name = "OEM No.")]
        public string OEMNo { get; set; }

        [Display(Name = "Vehicle Name")]
        public string ItemName { get; set; }

        [Display(Name = "Item Description")]
        public string ProductDescription { get; set; }

        [Display(Name = "Item Type")]
        public string ItemType { get; set; }

        [Display(Name = "Cross Ref.")]
        public string CrossRef { get; set; }
        
        [Display(Name = "Item Company")]
        public string ItemMainCompany { get; set; }

        [Display(Name = "Size")]
        public string Size { get; set; }

        [Display(Name = "Picture")]
        public byte[] PhotoData { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Purchase Price")]
        public decimal LCPrice { get; set; }

        [Display(Name = "Sale Price")]
        public decimal? SalePrice { get; set; }

        [Display(Name = "Chasis No")]
        public string ItemType2{ get; set; }

        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }




        //Extra Fields
       
        public int? ItemExtra_Int_1 { get; set; }
        public int? ItemExtra_Int_2 { get; set; }
        public int? ItemExtra_Int_3 { get; set; }
        public int? ItemExtra_Int_4 { get; set; }
        public int? ItemExtra_Int_5 { get; set; }
        public decimal? ItemExtra_Dec_1 { get; set; }
        public decimal? ItemExtra_Dec_2 { get; set; }
        public decimal? ItemExtra_Dec_3 { get; set; }
        public decimal? ItemExtra_Dec_4 { get; set; }
        public decimal? ItemExtra_Dec_5 { get; set; }

        [Display(Name = "Engine No.")]
        public string ItemExtra_String_1 { get; set; }

        [Display(Name = "Box No.")]
        public string ItemExtra_String_2 { get; set; }

        [Display(Name = "Plastic Bag")]
        public string ItemExtra_String_3 { get; set; }
        public string ItemExtra_String_4 { get; set; }
        public string ItemExtra_String_5 { get; set; }
        [NotMapped]
        public bool CheckCase_1 { get; set; }
        //public bool CheckCase_2 { get; set; }       
        public int? NotMappedInt { get; set; }
        public bool? NotMappedBool { get; set; }
        public string NotMappedString_1 { get; set; }
        public string NotMappedString_2 { get; set; }

        //Extra Fields

        //[Required]
        [Display(Name = "Product Name")]
        [ForeignKey("ProductGroup")]
        public int? ProductGroupID { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }

        public virtual ICollection<PurchaseBillItem> PurchaseBillItem { get; set; }
        public virtual ICollection<SaleBillItem> SaleBillItem { get; set; }
        public virtual ICollection<PurchaseReturn> PurchaseReturn { get; set; }
        public virtual ICollection<SaleReturn> SaleReturn { get; set; }

    }
}
