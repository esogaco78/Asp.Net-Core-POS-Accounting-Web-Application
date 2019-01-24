using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Sale.Models
{
    public class SaleReturnTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleTransactionID { get; set; }
        public int CompanyID { get; set; }
        public PaymentMode? Mode { get; set; }
        public string Cheque { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool Paid { get; set; }

        [ForeignKey("Bank")]
        public int? BankID { get; set; }
        public virtual Bank Bank { get; set; }

        public bool SaleTransactionExtraBool { get; set; }
        public string SaleTransactionExtraString { get; set; }
        public int SaleTransactionExtraInt { get; set; }
        public decimal SaleTransactionExtraDecimal { get; set; }
  

        public int SaleBillID { get; set; }

        [ForeignKey("SaleReturn")]
        public int SaleReturnID { get; set; }
        public virtual SaleReturn SaleReturn { get; set; }

    }
}
