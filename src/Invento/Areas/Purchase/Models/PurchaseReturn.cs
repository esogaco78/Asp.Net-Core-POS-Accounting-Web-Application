using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Finance.Models;
using Invento.Areas.Product.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Purchase.Models
{
    public class PurchaseReturn
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseReturnID { get; set; }

        [Required]
        [Display(Name = "Purchase Bill ID")]
        [ForeignKey("PurchaseBill")]
        public int PurchaseBillID { get; set; }
        public virtual PurchaseBill PurchaseBill { get; set; }

        [Required]
        [Display(Name = "Item ID")]
        [ForeignKey("Item")]
        public int ItemID { get; set; }
        public virtual Item Item { get; set; }
        
        [Display(Name = "Company Name")]
        [ForeignKey("Parties")]
        public int? PartiesID { get; set; }
        public virtual Parties Parties { get; set; }


        //public virtual PartyPayment PartyPayment { get; set; }

        [Display(Name = "Old Quantity")]
        public decimal OldQuantity { get; set; }

        [Display(Name = "Return Quantity")]
        public decimal ReturnQuantity { get; set; }

        [Display(Name = "Amount To Receive")]
        public decimal AmountToReceive { get; set; }

        
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }


        [Required]
        [Display(Name = "Purchase Return Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PurBillReturnDate { get; set; }
          
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public string Remarks { get; set; }

        //Extra Fields     
        public int PR_I_1 { get; set; }
        public int PR_I_2 { get; set; }
        public decimal PR_D_1 { get; set; }
        public decimal PR_D_2 { get; set; }
        public string PR_S_1 { get; set; }
        public string PR_S_2 { get; set; }
        public bool PR_B_1 { get; set; }
        //Extra Fields     
        public virtual ICollection<PurchaseReturnTransaction> PurchaseReturnTransaction { get; set; }

        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
