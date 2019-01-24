using Invento.Areas.Product.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.CompanyAdmin.Models.Company
{
    public class DashBoardVM
    {
        // Item Quantity Check
        public List<Item> ItemQuantiyList { get; set; }


        // Receivable      
        public List<ReceivableVM> StatementListReceive { get; set; }
        public List<ReceivableVM> StatementListPay { get; set; }
        public List<TransactionShow> PurTransactionList { get; set; }
        public List<TransactionShow> PurReturnTransactionList { get; set; }
        public List<TransactionShow> SaleTransactionList { get; set; }
        public List<TransactionShow> SeleReturnTransactionList { get; set; }
    }

    public class TransactionShow
    {
        public int ID { get; set; }
        public string Bank { get; set; }
        public decimal Amount { get; set; }
        public string Cheque { get; set; }
        public DateTime Date  { get; set; }
        public string PartyName { get; set; }
    }
    public class ReceivableVM
    {
        public int ID { get; set; }
        public string PartyName { get; set; }
        public decimal Amount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public List<ReceivableVM> StatementList { get; set; }        
    }
}
