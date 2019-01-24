using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Invento.Areas.SiteAdmin.Models;
using Invento.Areas.Purchase.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Finance.Models;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Invento.Areas.CompanyAdmin.Models.Company;
using Microsoft.AspNetCore.Http;

namespace Invento.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles = "SiteAdmin")]
    [Area("SiteAdmin")]
    [Route("SiteAdmin/[controller]")]
    public class SiteManageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteManageController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }


        [Route("[action]")]
        public async Task<IActionResult> NoOfUsers(int id)
        {
            if (_context.CompanyProfile.Where(r => r.CompanyID == id).Any() == true)
            {
                int CurrentCompProfileId = _context.CompanyProfile.Where(r => r.CompanyID == id).FirstOrDefault().CompanyProfileID;
                if (CurrentCompProfileId == 0)
                {
                    return NotFound();
                }
                var companyProfile = await _context.CompanyProfile.SingleOrDefaultAsync(m => m.CompanyProfileID == CurrentCompProfileId);
                if (companyProfile == null)
                {
                    return NotFound();
                }
                return View(companyProfile);
            }
            return RedirectToAction("Index", "Home", new { area = "SiteAdmin" });
        }        
        
        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NoOfUsers([Bind("NoOfCompanyUsersAllowed,CompanyID,CompanyProfileID,Address,AlternativeContact,City,CompanyName,CompanyWebsite,ContactNumber,CountryID,Fax,OfficeContact,FileData,FileName")] CompanyProfile companyProfile, IFormFile File)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    byte[] data;
                    if (File != null)
                    {
                        using (var stream = File.OpenReadStream())
                        {
                            data = new byte[stream.Length];
                            stream.Read(data, 0, (int)stream.Length);
                        }
                        companyProfile.FileData = data;
                        companyProfile.FileName = File.FileName;
                    }
                    else if (File == null)
                    {
                        companyProfile.FileData = companyProfile.FileData;
                        companyProfile.FileName = companyProfile.FileName;
                    }
                    _context.Update(companyProfile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyProfileExists(companyProfile.CompanyProfileID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home", new { area = "SiteAdmin" });
            }
            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == companyProfile.CompanyID), "CountryID", "Name", companyProfile.CountryID);
            return View(companyProfile);            
        }
        private bool CompanyProfileExists(int id)
        {
            return _context.CompanyProfile.Any(e => e.CompanyProfileID == id);
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult DeleteAllCompanyData()
        {
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAllCompanyData([Bind("CompanyID")] SiteManageVM siteManageVM)
        {            
            int CompanyID = siteManageVM.CompanyID;
            if (ModelState.IsValid)
            {
                _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.CompanyID == CompanyID));
                
                var purchaseReturn = _context.PurchaseReturn
                                  .Where(q => q.CompanyID == CompanyID)
                                  .Include(q => q.PurchaseReturnTransaction);
                _context.PurchaseReturn.RemoveRange(purchaseReturn);
                
                var salReturn = _context.SaleReturn
                                .Where(q => q.CompanyID == CompanyID)
                                .Include(q => q.SaleReturnTransaction);
                _context.SaleReturn.RemoveRange(salReturn);
                
                var salBill = _context.SaleBill
                                  .Where(q => q.CompanyID == CompanyID)
                                  .Include(q => q.SaleTransaction)
                                  .Include(q => q.SaleBillItem);
                _context.SaleBill.RemoveRange(salBill);

                var purBill = _context.PurchaseBill
                                  .Where(q => q.CompanyID == CompanyID)
                                  .Include(q => q.Transaction)
                                  .Include(q => q.PurchaseBillItem);
                _context.PurchaseBill.RemoveRange(purBill);

                var voucher = _context.Voucher
                                  .Where(q => q.CompanyID == CompanyID)
                                  .Include(q => q.VoucherItems);
                _context.Voucher.RemoveRange(voucher);
                
                var grn = _context.GRN
                                  .Where(q => q.CompanyID == CompanyID)
                                  .Include(q => q.GRNItem);
                _context.GRN.RemoveRange(grn);
                
                _context.CashInBank.RemoveRange(_context.CashInBank.Where(x => x.CompanyID == CompanyID));
                _context.CashPayment.RemoveRange(_context.CashPayment.Where(x => x.CompanyID == CompanyID));
                _context.CashReceipt.RemoveRange(_context.CashReceipt.Where(x => x.CompanyID == CompanyID));
                _context.ChequePayment.RemoveRange(_context.ChequePayment.Where(x => x.CompanyID == CompanyID));
                _context.ChequeReceipt.RemoveRange(_context.ChequeReceipt.Where(x => x.CompanyID == CompanyID));

                _context.Item.RemoveRange(_context.Item.Where(x => x.CompanyID == CompanyID));
                _context.Parties.RemoveRange(_context.Parties.Where(x => x.CompanyID == CompanyID));
                _context.CompanyProfile.RemoveRange(_context.CompanyProfile.Where(x => x.CompanyID == CompanyID));
                
                _context.Country.RemoveRange(_context.Country.Where(x => x.CompanyID == CompanyID));
                _context.Currency.RemoveRange(_context.Currency.Where(x => x.CompanyID == CompanyID));
                _context.Bank.RemoveRange(_context.Bank.Where(x => x.CompanyID == CompanyID));
                _context.ProductGroup.RemoveRange(_context.ProductGroup.Where(x => x.CompanyID == CompanyID));

                _context.TransactionAccount.RemoveRange(_context.TransactionAccount.Where(x => x.CompanyID == CompanyID));
                _context.SubAccount.RemoveRange(_context.SubAccount.Where(x => x.CompanyID == CompanyID));
                _context.MainAccount.RemoveRange(_context.MainAccount.Where(x => x.CompanyID == CompanyID));
               
                _context.SaveChanges();
                
                // Insert statements on New user creation *** STATEMENT OF ACCOUNTS ***

                DateTime CurrentDate = DateTime.Now;
                string CurrentUser = User.Identity.Name;

                MainAccount mainParties = new MainAccount();
                mainParties.AccountName = "Parties";
                mainParties.MainAccountNumber = "00";
                mainParties.CreationDate = CurrentDate;
                mainParties.CreatedBy = CurrentUser;
                mainParties.CompanyID = CompanyID;
                _context.Add(mainParties);

                MainAccount mainAssets = new MainAccount();
                mainAssets.AccountName = "Assets";
                mainAssets.MainAccountNumber = "01";
                mainAssets.CreationDate = CurrentDate;
                mainAssets.CreatedBy = CurrentUser;
                mainAssets.CompanyID = CompanyID;
                _context.Add(mainAssets);

                MainAccount mainLiabilities = new MainAccount();
                mainLiabilities.AccountName = "Liabilities";
                mainLiabilities.MainAccountNumber = "02";
                mainLiabilities.CreationDate = CurrentDate;
                mainLiabilities.CreatedBy = CurrentUser;
                mainLiabilities.CompanyID = CompanyID;
                _context.Add(mainLiabilities);

                MainAccount mainIncome = new MainAccount();
                mainIncome.AccountName = "Income";
                mainIncome.MainAccountNumber = "03";
                mainIncome.CreationDate = CurrentDate;
                mainIncome.CreatedBy = CurrentUser;
                mainIncome.CompanyID = CompanyID;
                _context.Add(mainIncome);

                MainAccount mainExpenses = new MainAccount();
                mainExpenses.AccountName = "Expenses";
                mainExpenses.MainAccountNumber = "04";
                mainExpenses.CreationDate = CurrentDate;
                mainExpenses.CreatedBy = CurrentUser;
                mainExpenses.CompanyID = CompanyID;
                _context.Add(mainExpenses);

                _context.SaveChanges();

                int mainPartiesID = mainParties.MainAccountID;
                int mainAssetsID = mainAssets.MainAccountID;
                int mainLiabilitiesID = mainLiabilities.MainAccountID;
                int mainIncomeID = mainIncome.MainAccountID;
                int mainExpensesID = mainExpenses.MainAccountID;

                SubAccount subParties = new SubAccount();
                subParties.AccountName = "Parties";
                subParties.SubAccountNumber = "0000";
                subParties.CreationDate = CurrentDate;
                subParties.CreatedBy = CurrentUser;
                subParties.CompanyID = CompanyID;
                subParties.MainAccountID = mainPartiesID;
                _context.Add(subParties);

                SubAccount subPurchase = new SubAccount();
                subPurchase.AccountName = "Purchase";
                subPurchase.SubAccountNumber = "0001";
                subPurchase.CreationDate = CurrentDate;
                subPurchase.CreatedBy = CurrentUser;
                subPurchase.CompanyID = CompanyID;
                subPurchase.MainAccountID = mainExpensesID;
                _context.Add(subPurchase);

                SubAccount subSaleReturn = new SubAccount();
                subSaleReturn.AccountName = "Sale Return";
                subSaleReturn.SubAccountNumber = "0002";
                subSaleReturn.CreationDate = CurrentDate;
                subSaleReturn.CreatedBy = CurrentUser;
                subSaleReturn.CompanyID = CompanyID;
                subSaleReturn.MainAccountID = mainExpensesID;
                _context.Add(subSaleReturn);

                SubAccount subCash = new SubAccount();
                subCash.AccountName = "Cash";
                subCash.SubAccountNumber = "0001";
                subCash.CreationDate = CurrentDate;
                subCash.CreatedBy = CurrentUser;
                subCash.CompanyID = CompanyID;
                subCash.MainAccountID = mainAssetsID;
                _context.Add(subCash);

                SubAccount subBank = new SubAccount();
                subBank.AccountName = "Bank";
                subBank.SubAccountNumber = "0002";
                subBank.CreationDate = CurrentDate;
                subBank.CreatedBy = CurrentUser;
                subBank.CompanyID = CompanyID;
                subBank.MainAccountID = mainAssetsID;
                _context.Add(subBank);

                SubAccount subSale = new SubAccount();
                subSale.AccountName = "Sale";
                subSale.SubAccountNumber = "0001";
                subSale.CreationDate = CurrentDate;
                subSale.CreatedBy = CurrentUser;
                subSale.CompanyID = CompanyID;
                subSale.MainAccountID = mainIncomeID;
                _context.Add(subSale);

                SubAccount subPurchaseReturn = new SubAccount();
                subPurchaseReturn.AccountName = "Purchase Return";
                subPurchaseReturn.SubAccountNumber = "0002";
                subPurchaseReturn.CreationDate = CurrentDate;
                subPurchaseReturn.CreatedBy = CurrentUser;
                subPurchaseReturn.CompanyID = CompanyID;
                subPurchaseReturn.MainAccountID = mainIncomeID;
                _context.Add(subPurchaseReturn);

                _context.SaveChanges();

                int subPurchaseID = subPurchase.SubAccountID;
                int subSaleReturnID = subSaleReturn.SubAccountID;
                int subCashID = subCash.SubAccountID;
                int subSaleID = subSale.SubAccountID;
                int subPurchaseReturnID = subPurchaseReturn.SubAccountID;

                TransactionAccount transPurchaseCash = new TransactionAccount();
                transPurchaseCash.AccountName = "Cash Purchase";
                transPurchaseCash.TransactionAccountNumber = "0001";
                transPurchaseCash.OpeningBalance = 0;
                transPurchaseCash.CreationDate = CurrentDate;
                transPurchaseCash.CreatedBy = CurrentUser;
                transPurchaseCash.CompanyID = CompanyID;
                transPurchaseCash.SubAccountID = subPurchaseID;
                _context.Add(transPurchaseCash);

                TransactionAccount transPurchaseCredit = new TransactionAccount();
                transPurchaseCredit.AccountName = "Credit Purchase";
                transPurchaseCredit.TransactionAccountNumber = "0002";
                transPurchaseCredit.OpeningBalance = 0;
                transPurchaseCredit.CreationDate = CurrentDate;
                transPurchaseCredit.CreatedBy = CurrentUser;
                transPurchaseCredit.CompanyID = CompanyID;
                transPurchaseCredit.SubAccountID = subPurchaseID;
                _context.Add(transPurchaseCredit);

                TransactionAccount transSaleReturn = new TransactionAccount();
                transSaleReturn.AccountName = "Sale Return";
                transSaleReturn.TransactionAccountNumber = "0001";
                transSaleReturn.OpeningBalance = 0;
                transSaleReturn.CreationDate = CurrentDate;
                transSaleReturn.CreatedBy = CurrentUser;
                transSaleReturn.CompanyID = CompanyID;
                transSaleReturn.SubAccountID = subSaleReturnID;
                _context.Add(transSaleReturn);

                TransactionAccount transCashInHand = new TransactionAccount();
                transCashInHand.AccountName = "Cash In Hand";
                transCashInHand.TransactionAccountNumber = "0001";
                transCashInHand.OpeningBalance = 0;
                transCashInHand.CreationDate = CurrentDate;
                transCashInHand.CreatedBy = CurrentUser;
                transCashInHand.CompanyID = CompanyID;
                transCashInHand.SubAccountID = subCashID;
                _context.Add(transCashInHand);

                TransactionAccount transCashSale = new TransactionAccount();
                transCashSale.AccountName = "Cash Sale";
                transCashSale.TransactionAccountNumber = "0001";
                transCashSale.OpeningBalance = 0;
                transCashSale.CreationDate = CurrentDate;
                transCashSale.CreatedBy = CurrentUser;
                transCashSale.CompanyID = CompanyID;
                transCashSale.SubAccountID = subSaleID;
                _context.Add(transCashSale);

                TransactionAccount transCreditSale = new TransactionAccount();
                transCreditSale.AccountName = "Credit Sale";
                transCreditSale.TransactionAccountNumber = "0002";
                transCreditSale.OpeningBalance = 0;
                transCreditSale.CreationDate = CurrentDate;
                transCreditSale.CreatedBy = CurrentUser;
                transCreditSale.CompanyID = CompanyID;
                transCreditSale.SubAccountID = subSaleID;
                _context.Add(transCreditSale);

                TransactionAccount transPurchaseReturn = new TransactionAccount();
                transPurchaseReturn.AccountName = "Purchase Return";
                transPurchaseReturn.TransactionAccountNumber = "0001";
                transPurchaseReturn.OpeningBalance = 0;
                transPurchaseReturn.CreationDate = CurrentDate;
                transPurchaseReturn.CreatedBy = CurrentUser;
                transPurchaseReturn.CompanyID = CompanyID;
                transPurchaseReturn.SubAccountID = subPurchaseReturnID;
                _context.Add(transPurchaseReturn);

                _context.SaveChanges();
                // Insert statements on New user creation *** STATEMENT OF ACCOUNTS ***


                return RedirectToAction("Index", "Home", new { area = "SiteAdmin" });                
            }
            return View();
        }
        
    }
}