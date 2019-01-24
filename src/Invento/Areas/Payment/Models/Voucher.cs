using System;
using Invento.Areas.CompanyAdmin.Models.Company;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invento.Areas.Payment.Models
{
    public class Voucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherID { get; set; }
        
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public string Particulars { get; set; }

        [Display(Name = "Import Bill ID")]
        public string ImportExportID { get; set; }

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int CurrencyID { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "Ex-Rate")]
        public string ExternalRef { get; set; }


        public int CompanyID { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual ICollection<VoucherItems> VoucherItems { get; set; }        
    }
}

