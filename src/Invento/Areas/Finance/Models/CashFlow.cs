using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Payment.Models;
using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Finance.Models
{
    public class CashFlow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashFlowID { get; set; }
        
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public string Narration { get; set; }
        public string VoucherType { get; set; }
        public string Details { get; set; }
        public DateTime DateCreation { get; set; }
        public int CompanyID{ get; set; }
        

        [ForeignKey("SubAccount")]
        public int? SubAccountID { get; set; }
        public virtual SubAccount SubAccount { get; set; }

        [ForeignKey("MainAccount")]
        public int? MainAccountID { get; set; }
        public virtual MainAccount MainAccount { get; set; }

        [ForeignKey("TransactionAccount")]
        public int? TransactionAccountID { get; set; }
        public virtual TransactionAccount TransactionAccount { get; set; }

        [ForeignKey("Parties")]
        public int? PartiesID { get; set; }
        public virtual Parties Parties { get; set; }
       
        [ForeignKey("PurchaseBill")]        
        public int? PurchaseBillID { get; set; }
        public virtual PurchaseBill PurchaseBill { get; set; }


        [ForeignKey("SaleBill")]        
        public int? SaleBillID { get; set; }
        public virtual SaleBill SaleBill { get; set; }

        [ForeignKey("CashInBank")]
        public int? CashInBankID { get; set; }
        public virtual CashInBank CashInBank { get; set; }

        [ForeignKey("CashPayment")]
        public int? CashPaymentID { get; set; }
        public virtual CashPayment CashPayment { get; set; }

        [ForeignKey("CashReceipt")]
        public int? CashReceiptID { get; set; }
        public virtual CashReceipt CashReceipt { get; set; }

        [ForeignKey("ChequePayment")]
        public int? ChequePaymentID { get; set; }
        public virtual ChequePayment ChequePayment { get; set; }

        [ForeignKey("ChequeReceipt")]
        public int? ChequeReceiptID { get; set; }
        public virtual ChequeReceipt ChequeReceipt { get; set; }

        [ForeignKey("PurchaseReturn")]
        public int? PurchaseReturnID { get; set; }
        public virtual PurchaseReturn PurchaseReturn { get; set; }

        [ForeignKey("SaleReturn")]
        public int? SaleReturnID { get; set; }
        public virtual SaleReturn SaleReturn { get; set; }

        [ForeignKey("VoucherItems")]
        public int? VoucherItemsID { get; set; }
        public virtual VoucherItems VoucherItems { get; set; }

    }
}
