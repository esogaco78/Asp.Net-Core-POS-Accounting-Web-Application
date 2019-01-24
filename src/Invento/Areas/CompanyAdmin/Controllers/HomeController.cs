using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Invento.Data;
using Microsoft.EntityFrameworkCore;
using Invento.Models;
using Invento.Areas.CompanyAdmin.Models.Company;
using Microsoft.AspNetCore.Http;

namespace Invento.Areas.CompanyAdmin.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Authorize(Roles = "CompanyAdmin")]
    [Area("CompanyAdmin")]
    [Route("Company/[controller]")]    
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
                                   }).OrderByDescending(r=>r.credit)
                                   //.Take(4)                            
                                   .ToList();

            DBvm.ItemQuantiyList = _context.Item.Where(r => r.CompanyID == CompID).Where(r => r.Quantity < 10).OrderBy(r=>r.Quantity).Take(10).ToList();            

            DBvm.PurTransactionList = _context.Transaction.Where(r => r.CompanyID == CompID).Where(r=>r.Paid == false).Where(r=>r.Date.Date ==  DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.PurchaseBill.Parties.PartyName , Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.TransactionID }).ToList();
            DBvm.PurReturnTransactionList = _context.PurchaseReturnTransaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r=>r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.PurchaseReturn.Parties.PartyName , Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.PurchaseReturnTransactionID }).ToList();
            DBvm.SaleTransactionList = _context.SaleTransaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r => r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.SaleBill.Parties.PartyName , Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.SaleTransactionID }).ToList();
            DBvm.SeleReturnTransactionList = _context.SaleReturnTransaction.Where(r => r.CompanyID == CompID).Where(r => r.Paid == false).Where(r => r.Date.Date == DateTime.Now.Date).Select(r => new TransactionShow { PartyName = r.SaleReturn.Parties.PartyName , Amount = r.Amount, Bank = r.Bank.BankName, Cheque = r.Cheque, Date = r.Date, ID = r.SaleReturnID }).ToList();

            return View(DBvm);
        }


        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin")]
        public IActionResult MyUsers()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompanyID = Convert.ToInt32(CompId);
            
            string CurrentRole;
            var userList = new List<SiteUserInfo>();
            foreach (var user in _context.Users.Include(r => r.Roles).Where(r => r.CompanyID == CompanyID).ToList())
            {
                var userRolesId = user.Roles.Select(m => m.RoleId).ToList();
                var model = new SiteUserInfo()
                {
                    UserID = user.Id,
                    Email = user.Email,
                    CompanyID = user.CompanyID,
                    AccountActive = user.AccountActive,
                    Roles = _context.Roles.Where(r => userRolesId.Contains(r.Id)).ToList()
                };
                List<string> RoleListTemp = new List<string>();
                string RoleNamesTemp;
                for (int i = 0; i < model.Roles.Count; i++)
                {
                    CurrentRole = model.Roles[i].Name;
                    RoleListTemp.Add(CurrentRole);
                }
                RoleNamesTemp = string.Join(" , ", RoleListTemp.ToArray());
                model.RoleName = RoleNamesTemp;
                userList.Add(model);
            }
            userList.RemoveAll(r => r.Email == User.Identity.Name);            
            return View(userList);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> AccountActive([Bind("UserID")] string id)
        {
            ApplicationUser UserModel = new ApplicationUser();
            UserModel = _context.Users.Where(r => r.Id == id).FirstOrDefault();

            UserModel.AccountActive = false;

            _context.Users.Update(UserModel);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyUsers");
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> AccountDeActive([Bind("UserID")] string id)
        {
            ApplicationUser UserModel = new ApplicationUser();
            UserModel = _context.Users.Where(r => r.Id == id).FirstOrDefault();

            UserModel.AccountActive = true;

            _context.Users.Update(UserModel);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyUsers");
        }

    }
}