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
    public class ChequePaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChequePaymentsController(ApplicationDbContext context)
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

            var chequePayment = await _context.ChequePayment.SingleOrDefaultAsync(m => m.ChequePaymentID == id);
            if (chequePayment == null)
            {
                return NotFound();
            }

            return View(chequePayment);
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
        public async Task<IActionResult> Create([Bind("ChequePaymentID,Amount,ChequeStatus,AmountInWords,BankID,ChequeNumber,CompanyID,CreatedBy,CreationDate,CurrencyID,CurrentStatus,Date,DateOfDeposite,DateOfMature,ExternalRef,InNameOf,Particulars,PartiesID,ImportExportID")] ChequePayment chequePayment)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            chequePayment.CompanyID = CompID;
            chequePayment.CreatedBy = User.Identity.Name;
            chequePayment.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(chequePayment);
                await _context.SaveChangesAsync();
                int CIB = chequePayment.ChequePaymentID;

                if (chequePayment.ChequeStatus == Purchase.Models.ChequeStatus.Cleared)
                {
                    CashFlow CF2 = new CashFlow();
                    CF2.Debit = chequePayment.Amount;
                    CF2.ChequePaymentID = CIB;
                    CF2.CompanyID = CompID;
                    CF2.PartiesID = chequePayment.PartiesID;
                    // Party Debit
                    int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == chequePayment.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF2.MainAccountID = PartMainAcc1_2;
                    CF2.SubAccountID = PartSubAcc1_2;
                    CF2.TransactionAccountID = (int)PartyTransID;
                    CF2.VoucherType = "Cheque Payment Voucher";
                    CF2.DateCreation = chequePayment.DateOfMature;

                    _context.CashFlow.Add(CF2);
                    await _context.SaveChangesAsync();

                    CashFlow CF3 = new CashFlow();
                    CF3.Credit = chequePayment.Amount;
                    CF3.ChequePaymentID = CIB;
                    CF3.CompanyID = CompID;
                    CF3.PartiesID = chequePayment.PartiesID;
                    // Bank Credit
                    int? bankID = chequePayment.BankID;
                    int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                    int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                    int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                    CF3.MainAccountID = MainAccID_3;
                    CF3.SubAccountID = SubAccID_3;
                    CF3.TransactionAccountID = transBankID;
                    CF3.VoucherType = "Cheque Payment Voucher";
                    CF3.DateCreation = chequePayment.DateOfMature;

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
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", chequePayment.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", chequePayment.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", chequePayment.PartiesID);
            return View(chequePayment);
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

            var chequePayment = await _context.ChequePayment.SingleOrDefaultAsync(m => m.ChequePaymentID == id);
            if (chequePayment == null)
            {
                return NotFound();
            }
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName", chequePayment.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", chequePayment.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", chequePayment.PartiesID);
            return View(chequePayment);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit([Bind("ChequePaymentID,ChequeStatus,Amount,AmountInWords,BankID,ChequeNumber,CompanyID,CreatedBy,CreationDate,CurrencyID,CurrentStatus,Date,DateOfDeposite,DateOfMature,ExternalRef,InNameOf,Particulars,PartiesID,ImportExportID")] ChequePayment chequePayment)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int CIB = chequePayment.ChequePaymentID;
            chequePayment.CompanyID = CompID;
            chequePayment.CreatedBy = User.Identity.Name;
            chequePayment.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chequePayment);
                    await _context.SaveChangesAsync();

                    // Cash Flow VOUCHER EDIT                    

                    _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.ChequePaymentID == CIB));
                    _context.SaveChanges();

                    if (chequePayment.ChequeStatus == Purchase.Models.ChequeStatus.Cleared)
                    {
                        CashFlow CF2 = new CashFlow();
                        CF2.Debit = chequePayment.Amount;
                        CF2.ChequePaymentID = CIB;
                        CF2.CompanyID = CompID;
                        CF2.PartiesID = chequePayment.PartiesID;
                        // Party Credit
                        int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == chequePayment.PartiesID).FirstOrDefault().TransactionAccountID;
                        int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                        int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                        CF2.MainAccountID = PartMainAcc1_2;
                        CF2.SubAccountID = PartSubAcc1_2;
                        CF2.TransactionAccountID = (int)PartyTransID;
                        CF2.VoucherType = "Cheque Payment Voucher";
                        CF2.DateCreation = chequePayment.DateOfMature;

                        _context.CashFlow.Add(CF2);
                        await _context.SaveChangesAsync();

                        CashFlow CF3 = new CashFlow();
                        CF3.Credit = chequePayment.Amount;
                        CF3.ChequePaymentID = CIB;
                        CF3.CompanyID = CompID;
                        CF3.PartiesID = chequePayment.PartiesID;
                        // Bank Debit
                        int? bankID = chequePayment.BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF3.MainAccountID = MainAccID_3;
                        CF3.SubAccountID = SubAccID_3;
                        CF3.TransactionAccountID = transBankID;
                        CF3.VoucherType = "Cheque Payment Voucher";
                        CF3.DateCreation = chequePayment.DateOfMature;

                        _context.CashFlow.Add(CF3);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChequePaymentExists(chequePayment.ChequePaymentID))
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
            ViewData["BankID"] = new SelectList(_context.Bank, "BankID", "BankName", chequePayment.BankID);
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", chequePayment.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", chequePayment.PartiesID);
            return View(chequePayment);
        }

        private bool ChequePaymentExists(int id)
        {
            return _context.ChequePayment.Any(e => e.ChequePaymentID == id);
        }


        [Route("[action]")]
        public JsonResult LoadChequePayments()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var data = _context.ChequePayment.Where(r => r.CompanyID == CompID).Include(r => r.Parties).Include(r=>r.Bank).Select(x => new { x.ChequePaymentID, date = x.Date.ToString("d"), x.Parties.PartyName, x.Amount, x.Particulars, x.ChequeNumber, x.AmountInWords ,x.Bank.BankName , dateOfDeposite = x.DateOfDeposite.ToString("d") ,status=x.ChequeStatus.ToString(), dateOfMature = x.DateOfMature.ToString("d"), x.ImportExportID ,x.CreatedBy });

            return Json(new { data = data });
        }
    }
}
 