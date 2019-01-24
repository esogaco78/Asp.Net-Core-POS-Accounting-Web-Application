using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Finance.Models;

namespace Invento.Areas.CompanyAdmin.Controllers
{
    [Area("CompanyAdmin")]
    [Route("Company/[controller]")]
    [Authorize(Roles = "BiznsBook")]
    public class BanksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BanksController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Banks")]
        public async Task<IActionResult> Index()
        {         
            int? CompID = HttpContext.Session.GetInt32("CompanyID");          

            return View(await _context.Bank.Where(r => r.CompanyID == CompID).ToListAsync());
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Banks")]
        public IActionResult Create()
        {
            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Banks")]
        public async Task<IActionResult> Create([Bind("BankID,BankDescription,BankName,CompanyID,CreatedBy")] Bank bank)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            bank.CompanyID = CompID;
            bank.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                // Chart of Account TRANSACTION ACCOUNT ENTRY
                TransactionAccount TraAcc = new TransactionAccount();

                TraAcc.AccountName = bank.BankName;
                TraAcc.CompanyID = CompID;
                TraAcc.CreatedBy = User.Identity.Name;
                TraAcc.CreationDate = DateTime.Now;
                TraAcc.OpeningBalance = 0;


                int mainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int subAccID = _context.SubAccount.Where(r => r.SubAccountNumber == "0002").Where(r => r.MainAccountID == mainAccID).FirstOrDefault().SubAccountID;
                
                if (_context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccID).Any() == false)
                {
                    TraAcc.TransactionAccountNumber = "0001";
                }
                else
                {
                    int CountTransAcc = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccID).Count();
                    int ADD_CountTransAcc = CountTransAcc + 1;
                    if (ADD_CountTransAcc < 10)
                    {
                        TraAcc.TransactionAccountNumber = "000" + ADD_CountTransAcc;
                    }
                    else if (ADD_CountTransAcc < 100)
                    {
                        TraAcc.TransactionAccountNumber = "00" + ADD_CountTransAcc;
                    }
                    else if (ADD_CountTransAcc < 1000)
                    {
                        TraAcc.TransactionAccountNumber = "0" + ADD_CountTransAcc;
                    }
                    else
                    {
                        TraAcc.TransactionAccountNumber = ADD_CountTransAcc.ToString();
                    }
                }

                TraAcc.SubAccountID = subAccID;

                _context.Add(TraAcc);
                await _context.SaveChangesAsync();
                int TransAccID_NEW = TraAcc.TransactionAccountID;
                // Here TransactionAccountID is Bank Transaction AccountID
                bank.TransactionAccountID = TransAccID_NEW;
                _context.Add(bank);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bank);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Banks")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var bank = await _context.Bank.SingleOrDefaultAsync(m => m.BankID == id);
            string TransAccNumber = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == bank.TransactionAccountID).FirstOrDefault().TransactionAccountNumber;
            bank.CreatedBy = TransAccNumber;
            if (bank == null)
            {
                return NotFound();
            }
            return PartialView(bank);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Banks")]
        public async Task<IActionResult> Edit([Bind("BankID,BankDescription,BankName,CompanyID,CreatedBy,TransactionAccountID")] Bank bank)
        {         
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            string TransAccNumber = bank.CreatedBy;
            bank.CompanyID = CompID;
            bank.CreatedBy = User.Identity.Name;
          
            int mainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
            int subAccID = _context.SubAccount.Where(r => r.SubAccountNumber == "0002").Where(r => r.MainAccountID == mainAccID).FirstOrDefault().SubAccountID;            

            TransactionAccount TraAcc = new TransactionAccount();
            TraAcc.TransactionAccountID = bank.TransactionAccountID;
            TraAcc.AccountName = bank.BankName;
            TraAcc.CompanyID = CompID;
            TraAcc.CreatedBy = User.Identity.Name;
            TraAcc.CreationDate = DateTime.Now;
            TraAcc.OpeningBalance = 0;
            TraAcc.SubAccountID = subAccID;
            TraAcc.TransactionAccountNumber = TransAccNumber;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bank);
                    _context.Update(TraAcc);                                     
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankExists(bank.BankID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }            
            return PartialView(bank);
        }

        private bool BankExists(int id)
        {
            return _context.Bank.Any(e => e.BankID == id);
        }

        [Route("[action]")]
        public JsonResult LoadBanks()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.Bank.Where(r => r.CompanyID == CompID).ToList();
            return Json(new { data = data });
        }
    }
}
 