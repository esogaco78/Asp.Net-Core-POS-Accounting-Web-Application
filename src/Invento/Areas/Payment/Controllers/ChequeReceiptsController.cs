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
    public class ChequeReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChequeReceiptsController(ApplicationDbContext context)
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

            var chequeReceipt = await _context.ChequeReceipt.SingleOrDefaultAsync(m => m.ChequeReceiptID == id);
            if (chequeReceipt == null)
            {
                return NotFound();
            }

            return View(chequeReceipt);
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
        public async Task<IActionResult> Create([Bind("ChequeReceiptID,ChequeStatus,Amount,AmountInWords,BankID,ChequeNumber,CompanyID,CreatedBy,CreationDate,CurrencyID,CurrentStatus,Date,DateOfDeposite,DateOfMature,ExternalRef,InNameOf,Particulars,PartiesID")] ChequeReceipt chequeReceipt)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            chequeReceipt.CompanyID = CompID;
            chequeReceipt.CreatedBy = User.Identity.Name;
            chequeReceipt.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(chequeReceipt);
                await _context.SaveChangesAsync();
                int CIB = chequeReceipt.ChequeReceiptID;

                if (chequeReceipt.ChequeStatus == Purchase.Models.ChequeStatus.Cleared)
                {
                    CashFlow CF2 = new CashFlow();
                    CF2.Credit = chequeReceipt.Amount;
                    CF2.ChequeReceiptID = CIB;
                    CF2.CompanyID = CompID;
                    CF2.PartiesID = chequeReceipt.PartiesID;
                    // Party Credit
                    int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == chequeReceipt.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF2.MainAccountID = PartMainAcc1;
                    CF2.SubAccountID = PartSubAcc1;
                    CF2.TransactionAccountID = (int)PartyTransID;
                    CF2.VoucherType = "Cheque Receipt Voucher";
                    CF2.DateCreation = chequeReceipt.DateOfMature;

                    _context.CashFlow.Add(CF2);
                    await _context.SaveChangesAsync();

                    CashFlow CF3 = new CashFlow();
                    CF3.Debit = chequeReceipt.Amount;
                    CF3.ChequeReceiptID = CIB;
                    CF3.CompanyID = CompID;
                    CF3.PartiesID = chequeReceipt.PartiesID;
                    // Bank Debit
                    int? bankID = chequeReceipt.BankID;
                    int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                    int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                    int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                    CF3.MainAccountID = MainAccID_3;
                    CF3.SubAccountID = SubAccID_3;
                    CF3.TransactionAccountID = transBankID;
                    CF3.VoucherType = "Cheque Receipt Voucher";
                    CF3.DateCreation = chequeReceipt.DateOfMature;

                    _context.CashFlow.Add(CF3);
                    await _context.SaveChangesAsync();
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
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", chequeReceipt.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", chequeReceipt.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", chequeReceipt.PartiesID);
            return View(chequeReceipt);
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

            var chequeReceipt = await _context.ChequeReceipt.SingleOrDefaultAsync(m => m.ChequeReceiptID == id);
            if (chequeReceipt == null)
            {
                return NotFound();
            }
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", chequeReceipt.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", chequeReceipt.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", chequeReceipt.PartiesID);
            return View(chequeReceipt);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit([Bind("ChequeReceiptID,ChequeStatus,Amount,AmountInWords,BankID,ChequeNumber,CompanyID,CreatedBy,CreationDate,CurrencyID,CurrentStatus,Date,DateOfDeposite,DateOfMature,ExternalRef,InNameOf,Particulars,PartiesID")] ChequeReceipt chequeReceipt)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int CIB = chequeReceipt.ChequeReceiptID;
            chequeReceipt.CompanyID = CompID;
            chequeReceipt.CreatedBy = User.Identity.Name;
            chequeReceipt.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chequeReceipt);
                    await _context.SaveChangesAsync();

                    // Cash Flow VOUCHER EDIT                    

                    _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.ChequeReceiptID == CIB));
                    _context.SaveChanges();

                    if (chequeReceipt.ChequeStatus == Purchase.Models.ChequeStatus.Cleared)
                    {
                        CashFlow CF2 = new CashFlow();
                        CF2.Credit = chequeReceipt.Amount;
                        CF2.ChequeReceiptID = CIB;
                        CF2.CompanyID = CompID;
                        CF2.PartiesID = chequeReceipt.PartiesID;
                        // Party Credit
                        int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == chequeReceipt.PartiesID).FirstOrDefault().TransactionAccountID;
                        int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                        int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                        CF2.MainAccountID = PartMainAcc1;
                        CF2.SubAccountID = PartSubAcc1;
                        CF2.TransactionAccountID = (int)PartyTransID;
                        CF2.VoucherType = "Cheque Receipt Voucher";
                        CF2.DateCreation = chequeReceipt.DateOfMature;

                        _context.CashFlow.Add(CF2);
                        await _context.SaveChangesAsync();

                        CashFlow CF3 = new CashFlow();
                        CF3.Debit = chequeReceipt.Amount;
                        CF3.ChequeReceiptID = CIB;
                        CF3.CompanyID = CompID;
                        CF3.PartiesID = chequeReceipt.PartiesID;
                        // Bank Debit
                        int? bankID = chequeReceipt.BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF3.MainAccountID = MainAccID_3;
                        CF3.SubAccountID = SubAccID_3;
                        CF3.TransactionAccountID = transBankID;
                        CF3.VoucherType = "Cheque Receipt Voucher";
                        CF3.DateCreation = chequeReceipt.DateOfMature;

                        _context.CashFlow.Add(CF3);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChequeReceiptExists(chequeReceipt.ChequeReceiptID))
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
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", chequeReceipt.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", chequeReceipt.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", chequeReceipt.PartiesID);
            return View(chequeReceipt);
        }

        private bool ChequeReceiptExists(int id)
        {
            return _context.ChequeReceipt.Any(e => e.ChequeReceiptID == id);
        }

        [Route("[action]")]
        public JsonResult LoadChequeReceipts()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.ChequeReceipt.Where(r => r.CompanyID == CompID).Include(r => r.Parties).Include(r => r.Bank).Select(x => new { x.ChequeReceiptID, date = x.Date.ToString("d"), status = x.ChequeStatus.ToString(), x.Parties.PartyName, x.Amount, x.Particulars, x.ChequeNumber, x.AmountInWords, x.Bank.BankName, dateOfDeposite = x.DateOfDeposite.ToString("d"), dateOfMature = x.DateOfMature.ToString("d"), x.CreatedBy });            

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

//    var chequeReceipt = await _context.ChequeReceipt.SingleOrDefaultAsync(m => m.ChequeReceiptID == id);
//    if (chequeReceipt == null)
//    {
//        return NotFound();
//    }

//    return View(chequeReceipt);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var chequeReceipt = await _context.ChequeReceipt.SingleOrDefaultAsync(m => m.ChequeReceiptID == id);
//    _context.ChequeReceipt.Remove(chequeReceipt);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}