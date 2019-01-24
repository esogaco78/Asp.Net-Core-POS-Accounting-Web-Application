using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Payment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Finance.Models
{
    public class TransactionAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionAccountID { get; set; }
        
        [Required]
        [Display(Name = "Sub A/c. Number")]
        [ForeignKey("SubAccount")]
        public int SubAccountID { get; set; }
        public virtual SubAccount SubAccount { get; set; }

        [Required]
        [Display(Name = "Transaction A/c. Number")]
        public string TransactionAccountNumber { get; set; }
        
        [Required]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [Required]
        [Display(Name = "Opening Balance")]
        public decimal OpeningBalance { get; set; }
        
        public int CompanyID { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual Parties Parties { get; set; }

        public virtual ICollection<CashFlow> CashFlow { get; set; }

        public virtual ICollection<VoucherItems> VoucherItems { get; set; }
    }
}
