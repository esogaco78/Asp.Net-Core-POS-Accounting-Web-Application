using System.ComponentModel.DataAnnotations;

namespace Invento.Areas.Sale.Models
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
    //public enum PaymentType
    //{
    //    [Display(Name = "To Receive")]
    //    ToReceive,
    //    [Display(Name = "To Pay")]
    //    ToPay,
    //    [Display(Name = "Paid")]
    //    Paid
    //}
}
