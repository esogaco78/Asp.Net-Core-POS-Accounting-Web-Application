using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Purchase.Models
{
    public class PurchaseReturnVM
    {
        public int PurchaseReturnID { get; set; }

        public int PurchaseBillID { get; set; }
        public int ItemID { get; set; }
    
        public int? PartiesID { get; set; }
     
        [Display(Name = "Old Quantity")]
        public decimal OldQuantity { get; set; }

        [Display(Name = "Return Quantity")]
        public decimal ReturnQuantity { get; set; }

        [Display(Name = "Amount To Receive")]
        public decimal AmountToReceive { get; set; }

        public PurchaseType PurchaseImport { get; set; }
        [Display(Name = "T-Landing Expenses")]
        public decimal TLandingExpenses { get; set; }

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

        public decimal CashPaid { get; set; }
        public decimal Balance { get; set; }
        public string BillDate_OldBill { get; set; }
        public decimal CashPaid_OldBill { get; set; }
        public string ContactNumber_OldBill { get; set; }
        public string ContactPerson_OldBill { get; set; }
        public decimal GrossTotal_OldBill { get; set; }
        public decimal NetAmount_OldBill { get; set; }
        public string Remarks_OldBill { get; set; }
        public string TDiscount_OldBill { get; set; }
        public decimal TotalQuantity_OldBill { get; set; }
        public List<Transaction> TransactionList { get; set; }
        public List<PurchaseBillItem> PurchaseBillItem_List { get; set; }
    }
}
