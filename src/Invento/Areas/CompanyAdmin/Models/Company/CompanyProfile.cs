using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Invento.Areas.CompanyAdmin.Models.Company
{
    public class CompanyProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyProfileID { get; set; }

        public int CompanyID { get; set; }

        [Required]
        //[RegularExpression(@"^[A-Z]+[a-zA-Z''-'\s]*$", ErrorMessage = "Name Should start with capital letter.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Name cannot be longer than 50 characters.")]
        [Display(Name = "Company Name")]        
        public string CompanyName { get; set; }

        [Phone]
        [Display(Name = "Contact Number")]
        //[Required(ErrorMessage = "Contact Number Required")]
        //[RegularExpression(@"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$", ErrorMessage = "Entered Phone format is not valid.Eg.(333 1234567)(423 1234567)")]
        public string ContactNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }        
       
        [Phone]
        [Display(Name = "Office Number")]
        //[RegularExpression(@"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$", ErrorMessage = "Entered Phone format is not valid.Eg.(333 1234567)(423 1234567)")]
        public string OfficeContact { get; set; }

        [Phone]
        [Display(Name = "Alternative Contact #")]
        //[RegularExpression(@"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$", ErrorMessage = "Entered Phone format is not valid.Eg.(333 1234567)(423 1234567)")]
        public string AlternativeContact { get; set; }

        [Display(Name = "Website")]
        [RegularExpression(@"^((http:\/\/www\.)|(www\.)|(http:\/\/))[a-zA-Z0-9._-]+\.[a-zA-Z.]{2,5}$", ErrorMessage = "Incorrect Website Address.Always add www with the site name.")]
        public string CompanyWebsite { get; set; }

        [Display(Name = "Fax")]        
        public string Fax { get; set; }
        
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        // For Number Of users in Company Allowed
        public int NoOfCompanyUsersAllowed { get; set; }
        // For Number Of users in Company Allowed

        [Required]
        [Display(Name = "Country Name")]
        [ForeignKey("Country")]
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }        
    }
   
    public class SiteUserInfo
    {
        public string UserID { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public int CompanyID { get; set; }

        public bool AccountActive { get; set; }
        public List<IdentityRole> Roles { get; set; }
        
    }

}
