using Invento.Areas.CompanyAdmin.Models.Company;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Purchase.Models
{
    public class Transaction
    {
        // Purchase Bill Transactions
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionID { get; set; }

        public int CompanyID { get; set; }
        public PaymentMode? Mode { get; set; }
        public string Cheque { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool Paid { get; set; }

        [ForeignKey("Bank")]
        public int? BankID { get; set; }
        public virtual Bank Bank { get; set; }

        public bool PurchaseTransactionExtraBool { get; set; }
        public string PurchaseTransactionExtraString { get; set; }
        public int PurchaseTransactionExtraInt { get; set; }
        public decimal PurchaseTransactionExtraDecimal { get; set; }


        [ForeignKey("PurchaseBill")]
        public int? PurchaseBillID { get; set; }
        public virtual PurchaseBill PurchaseBill { get; set; }

    }
}
