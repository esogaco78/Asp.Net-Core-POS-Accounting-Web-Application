using System.ComponentModel.DataAnnotations;

namespace Invento.Areas.Purchase.Models
{
    public enum PayTerms
    {
        [Display(Name = "Debit")]
        Debit,
        [Display(Name = "Credit")]
        Credit
    }

    public enum PaymentMode
    {
        [Display(Name = "Cash")]
        Cash,
        [Display(Name = "Cheque")]
        Cheque
    }
    public enum PurchaseType
    {
        [Display(Name = "Local")]
        Local,
        [Display(Name = "Import/Export")]
        ImportExport
    }
    public enum PaymentType
    {
        [Display(Name = "To Receive")]
        ToReceive,
        [Display(Name = "To Pay")]
        ToPay,
        [Display(Name = "Paid")]
        Paid
    }
    public enum ChequeStatus
    {
        [Display(Name = "Not Cleared")]
        NotCleared,
        [Display(Name = "Cleared")]
        Cleared
    }
}
