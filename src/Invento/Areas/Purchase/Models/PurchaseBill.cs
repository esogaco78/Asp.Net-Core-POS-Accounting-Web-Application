using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Finance.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Purchase.Models
{
    public class PurchaseBill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseBillID { get; set; }

        [Required]
        [Display(Name = "Bill Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BillDate { get; set; }

        [Required]
        [Display(Name = "Purchase Bill No")]
        public string PurchaseBillNo { get; set; }

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
       
        [Display(Name = "Exchange Rate")]
        public int ExchangeRate { get; set; }


        [Display(Name = "External Ref.")]
        public string ExternalRef { get; set; }

        [Display(Name = "Ref. Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RefDate { get; set; }

        [Display(Name = "Pay Terms")]
        public PayTerms? PayTerms { get; set; }

        [Display(Name = "Credit Days")]
        public string CreditDays { get; set; }
        
        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public string Remarks { get; set; }

        // For Total

        [Required]
        [Display(Name = "Total Quantity")]
        public decimal TotalQuantity { get; set; }

        [Required]
        [Display(Name = "Gross Total")]
        public decimal GrossTotal { get; set; }

        public PurchaseType PurchaseImport { get; set; }
        public decimal TLandingExpenses { get; set; }

        [Display(Name = "Total Discount")]
        public string TDiscount { get; set; }

        [Required]
        [Display(Name = "Net Amount")]
        public decimal NetAmount { get; set; }

        [Display(Name = "Advance")]
        public decimal Advance { get; set; }

        [Required]
        [Display(Name = "Cash Paid")]
        public decimal CashPaid { get; set; }

        [Required]
        [Display(Name = "Balance")]
        public decimal Balance { get; set; }

        //Extra Fields
        public int PB_1 { get; set; }
        public int PB_2 { get; set; }
        public decimal PB_D_1 { get; set; }
        public decimal PB_D_2 { get; set; }
        public string PB_S_1 { get; set; }
        public string PB_S_2 { get; set; }
        public bool PB_B_1 { get; set; }
        //Extra Fields
         
        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int CurrencyID { get; set; }
        public virtual Currency Currency { get; set; }
                 
        [Display(Name = "Company Name")]
        [ForeignKey("Parties")]
        public int? PartiesID { get; set; }
        public virtual Parties Parties { get; set; }
         
        public virtual ICollection<Transaction> Transaction { get; set; }
        public virtual ICollection<PurchaseBillItem> PurchaseBillItem { get; set; }
        public virtual ICollection<PurchaseReturn> PurchaseReturn { get; set; }
        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
