using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.SiteAdmin.Models
{
    public class SiteManageVM
    {
        [Required]
        public int CompanyID { get; set; }
    }

}
