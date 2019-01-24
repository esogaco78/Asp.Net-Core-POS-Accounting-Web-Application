using Invento.Areas.Payment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Finance.Models
{
    public class MainAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MainAccountID { get; set; }

        [Required]
        [Display(Name = "Main A/c. Number")]
        public string MainAccountNumber { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }        

        public int CompanyID { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual ICollection<SubAccount> SubAccount { get; set; }
        public virtual ICollection<CashFlow> CashFlow { get; set; }

        //public virtual ICollection<VoucherItems> VoucherItems { get; set; }
    }
}
