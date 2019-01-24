using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Payment.Models;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Finance.Models;

namespace Invento.Areas.Payment.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Payment")]
    [Route("Payment/[controller]")]
    public class CashInBanksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashInBanksController(ApplicationDbContext context)
        {
            _context = context;    
        }
        
        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Vouchers")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Vouchers Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashInBank = await _context.CashInBank.SingleOrDefaultAsync(m => m.CashInBankID == id);
            if (cashInBank == null)
            {
                return NotFound();
            }

            return View(cashInBank);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Vouchers")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Vouchers")]
        public async Task<IActionResult> Create([Bind("CashInBankID,Amount,AmountInWords,BankID,CompanyID,CreatedBy,CreationDate,CurrencyID,Date,DepositedBy,ExternalRef,Particulars,PartiesID")] CashInBank cashInBank)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            cashInBank.CompanyID = CompID;
            cashInBank.CreatedBy = User.Identity.Name;
            cashInBank.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(cashInBank);
                await _context.SaveChangesAsync();
                int CIB = cashInBank.CashInBankID;
                
                CashFlow CF2 = new CashFlow();
                CF2.Credit = cashInBank.Amount;
                CF2.CashInBankID = CIB;
                CF2.CompanyID = CompID;
                CF2.PartiesID = cashInBank.PartiesID;
                // Party Credit
                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == cashInBank.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc1_2;
                CF2.SubAccountID = PartSubAcc1_2;
                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.VoucherType = "Cash In Bank Voucher";
                CF2.DateCreation = DateTime.Now.Date;
             //   CF2.Narration = 
                    
                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = cashInBank.Amount;
                CF3.CashInBankID = CIB;
                CF3.CompanyID = CompID;
                CF3.PartiesID = cashInBank.PartiesID;
                // Bank Debit
                int? bankID = cashInBank.BankID;
                int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                CF3.MainAccountID = MainAccID_3;
                CF3.SubAccountID = SubAccID_3;
                CF3.TransactionAccountID = transBankID;
                CF3.VoucherType = "Cash In Bank Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();

                if(User.IsInRole("CompanyAdmin"))
                {
                    return RedirectToAction("Index");
                }                
                else
                {
                    return RedirectToAction("Index","Home",new { area = "CompanyUser" } );
                }
            }
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", cashInBank.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashInBank.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashInBank.PartiesID);
            return View(cashInBank);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var cashInBank = await _context.CashInBank.SingleOrDefaultAsync(m => m.CashInBankID == id);
            if (cashInBank == null)
            {
                return NotFound();
            }
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", cashInBank.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency, "CurrencyID", "CurrencyName", cashInBank.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashInBank.PartiesID);
            return View(cashInBank);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit([Bind("CashInBankID,Amount,AmountInWords,BankID,CompanyID,CreatedBy,CreationDate,CurrencyID,Date,DepositedBy,ExternalRef,Particulars,PartiesID")] CashInBank cashInBank)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int CIB = cashInBank.CashInBankID;
            cashInBank.CompanyID = CompID;
            cashInBank.CreatedBy = User.Identity.Name;
            cashInBank.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cashInBank);
                    await _context.SaveChangesAsync();

                    // Cash Flow VOUCHER EDIT                    

                    _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.CashInBankID == CIB));
                    _context.SaveChanges();

                    CashFlow CF2 = new CashFlow();
                    CF2.Credit = cashInBank.Amount;
                    CF2.CashInBankID = CIB;
                    CF2.CompanyID = CompID;
                    CF2.PartiesID = cashInBank.PartiesID;
                    // Party Credit
                    int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == cashInBank.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF2.MainAccountID = PartMainAcc1_2;
                    CF2.SubAccountID = PartSubAcc1_2;
                    CF2.TransactionAccountID = (int)PartyTransID;
                    CF2.VoucherType = "Cash In Bank Voucher";
                    CF2.DateCreation = DateTime.Now.Date;
                    //   CF2.Narration = 

                    _context.CashFlow.Add(CF2);
                    await _context.SaveChangesAsync();

                    CashFlow CF3 = new CashFlow();
                    CF3.Debit = cashInBank.Amount;
                    CF3.CashInBankID = CIB;
                    CF3.CompanyID = CompID;
                    CF3.PartiesID = cashInBank.PartiesID;
                    // Bank Debit
                    int? bankID = cashInBank.BankID;
                    int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                    int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                    int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                    CF3.MainAccountID = MainAccID_3;
                    CF3.SubAccountID = SubAccID_3;
                    CF3.TransactionAccountID = transBankID;
                    CF3.VoucherType = "Cash In Bank Voucher";
                    CF3.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF3);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashInBankExists(cashInBank.CashInBankID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (User.IsInRole("CompanyAdmin"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "CompanyUser" });
                }
            }
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", cashInBank.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashInBank.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashInBank.PartiesID);
            return View(cashInBank);
        }
        
        private bool CashInBankExists(int id)
        {
            return _context.CashInBank.Any(e => e.CashInBankID == id);
        }

        [Route("[action]")]
        public JsonResult LoadCashInBanks()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var data = _context.CashInBank.Where(r => r.CompanyID == CompID).Include(r => r.Parties).Select(x => new { x.CashInBankID, date = x.Date.ToString("d"), x.Parties.PartyName, x.Amount, x.Particulars, x.DepositedBy, x.AmountInWords ,x.CreatedBy});

            return Json(new { data = data });
        }
    }
}


//[Route("[action]")]
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var cashInBank = await _context.CashInBank.SingleOrDefaultAsync(m => m.CashInBankID == id);
//    if (cashInBank == null)
//    {
//        return NotFound();
//    }

//    return View(cashInBank);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var cashInBank = await _context.CashInBank.SingleOrDefaultAsync(m => m.CashInBankID == id);
//    _context.CashInBank.Remove(cashInBank);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}
