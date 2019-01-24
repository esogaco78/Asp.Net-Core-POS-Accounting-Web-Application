using Invento.Areas.CompanyAdmin.Models.Company;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Product.Models
{
    public class GRNVM
    {
        public int GRNID { get; set; }

        [Required]
        [Display(Name = "GRN Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime GRNDate { get; set; }

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }
        public string PartyName { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public string Remarks { get; set; }

        // For Total

        public int GRN_I_Extra { get; set; }

        [Required]
        [Display(Name = "Total Quantity")]
        public int TotalQuantity { get; set; }


        [Required]
        [Display(Name = "Company Name")]
        [ForeignKey("Parties")]
        public int PartiesID { get; set; }
        public virtual Parties Parties { get; set; }
        public virtual ICollection<GRNItem> GRNItem { get; set; }
        public List<Item> ItemList { get; set; }
    }
}
