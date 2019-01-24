using Invento.Areas.Payment.Models;
using Invento.Areas.Purchase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Finance.Models
{
    public class GeneralLedgerVM
    {
        public bool Check_GL_Level { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public SelectList mainAccList { get; set; }

        public SelectList tranAccList { get; set; }

        public int IncomeID { get; set; }
        public int ExpenseID { get; set; }
        public int AssetID { get; set; }
        public int LiabilityID { get; set; }
        public int PartyID { get; set; }

        public decimal IncomeVal { get; set; }
        public decimal ExpenseVal { get; set; }
        public decimal AssetVal { get; set; }
        public decimal LiabilityVal { get; set; }
        public decimal PartyDebitors { get; set; }
        public decimal PartyCreditors { get; set; }
        public decimal Profit { get; set; }
        public decimal Loss { get; set; }

        public decimal OpeningBalance { get; set; }
        public List<LedgerListVM> LedgerList { get; set; }

        public List<LedgerListVM> CashBook_DR { get; set; }
        public List<LedgerListVM> CashBook_CR { get; set; }


        public List<PlusAllSubAccountsVM> ListPlus { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class Naration
    {
        public int id { get; set; }
        public string Narration { get; set; }
    }
    public class AA
    {
        public int id { get; set; }
        public int subID { get; set; }
        public int mainID { get; set; }
        public string MainAccountNumber { get; set; }
        public string SubAccountNumber { get; set; }
        public string TransactionAccountNumber { get; set; }
        public string TransactionAccountName { get; set; }
        
        public decimal OpeningBalance { get; set; }
    }

    public class PlusAllSubAccountsVM
    {
        public int ID { get; set; }
        public decimal TotalAdd { get; set; }
    }

    public class LedgerListVM
    {
        public int MainAccountID { get; set; }
        public string MainAccountNumber { get; set; }
        public string MainAccountName { get; set; }

        public string Narration_1 { get; set; }
        public string Narration_2 { get; set; }
        public string Narration_3 { get; set; }
        public string Narration_4 { get; set; }
        public string Narration_5 { get; set; }
        public string Narration_6 { get; set; }
        public string Narration_7 { get; set; }
        public string Narration_8 { get; set; }
        public string Narration_9 { get; set; }
        public string Narration_10 { get; set; }

        public DateTime? Date_1 { get; set; }
        public DateTime? Date_2 { get; set; }
        public DateTime? Date_3 { get; set; }
        public DateTime? Date_4 { get; set; }
        public DateTime? Date_5 { get; set; }
        public DateTime? Date_6 { get; set; }
        public DateTime? Date_7 { get; set; }
        public DateTime? Date_8 { get; set; }
        public DateTime? Date_9 { get; set; }
        public DateTime? Date_10 { get; set; }
        
        public int? Voucher_1 { get; set; }
        public int? Voucher_2 { get; set; }
        public int? Voucher_3 { get; set; }
        public int? Voucher_4 { get; set; }
        public int? Voucher_5 { get; set; }
        public int? Voucher_6 { get; set; }
        public int? Voucher_7 { get; set; }
        public int? Voucher_8 { get; set; }
        public int? Voucher_9 { get; set; }
        public int? Voucher_10 { get; set; }


        //public int? ID_Voucher_1 { get; set; }
        //public int? ID_Voucher_2 { get; set; }
        //public int? ID_Voucher_3 { get; set; }
        //public int? ID_Voucher_4 { get; set; }
        //public int? ID_Voucher_5 { get; set; }
        //public int? ID_Voucher_6 { get; set; }
        //public int? ID_Voucher_7 { get; set; }
        //public int? ID_Voucher_8 { get; set; }
        //public int? ID_Voucher_9 { get; set; }
        //public int? ID_Voucher_10 { get; set; }


        public int SubAccountID { get; set; }
        public string SubAccountNumber { get; set; }
        public string SubAccountName { get; set; }


        public int TransactionAccountID { get; set; }
        public string TransactionAccountNumber { get; set; }
        public string TransactionAccountName { get; set; }


        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance_Debit_Plus { get; set; }
        public decimal Balance_Credit_Minus { get; set; }
        public decimal Balance { get; set; }

        public virtual PurchaseBill PurchaseBill { get; set; }
    }

    public class CashFlowVM
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }        
        public List<CashFlow> CFList { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class CashFlowDetailsVM
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public string Title { get; set; }
        public string Name_1 { get; set; }
        public string Name_2 { get; set; }
        public string Name_3 { get; set; }
        public string Name_4 { get; set; }
        public string Name_5 { get; set; }
        public string Name_6 { get; set; }
        public decimal Decimal_1 { get; set; }
        public decimal Decimal_2 { get; set; }
        public string curreny { get; set; }
        public decimal Amount { get; set; }
        public DateTime date { get; set; }
    }
}
