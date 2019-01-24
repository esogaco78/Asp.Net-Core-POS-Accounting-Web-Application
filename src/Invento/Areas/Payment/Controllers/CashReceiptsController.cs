using System;
using System.Collections.Generic;
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
    public class CashReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashReceiptsController(ApplicationDbContext context)
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

            var cashReceipt = await _context.CashReceipt.SingleOrDefaultAsync(m => m.CashReceiptID == id);
            if (cashReceipt == null)
            {
                return NotFound();
            }

            return View(cashReceipt);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Vouchers")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Vouchers")]
        public async Task<IActionResult> Create([Bind("CashReceiptID,Amount,AmountInWords,CompanyID,CreatedBy,CreationDate,CurrencyID,Date,ExternalRef,PaidBy,Particulars,PartiesID")] CashReceipt cashReceipt)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            cashReceipt.CompanyID = CompID;
            cashReceipt.CreatedBy = User.Identity.Name;
            cashReceipt.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(cashReceipt);
                await _context.SaveChangesAsync();
                int CIB = cashReceipt.CashReceiptID;

                CashFlow CF2 = new CashFlow();
                CF2.Credit = cashReceipt.Amount;
                CF2.CashReceiptID = CIB;
                CF2.CompanyID = CompID;
                CF2.PartiesID = cashReceipt.PartiesID;
                // Party Credit
                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == cashReceipt.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc1_2;
                CF2.SubAccountID = PartSubAcc1_2;

                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.VoucherType = "Cash Receipt Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = cashReceipt.Amount;
                CF3.CashReceiptID = CIB;
                CF3.CompanyID = CompID;
                CF3.PartiesID = cashReceipt.PartiesID;

                // Cash In Hand 01-0001-0001 Debit
                int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF3.MainAccountID = MainAccID_2;
                CF3.SubAccountID = SubAccID_2;
                CF3.TransactionAccountID = TraAccID_2;
                CF3.VoucherType = "Cash Receipt Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();
                
                if (User.IsInRole("CompanyAdmin"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "CompanyUser" });
                }
            }
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashReceipt.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashReceipt.PartiesID);
            return View(cashReceipt);
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

            var cashReceipt = await _context.CashReceipt.SingleOrDefaultAsync(m => m.CashReceiptID == id);
            if (cashReceipt == null)
            {
                return NotFound();
            }
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashReceipt.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashReceipt.PartiesID);
            return View(cashReceipt);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit(int id, [Bind("CashReceiptID,Amount,AmountInWords,CompanyID,CreatedBy,CreationDate,CurrencyID,Date,ExternalRef,PaidBy,Particulars,PartiesID")] CashReceipt cashReceipt)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int CIB = cashReceipt.CashReceiptID;
            cashReceipt.CompanyID = CompID;
            cashReceipt.CreatedBy = User.Identity.Name;
            cashReceipt.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cashReceipt);
                    await _context.SaveChangesAsync();

                    // Cash Flow VOUCHER EDIT                    

                    _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.CashReceiptID == CIB));
                    _context.SaveChanges();

                    CashFlow CF2 = new CashFlow();
                    CF2.Credit = cashReceipt.Amount;
                    CF2.CashReceiptID = CIB;
                    CF2.CompanyID = CompID;
                    CF2.PartiesID = cashReceipt.PartiesID;
                    // Party Credit
                    int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == cashReceipt.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF2.MainAccountID = PartMainAcc1_2;
                    CF2.SubAccountID = PartSubAcc1_2;

                    CF2.TransactionAccountID = (int)PartyTransID;
                    CF2.VoucherType = "Cash Receipt Voucher";
                    CF2.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF2);
                    await _context.SaveChangesAsync();

                    CashFlow CF3 = new CashFlow();
                    CF3.Debit = cashReceipt.Amount;
                    CF3.CashReceiptID = CIB;
                    CF3.CompanyID = CompID;
                    CF3.PartiesID = cashReceipt.PartiesID;

                    // Cash In Hand 01-0001-0001 Debit
                    int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                    int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                    int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                    CF3.MainAccountID = MainAccID_2;
                    CF3.SubAccountID = SubAccID_2;
                    CF3.TransactionAccountID = TraAccID_2;
                    CF3.VoucherType = "Cash Receipt Voucher";
                    CF3.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF3);
                    await _context.SaveChangesAsync();


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashReceiptExists(cashReceipt.CashReceiptID))
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
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashReceipt.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashReceipt.PartiesID);
            return View(cashReceipt);
        }

        private bool CashReceiptExists(int id)
        {
            return _context.CashReceipt.Any(e => e.CashReceiptID == id);
        }

        [Route("[action]")]
        public JsonResult LoadCashReceipts()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var data = _context.CashReceipt.Where(r => r.CompanyID == CompID).Include(r => r.Parties).Select(x => new { x.CashReceiptID, date = x.Date.ToString("d"), x.Parties.PartyName, x.Amount, x.Particulars, x.PaidBy, x.AmountInWords, x.CreatedBy });

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

//    var cashReceipt = await _context.CashReceipt.SingleOrDefaultAsync(m => m.CashReceiptID == id);
//    if (cashReceipt == null)
//    {
//        return NotFound();
//    }

//    return View(cashReceipt);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var cashReceipt = await _context.CashReceipt.SingleOrDefaultAsync(m => m.CashReceiptID == id);
//    _context.CashReceipt.Remove(cashReceipt);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}