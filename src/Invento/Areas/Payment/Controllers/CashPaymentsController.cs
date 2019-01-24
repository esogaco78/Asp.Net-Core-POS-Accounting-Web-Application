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
    public class CashPaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashPaymentsController(ApplicationDbContext context)
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

            var cashPayment = await _context.CashPayment.SingleOrDefaultAsync(m => m.CashPaymentID == id);
            if (cashPayment == null)
            {
                return NotFound();
            }

            return View(cashPayment);
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
        public async Task<IActionResult> Create([Bind("CashPaymentID,Amount,AmountInWords,BankID,CompanyID,CreatedBy,CreationDate,CurrencyID,Date,ExternalRef,Particulars,PartiesID,Payee,ImportExportID")] CashPayment cashPayment)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            cashPayment.CompanyID = CompID;
            cashPayment.CreatedBy = User.Identity.Name;
            cashPayment.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(cashPayment);
                await _context.SaveChangesAsync();
                int CIB = cashPayment.CashPaymentID;

                CashFlow CF2 = new CashFlow();
                CF2.Debit = cashPayment.Amount;
                CF2.CashPaymentID = CIB;
                CF2.CompanyID = CompID;
                CF2.PartiesID = cashPayment.PartiesID;
                // Party Debit
                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == cashPayment.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == PartMainAcc1_2).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc1_2;
                CF2.SubAccountID = PartSubAcc1_2;
                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.VoucherType = "Cash Payment Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Credit = cashPayment.Amount;
                CF3.CashPaymentID = CIB;
                CF3.CompanyID = CompID;
                CF3.PartiesID = cashPayment.PartiesID;

                // Cash In Hand 01-0001-0001 Credit
                int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF3.MainAccountID = MainAccID_2;
                CF3.SubAccountID = SubAccID_2;
                CF3.TransactionAccountID = TraAccID_2;
                CF3.VoucherType = "Cash Payment Voucher";
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
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashPayment.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashPayment.PartiesID);
            return View(cashPayment);
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

            var cashPayment = await _context.CashPayment.SingleOrDefaultAsync(m => m.CashPaymentID == id);
            if (cashPayment == null)
            {
                return NotFound();
            }
            
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashPayment.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashPayment.PartiesID);
            return View(cashPayment);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit(int id, [Bind("CashPaymentID,Amount,AmountInWords,CompanyID,CreatedBy,CreationDate,CurrencyID,Date,ExternalRef,Particulars,PartiesID,Payee,ImportExportID")] CashPayment cashPayment)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int CIB = cashPayment.CashPaymentID;
            cashPayment.CompanyID = CompID;
            cashPayment.CreatedBy = User.Identity.Name;
            cashPayment.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cashPayment);
                    await _context.SaveChangesAsync();

                    // Cash Flow VOUCHER EDIT                    

                    _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.CashPaymentID == CIB));
                    _context.SaveChanges();

                    CashFlow CF2 = new CashFlow();
                    CF2.Debit = cashPayment.Amount;
                    CF2.CashPaymentID = CIB;
                    CF2.CompanyID = CompID;
                    CF2.PartiesID = cashPayment.PartiesID;
                    // Party Debit
                    int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == cashPayment.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == PartMainAcc1_2).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF2.MainAccountID = PartMainAcc1_2;
                    CF2.SubAccountID = PartSubAcc1_2;
                    CF2.TransactionAccountID = (int)PartyTransID;
                    CF2.VoucherType = "Cash Payment Voucher";
                    CF2.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF2);
                    await _context.SaveChangesAsync();

                    CashFlow CF3 = new CashFlow();
                    CF3.Credit = cashPayment.Amount;
                    CF3.CashPaymentID = CIB;
                    CF3.CompanyID = CompID;
                    CF3.PartiesID = cashPayment.PartiesID;

                    // Cash In Hand 01-0001-0001 Credit
                    int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                    int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                    int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                    CF3.MainAccountID = MainAccID_2;
                    CF3.SubAccountID = SubAccID_2;
                    CF3.TransactionAccountID = TraAccID_2;
                    CF3.VoucherType = "Cash Payment Voucher";
                    CF3.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF3);
                    await _context.SaveChangesAsync();
                    
                    // Cash Flow VOUCHER EDIT
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CashPaymentExists(cashPayment.CashPaymentID))
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
            
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", cashPayment.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", cashPayment.PartiesID);
            return View(cashPayment);
        }



        private bool CashPaymentExists(int id)
        {
            return _context.CashPayment.Any(e => e.CashPaymentID == id);
        }

        [Route("[action]")]
        public JsonResult LoadCashPayments()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
           
            var data = _context.CashPayment.Where(r => r.CompanyID == CompID).Include(r => r.Parties).Select(x => new { x.CashPaymentID, date = x.Date.ToString("d"), x.Parties.PartyName, x.Amount, x.Particulars, x.Payee, x.AmountInWords ,x.ImportExportID, x.CreatedBy });

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

//    var cashPayment = await _context.CashPayment.SingleOrDefaultAsync(m => m.CashPaymentID == id);
//    if (cashPayment == null)
//    {
//        return NotFound();
//    }

//    return View(cashPayment);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var cashPayment = await _context.CashPayment.SingleOrDefaultAsync(m => m.CashPaymentID == id);
//    _context.CashPayment.Remove(cashPayment);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}