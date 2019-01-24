using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Sale.Models
{
    public class SaleReturnVM
    {
        public int SaleReturnID { get; set; }

        public int SaleBillID { get; set; }
        public int ItemID { get; set; }
    
        public int? PartiesID { get; set; }
     
        [Display(Name = "Old Quantity")]
        public decimal OldQuantity { get; set; }

        [Display(Name = "Return Quantity")]
        public decimal ReturnQuantity { get; set; }

        [Display(Name = "Amount To Pay")]
        public decimal AmountToPay { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }


        [Required]
        [Display(Name = "Sale Return Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SaleBillReturnDate { get; set; }

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public string Remarks { get; set; }

        public decimal CashPaid { get; set; }
        public decimal Balance { get; set; }

        //Extra Fields     
        public int SR_I_1 { get; set; }
        public int SR_I_2 { get; set; }
        public decimal SR_D_1 { get; set; }
        public decimal SR_D_2 { get; set; }
        public string SR_S_1 { get; set; }
        public string SR_S_2 { get; set; }
        public bool SR_B_1 { get; set; }
        //Extra Fields     

       
        public string BillDate_OldBill { get; set; }
        public decimal CashPaid_OldBill { get; set; }
        public string ContactNumber_OldBill { get; set; }
        public string ContactPerson_OldBill { get; set; }
        public decimal GrossTotal_OldBill { get; set; }
        public decimal NetAmount_OldBill { get; set; }
        public string Remarks_OldBill { get; set; }
        public string TDiscount_OldBill { get; set; }
        public decimal TotalQuantity_OldBill { get; set; }
        public List<SaleTransaction> SaleTransactionList { get; set; }
        public List<SaleBillItem> SaleBillItem_List { get; set; }
    }
}
