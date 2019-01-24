using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Models.AccountViewModels
{
    public class RegisterVMCompanyUser
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        [Required]
        [Display(Name = "Company ID")]
        public int CompanyID { get; set; }

        [Required]
        [Display(Name = "Assign Roles to User")]
        public string[] SelectedRoles { get; set; }

        public int NoUserAllowed { get; set; }
        public List<IdentityRole> RoleList { get; set; }
    }
    public class UserEdit
    {
        [Required]
        [Display(Name = "Assign Roles to User")]
        public string[] SelectedRoles { get; set; }

        public string UserID { get; set; }
        public string Email { get; set; }
        public List<IdentityRole> RoleList { get; set; }
    }
}
