using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Product.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Sale.Models
{
    public class SaleBillVM
    {
        public int SaleBillID { get; set; }

        [Required]
        [Display(Name = "Bill Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BillDate { get; set; }

        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Sale Bill No")]
        public string SaleBillNo { get; set; }

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
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal TotalQuantity { get; set; }

        [Required]
        [Display(Name = "Gross Total")]
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal GrossTotal { get; set; }


        [Display(Name = "Total Discount")]
        public string TDiscount { get; set; }

        [Required]
        [Display(Name = "Net Amount")]
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal NetAmount { get; set; }

        [Display(Name = "Advance")]
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal Advance { get; set; }

        
        [Display(Name = "Cash Paid")]
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal CashPaid { get; set; }

        [Required]
        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal Balance { get; set; }

        //Extra Fields       
        public int SB_1 { get; set; }
        public int SB_2 { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal SB_D_1 { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0#####}", ApplyFormatInEditMode = true)]
        public decimal SB_D_2 { get; set; }
        public string SB_S_1 { get; set; }
        public string SB_S_2 { get; set; }
        public bool SB_B_1 { get; set; }
        //Extra Fields

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int CurrencyID { get; set; }
        public virtual Currency Currency { get; set; }    
        
        [Display(Name = "Company Name")]
        [ForeignKey("Parties")]
        public int? PartiesID { get; set; }
        public virtual Parties Parties { get; set; }

        public List<Bank> BankList { get; set; }

        public List<Item> ItemList { get; set; }

        public List<SaleBillItem> SaleBillItem_List { get; set; }

        public List<SaleTransaction> SaleTransactionList { get; set; }

        public virtual SaleBillItem SaleBillItem { get; set; }        
    }

}
