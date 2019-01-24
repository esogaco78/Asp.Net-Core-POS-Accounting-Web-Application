using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Finance.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Payment.Models
{
    public class CashReceipt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashReceiptID { get; set; }

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
        [Display(Name = "Received With Thanks From")]
        [ForeignKey("Parties")]
        public int PartiesID { get; set; }
        public virtual Parties Parties { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Display(Name = "Paid By")]
        public string PaidBy { get; set; }

        [Display(Name = "Amount In Words")]
        public string AmountInWords { get; set; }
        public string Particulars { get; set; }

        public int CompanyID { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        
        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
