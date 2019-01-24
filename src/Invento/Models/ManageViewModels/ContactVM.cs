using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Models.ManageViewModels
{
    public class ContactVM
    {
        [Required]        
        [Display(Name = "FULL NAME")]
        public string FullName{ get; set; }
                
        [Required]
        [Display(Name = "EMAIL")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "PHONE NUMBER")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "COMPANY")]        
        public string Company { get; set; }
        
        [Required]
        [Display(Name = "MESSAGE")]
        public string Message { get; set; }
        
        [Display(Name = "CUSTOMER NO.")]
        public string CustomerNo { get; set; }
    }
}
