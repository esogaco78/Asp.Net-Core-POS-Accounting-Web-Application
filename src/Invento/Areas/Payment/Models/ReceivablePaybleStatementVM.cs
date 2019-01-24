using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Payment.Models
{
    public class ReceivablePaybleStatementVM
    {
        public int ID { get; set; }
        public string PartyName { get; set; }
        public decimal Amount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public List<ReceivablePaybleStatementVM> StatementList { get; set; }
        public SelectList PartyList { get; set; }
    } 
}
