using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Invento.Areas.CompanyAdmin.Models.Company;
using Microsoft.AspNetCore.Http;
using Invento.Data;

namespace Invento.Areas.CompanyUser.Controllers
{
    [Authorize(Roles = "BiznsBook")]    
    [Area("CompanyUser")]
    [Route("CompanyUser/[controller]")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("[action]")]
        public IActionResult Index()
        {
            DashBoardVM DBvm = new DashBoardVM();

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            HttpContext.Session.SetInt32("CompanyID", CompID);

            int MainPartyAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
            int SubPartyAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainPartyAcc).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

            DBvm.StatementListReceive = (from r in _context.CashFlow
                                         where r.CompanyID == CompID
                                         where r.MainAccountID == MainPartyAcc
                                         where r.SubAccountID == SubPartyAcc
                                         where r.PartiesID != null
                                         group r by r.PartiesID into g
                                         select new ReceivableVM
                                         {
                                             ID = (int)g.Key,
                                             PartyName = g.Select(r => r.Parties.PartyName).FirstOrDefault(),
                                             OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                             credit = g.Sum(x => x.Credit),
                                             debit = g.Sum(x => x.Debit)
                                         }).OrderByDescending(r => r.debit)
                                   //.Take(4)
                                   .ToList();

            DBvm.StatementListPay = (from r in _context.CashFlow
                                     where r.CompanyID == CompID
                                     where r.MainAccountID == MainPartyAcc
                                     where r.SubAccountID == SubPartyAcc
                                     where r.PartiesID != null
                                     group r by r.PartiesID into g
                                     select new ReceivableVM
                                     {
                                         ID = (int)g.Key,
                                         PartyName = g.Select(r => r.Parties.PartyName).FirstOrDefault(),
                                         OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                         credit = g.Sum(x => x.Credit),
                                         debit = g.Sum(x => x.Debit)
                                     }).OrderByDescending(r => r.credit)
                                   //.Take(4)                            
                                   .ToList();

            DBvm.ItemQuantiyList = _context.Item.Where(r => r.CompanyID == CompID).Where(r => r.Quantity < 10).OrderBy(r => r.Quantity).Take(10).ToList();

            DBvm.PurTransactionList = _context.Transaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r => r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.PurchaseBill.Parties.PartyName, Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.TransactionID }).ToList();
            DBvm.PurReturnTransactionList = _context.PurchaseReturnTransaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r => r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.PurchaseReturn.Parties.PartyName, Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.PurchaseReturnTransactionID }).ToList();
            DBvm.SaleTransactionList = _context.SaleTransaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r => r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.SaleBill.Parties.PartyName, Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.SaleTransactionID }).ToList();
            DBvm.SeleReturnTransactionList = _context.SaleReturnTransaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r => r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.SaleReturn.Parties.PartyName, Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.SaleReturnID }).ToList();

            return View(DBvm);
        }


    }
}