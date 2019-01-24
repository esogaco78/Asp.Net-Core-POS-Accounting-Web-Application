using Invento.Areas.Finance.Models;
using Invento.Areas.Payment.Models;
using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.CompanyAdmin.Models.Company
{
    public class Parties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartiesID { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public string PartyName { get; set; }

        [Display(Name = "Short Name")]
        public string PartyShortName { get; set; }

        [Display(Name = "Business Relation")]
        public string BusinessRelation { get; set; }

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Phone Number")]
        public string Phone1 { get; set; }

        [Display(Name = "Phone No. 2")]
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Road { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string State { get; set; }        
        public string Email { get; set; }
        public string Remarks { get; set; }

        [Display(Name = "Other Details")]
        public string OtherDetails { get; set; }
        public string Observations { get; set; }

        [Display(Name = "Additional Information 1")]
        public string AdditionalInfo1 { get; set; }

        [Display(Name = "Additional Information 2")]
        public string AdditionalInfo2 { get; set; }


        public string ExtraString { get; set; }
        
        public bool ExtraBool { get; set; }

        [Required]
        [Display(Name = "Country Name")]
        [ForeignKey("Country")]
        public int CountryID { get; set; }

        public virtual Country Country { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        [ForeignKey("TransactionAccount")]
        public int? TransactionAccountID { get; set; }
        public virtual TransactionAccount TransactionAccount { get; set; }

        public virtual ICollection<PurchaseBill> PurchaseBill { get; set; }
        public virtual ICollection<SaleBill> SaleBill { get; set; }

        public virtual ICollection<PurchaseReturn> PurchaseReturn { get; set; }
        public virtual ICollection<SaleReturn> SaleReturn { get; set; }        
        public virtual ICollection<CashPayment> CashPayment { get; set; }
        public virtual ICollection<CashReceipt> CashReceipt { get; set; }
        public virtual ICollection<CashInBank> CashInBank { get; set; }
        public virtual ICollection<ChequeReceipt> ChequeReceipt { get; set; }
        public virtual ICollection<ChequePayment> ChequePayment { get; set; }

        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
