using Invento.Areas.Finance.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Payment.Models
{
    public class VoucherItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherItemsID { get; set; }

        [ForeignKey("MainAccount")]
        public int MainAccountID { get; set; }
        public virtual MainAccount MainAccount { get; set; }

        [ForeignKey("SubAccount")]
        public int SubAccountID { get; set; }
        public virtual SubAccount SubAccount { get; set; }

        [ForeignKey("TransactionAccount")]
        public int TransactionAccountID { get; set; }
        public virtual TransactionAccount TransactionAccount { get; set; }

        public string Narration { get; set; }
        public int CompanyID { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }        

        [ForeignKey("Voucher")]
        public int VoucherID { get; set; }
        public virtual Voucher Voucher { get; set; }
        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
