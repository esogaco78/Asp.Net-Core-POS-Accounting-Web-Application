using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Finance.Models;
using Invento.Areas.Purchase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Payment.Models
{
    public class ChequePayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChequePaymentID { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int CurrencyID { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "Ex-Rate")]
        public string ExternalRef { get; set; }

        [Required]
        [Display(Name = "Debit A/c.")]
        [ForeignKey("Parties")]
        public int PartiesID { get; set; }
        public virtual Parties Parties { get; set; }

        [Display(Name = "In Name Of")]
        public string InNameOf { get; set; }

        [Required]
        public decimal Amount { get; set; }        

        [Display(Name = "Amount In Words")]
        public string AmountInWords { get; set; }

        [Display(Name = "Narration")]
        public string Particulars { get; set; }


        // For Cheque to be cash later
        [Display(Name = "Current Status")]
        public ChequeStatus ChequeStatus { get; set; }



        [Display(Name = "Cheque Number")]
        public string ChequeNumber { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Of Mature ")]
        public DateTime DateOfMature { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Of Deposite")]
        public DateTime DateOfDeposite { get; set; }

        [Display(Name = "Current Status")]
        public string CurrentStatus { get; set; }

        [Display(Name = "Import Bill ID")]
        public string ImportExportID { get; set; }
        public int CompanyID { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }

        [Required]
        [ForeignKey("Bank")]
        [Display(Name = "Paid From Account")]
        public int BankID { get; set; }
        public virtual Bank Bank { get; set; }
        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
