using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Finance.Models;
using Invento.Areas.Product.Models;
using Invento.Areas.Sale.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Sale.Models
{
    public class SaleReturn
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleReturnID { get; set; }

        [Required]
        [Display(Name = "Sale Bill ID")]
        [ForeignKey("SaleBill")]
        public int SaleBillID { get; set; }
        public virtual SaleBill SaleBill { get; set; }

        [Required]
        [Display(Name = "Item ID")]
        [ForeignKey("Item")]
        public int ItemID { get; set; }
        public virtual Item Item { get; set; }
        
        [Display(Name = "Company Name")]
        [ForeignKey("Parties")]
        public int? PartiesID { get; set; }
        public virtual Parties Parties { get; set; }


        //public virtual PartyPayment PartyPayment { get; set; }

        [Display(Name = "Old Quantity")]
        public decimal OldQuantity { get; set; }

        [Display(Name = "Return Quantity")]
        public decimal ReturnQuantity { get; set; }

        [Display(Name = "Amount To Pay")]
        public decimal AmountToPay { get; set; }

        
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }


        [Required]
        [Display(Name = "Sale Return Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SaleBillReturnDate { get; set; }
          
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }

        public string Remarks { get; set; }

        //Extra Fields     
        public int SR_I_1 { get; set; }
        public int SR_I_2 { get; set; }
        public decimal SR_D_1 { get; set; }
        public decimal SR_D_2 { get; set; }
        public string SR_S_1 { get; set; }
        public string SR_S_2 { get; set; }
        public bool SR_B_1 { get; set; }
        //Extra Fields     
        public virtual ICollection<SaleReturnTransaction> SaleReturnTransaction { get; set; }
        public virtual ICollection<CashFlow> CashFlow { get; set; }
    }
}
