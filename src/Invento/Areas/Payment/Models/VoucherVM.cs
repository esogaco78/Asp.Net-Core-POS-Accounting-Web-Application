using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Payment.Models
{
    public class VoucherVM
    {       
        public int VoucherID { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public string Particulars { get; set; }

        [Display(Name = "Currency")]
        public int CurrencyID { get; set; }

        [Display(Name = "Ex-Rate")]
        public string ExternalRef { get; set; }

        [Display(Name = "Import Bill ID")]
        public string ImportExportID { get; set; }
        public int CompanyID { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }


        public List<VoucherItems> VIList { get; set; }

    }
}
