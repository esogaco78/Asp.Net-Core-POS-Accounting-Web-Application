using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Invento.Areas.CompanyAdmin.Models.Company
{
    public class Country
    {
        public int CountryID { get; set; }
        public string ISO { get; set; }
        [Required]
        [Display(Name = "Country Name")]
        public string Name { get; set; }
        public string NiceName { get; set; }
        public string Iso3 { get; set; }
        public string NumCode { get; set; }
        public string PhoneCode { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public virtual ICollection<CompanyProfile> CompanyProfile { get; set; }
        public virtual ICollection<Parties> Parties { get; set; }
    }
}
