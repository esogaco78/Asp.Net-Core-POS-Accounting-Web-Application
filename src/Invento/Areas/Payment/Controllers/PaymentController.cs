using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Product.Models;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Finance.Models;
using System.Data.SqlClient;
using Invento.Areas.Payment.Models;
using Invento.Areas.CompanyAdmin.Models.Company;

namespace Invento.Areas.Payment.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Payment")]
    [Route("Payment/[controller]")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Statement Of Receivable")]
        public IActionResult StatementOfReceivable(int? Party)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            ReceivablePaybleStatementVM model = new ReceivablePaybleStatementVM();

            int MainPartyAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
            int SubPartyAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainPartyAcc).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;
            
            if (Party == null)
            {
                model.StatementList = (from r in _context.CashFlow
                                       where r.CompanyID == CompID
                                       where r.MainAccountID == MainPartyAcc
                                       where r.SubAccountID == SubPartyAcc
                                       where r.PartiesID != null
                                       group r by r.PartiesID into g
                                       select new ReceivablePaybleStatementVM
                                       {
                                           ID = (int)g.Key,
                                           PartyName = g.Select(r => r.Parties.PartyName).FirstOrDefault(),
                                           // Amount = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                                           OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                           credit = g.Sum(x => x.Credit),
                                           debit = g.Sum(x => x.Debit)
                                       })
                                       //.Where(r => r.Amount > 0)
                                       .ToList();
            }
            else {
                model.StatementList = (from r in _context.CashFlow
                                       where r.CompanyID == CompID
                                       where r.MainAccountID == MainPartyAcc
                                       where r.SubAccountID == SubPartyAcc
                                       where r.PartiesID == Party
                                       group r by r.PartiesID into g
                                       select new ReceivablePaybleStatementVM
                                       {
                                           ID = (int)g.Key,
                                           PartyName = g.Select(r => r.Parties.PartyName).FirstOrDefault(),
                                           //Amount = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                                           OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                           credit = g.Sum(x => x.Credit),
                                           debit = g.Sum(x => x.Debit)
                                       })
                                       //.OrderByDescending(r => r.Amount)
                                       //    .Where(r => r.Amount > 0)
                                           .ToList();
            }

            var partyList = new List<Parties>();
            partyList = _context.Parties.Where(r => r.CompanyID == CompID).ToList();
            model.PartyList = new SelectList(partyList, "PartiesID", "PartyName");

            return View(model);
        }


        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Statement Of Payable")]
        public IActionResult StatementOfPayable(int? Party)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            ReceivablePaybleStatementVM model = new ReceivablePaybleStatementVM();

            int MainPartyAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
            int SubPartyAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainPartyAcc).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;
            if(Party == null)
            {
                model.StatementList = (from r in _context.CashFlow
                                       where r.CompanyID == CompID
                                       where r.MainAccountID == MainPartyAcc
                                       where r.SubAccountID == SubPartyAcc
                                       where r.PartiesID != null
                                       group r by r.PartiesID into g
                                       select new ReceivablePaybleStatementVM
                                       {
                                           ID = (int)g.Key,
                                           PartyName = g.Select(r => r.Parties.PartyName).FirstOrDefault(),
                                           OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                           credit = g.Sum(x => x.Credit),
                                           debit = g.Sum(x => x.Debit)
                                       })
                                       //.OrderByDescending(r => r.Amount)
                                        //.Where(r => r.Amount > 0)
                                        .ToList();
            }
            else
            {
                model.StatementList = (from r in _context.CashFlow
                                       where r.CompanyID == CompID
                                       where r.MainAccountID == MainPartyAcc
                                       where r.SubAccountID == SubPartyAcc
                                       where r.PartiesID == Party
                                       group r by r.PartiesID into g
                                       select new ReceivablePaybleStatementVM
                                       {
                                           ID = (int)g.Key,
                                           PartyName = g.Select(r => r.Parties.PartyName).FirstOrDefault(),
                                           //Amount = g.Sum(x => x.Credit) - g.Sum(x => x.Debit)
                                           OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                           credit = g.Sum(x => x.Credit),
                                           debit = g.Sum(x => x.Debit)
                                       })
                                       //.OrderByDescending(r => r.Amount)
                                       // .Where(r => r.Amount > 0)
                                        .ToList();
            }
            
            var partyList = new List<Parties>();
            partyList = _context.Parties.Where(r => r.CompanyID == CompID).ToList();
            model.PartyList = new SelectList(partyList, "PartiesID", "PartyName");

            return View(model);
        }      
    } 
}