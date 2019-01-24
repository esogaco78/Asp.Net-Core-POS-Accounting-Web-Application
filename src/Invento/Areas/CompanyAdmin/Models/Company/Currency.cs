using Invento.Areas.Payment.Models;
using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.CompanyAdmin.Models.Company
{
    public class Currency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CurrencyID { get; set; }

        [Required]
        [Display(Name = "Currency Name")]
        public string CurrencyName { get; set; }

        [Display(Name = "ISO")]
        public string ISO { get; set; }
        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public virtual ICollection<PurchaseBill> PurchaseBill { get; set; }
        public virtual ICollection<SaleBill> SaleBill { get; set; }
        public virtual ICollection<CashPayment> CashPayment { get; set; }
        public virtual ICollection<CashReceipt> CashReceipt { get; set; }
        public virtual ICollection<CashInBank> CashInBank{ get; set; }
        public virtual ICollection<ChequeReceipt> ChequeReceipt { get; set; }
        public virtual ICollection<ChequePayment> ChequePayment { get; set; }

    }
}
