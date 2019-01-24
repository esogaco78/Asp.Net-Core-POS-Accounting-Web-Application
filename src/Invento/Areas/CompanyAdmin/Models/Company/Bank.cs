using Invento.Areas.Payment.Models;
using Invento.Areas.Purchase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.CompanyAdmin.Models.Company
{
    public class Bank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankID { get; set; }

        [Required]              
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Display(Name = "Bank Description")]
        public string BankDescription { get; set; }
        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public int TransactionAccountID { get; set; }

        public virtual ICollection<Transaction> Transaction { get; set; }
        
        public virtual ICollection<CashInBank> CashInBank { get; set; }
        public virtual ICollection<ChequeReceipt> ChequeReceipt { get; set; }
        public virtual ICollection<ChequePayment> ChequePayment { get; set; }
    }
}
