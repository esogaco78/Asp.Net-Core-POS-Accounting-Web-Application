using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Finance.Models;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Invento.Areas.Finance.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Authorize(Roles = "CompanyAdmin,Chart Of Accounts")]
    [Area("Finance")]
    [Route("Finance/[controller]")]
    public class ChartOfAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChartOfAccountsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        public async Task<IActionResult> OpeningBalance()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return View(await _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r=>r.OpeningBalance != 0).Include(r => r.SubAccount.MainAccount).Include(r=>r.SubAccount).OrderBy(r => r.SubAccount.SubAccountNumber).ToListAsync());
        }

        #region ChartOfAccounts_Index_Functions

        [Route("[action]")]
        public async Task<IActionResult> MainAccounts()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return View(await _context.MainAccount.Where(r=>r.CompanyID == CompID).OrderBy(r=>r.MainAccountNumber).ToListAsync());
        }

        [Route("[action]")]
        public async Task<IActionResult> SubAccounts()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return View(await _context.SubAccount.Where(r => r.CompanyID == CompID).Include(r=>r.MainAccount).OrderBy(r=>r.MainAccount.MainAccountID).ThenBy(r => r.SubAccountNumber).ToListAsync());
        }

        [Route("[action]")]
        public async Task<IActionResult> TransactionAccounts()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return View(await _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r=>r.SubAccount).Include(r=>r.SubAccount.MainAccount).OrderBy(r => r.SubAccount.MainAccount.MainAccountNumber).ThenBy(r=>r.SubAccount.SubAccountNumber).ThenBy(r=>r.TransactionAccountNumber).ToListAsync());
        }

        #endregion


        #region ChartOfAccounts_Create_Functions

        [Route("[action]")]
        public IActionResult CreateMainAccount()
        {
            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMainAccount([Bind("MainAccountID,AccountName,MainAccountNumber")] MainAccount mainAccount)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (_context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == mainAccount.MainAccountNumber).Any() == true)
            {
                ViewData["SameCodeError"] = "Main Account Code is already assigned to account.";
                return View("MainAccounts", await _context.MainAccount.Where(r => r.CompanyID == CompID).OrderBy(r => r.MainAccountNumber).ToListAsync());
            }

            mainAccount.CompanyID = CompID;
            mainAccount.CreationDate = DateTime.Now;
            mainAccount.CreatedBy = User.Identity.Name;           

            if (ModelState.IsValid)
            {
                _context.Add(mainAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("MainAccounts");
            }
            return View(mainAccount);
        }


        [Route("[action]")]
        public IActionResult CreateSubAccount()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
                        
            var mainAccList = new List<MainAccount>();
            mainAccList = _context.MainAccount.Where(r => r.CompanyID == CompID).ToList();
            mainAccList.RemoveAll(r => r.MainAccountNumber == "00");

            ViewData["MainAccount"] = new SelectList(mainAccList, "MainAccountID", "AccountName");
            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubAccount(SubAccount subAccount)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (_context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == subAccount.MainAccountID).Where(r => r.SubAccountNumber == subAccount.SubAccountNumber).Any() == true)
            {
                ViewData["SameCodeError"] = "Sub Account Code is already assigned to account.";
                return View("SubAccounts", await _context.SubAccount.Where(r => r.CompanyID == CompID).Include(r => r.MainAccount).OrderBy(r => r.MainAccount.MainAccountNumber).ToListAsync());
            }

            subAccount.CompanyID = CompID;
            subAccount.CreationDate = DateTime.Now;
            subAccount.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                _context.Add(subAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("SubAccounts");
            }

            var mainAccList = new List<MainAccount>();
            mainAccList = _context.MainAccount.Where(r => r.CompanyID == CompID).ToList();
            mainAccList.RemoveAll(r => r.MainAccountNumber == "00");

            ViewData["MainAccount"] = new SelectList(mainAccList, "MainAccountID", "AccountName");            
            return View(subAccount);
        }
        
        [Route("[action]")]
        public IActionResult CreateTransactionAccount()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            var mainAccList = new List<SubAccount>();
            mainAccList = _context.SubAccount.Where(r => r.CompanyID == CompID).ToList();
            mainAccList.RemoveAll(r => r.SubAccountNumber == "0000");

            int mainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
            int subAccID = _context.SubAccount.Where(r => r.SubAccountNumber == "0002").Where(r => r.MainAccountID == mainAccID).FirstOrDefault().SubAccountID;        
            mainAccList.RemoveAll(r => r.SubAccountID == subAccID);

            ViewData["SubAccount"] = new SelectList(mainAccList, "SubAccountID", "AccountName");            

            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransactionAccount(TransactionAccount transactionAccount)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            int SubAcc = transactionAccount.SubAccountID;
            
            if (_context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == transactionAccount.SubAccountID).Where(r => r.TransactionAccountNumber == transactionAccount.TransactionAccountNumber).Any() == true)
            {
                ViewData["SameCodeError"] = "Transaction Account Code is already assigned to account.";
                return View("TransactionAccounts", await _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r => r.SubAccount).Include(r => r.SubAccount.MainAccount).OrderBy(r => r.SubAccount.MainAccountID).ToListAsync());
            }

            transactionAccount.CompanyID = CompID;
            transactionAccount.CreationDate = DateTime.Now;
            transactionAccount.CreatedBy = User.Identity.Name;
            
            if (ModelState.IsValid)
            {
                _context.Add(transactionAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("TransactionAccounts");
            }

            var mainAccList = new List<SubAccount>();
            mainAccList = _context.SubAccount.Where(r => r.CompanyID == CompID).ToList();
            mainAccList.RemoveAll(r => r.SubAccountNumber == "0000");

            int mainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
            int subAccID = _context.SubAccount.Where(r => r.SubAccountNumber == "0002").Where(r => r.MainAccountID == mainAccID).FirstOrDefault().SubAccountID;
            mainAccList.RemoveAll(r => r.SubAccountID == subAccID);

            ViewData["SubAccount"] = new SelectList(mainAccList, "SubAccountID", "AccountName");
            return View(transactionAccount);
        }

        #endregion

        #region GenreateNumber

        [Route("[action]")]
        [Authorize]
        public JsonResult GenerateSubAccountNumber(int mainAccNum)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            string a = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r=>r.MainAccountID == mainAccNum).Select(x=>x.SubAccountNumber).Max();

            string subNumber;

            int Number = Convert.ToInt32(a);
            Number = Number + 1;

            if (Number <= 9)
            {
                subNumber = "000" + Number;
            }
            else if(Number <= 99)
            {
                subNumber = "00" + Number;
            }
            else if(Number <= 999)
            {
                subNumber = "0" + Number; 
            }
            else
            {
                subNumber = Number.ToString();
            }
            return Json(subNumber);
        }


        [Route("[action]")]
        [Authorize]
        public JsonResult GenerateTranAccountNumber(int subAccNum)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            string a = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccNum).Select(x => x.TransactionAccountNumber).Max();

            string tranNumber;

            int Number = Convert.ToInt32(a);
            Number = Number + 1;

            if (Number <= 9)
            {
                tranNumber = "000" + Number;
            }
            else if (Number <= 99)
            {
                tranNumber = "00" + Number;
            }
            else if (Number <= 999)
            {
                tranNumber = "0" + Number;
            }
            else
            {
                tranNumber = Number.ToString();
            }
            return Json(tranNumber);
        }


        #endregion


        #region ChartOfAccounts_Edit_Functions

        [Route("[action]")]
        public async Task<IActionResult> EditMainAccount(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var mainAccount = await _context.MainAccount.Where(r=>r.CompanyID == CompID).SingleOrDefaultAsync(m => m.MainAccountID == id);
            if (mainAccount == null)
            {
                return NotFound();
            }
            return PartialView(mainAccount);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMainAccount([Bind("MainAccountID,AccountName,MainAccountNumber")] MainAccount mainAccount)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            //if (_context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == mainAccount.MainAccountNumber).Any() == true)
            //{
            //    ViewData["SameCodeError"] = "Main Account Code is already assigned to account.";
            //    return View("MainAccounts", await _context.MainAccount.Where(r => r.CompanyID == CompID).OrderBy(r => r.MainAccountNumber).ToListAsync());
            //}

            mainAccount.CompanyID = CompID;
            mainAccount.CreationDate = DateTime.Now;
            mainAccount.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mainAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainAccountExists(mainAccount.MainAccountID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("MainAccounts");
            }
            return View(mainAccount);
        }


        [Route("[action]")]
        public async Task<IActionResult> EditSubAccount(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var subAccount = await _context.SubAccount.Where(r => r.CompanyID == CompID).SingleOrDefaultAsync(m => m.SubAccountID == id);

            var mainAccList = new List<MainAccount>();
            mainAccList = _context.MainAccount.Where(r => r.CompanyID == CompID).ToList();
            //mainAccList.RemoveAll(r => r.MainAccountNumber == "00");

            ViewData["MainAccount"] = new SelectList(mainAccList, "MainAccountID", "AccountName");

            if (subAccount == null)
            {
                return NotFound();
            }
            return PartialView(subAccount);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubAccount(SubAccount subAccount)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            //if (_context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == subAccount.MainAccountID).Where(r => r.SubAccountNumber == subAccount.SubAccountNumber).Any() == true)
            //{
            //    ViewData["SameCodeError"] = "Sub Account Code is already assigned to account.";
            //    return View("SubAccounts", await _context.SubAccount.Where(r => r.CompanyID == CompID).Include(r => r.MainAccount).OrderBy(r => r.MainAccount.MainAccountNumber).ToListAsync());
            //}

            subAccount.CompanyID = CompID;
            subAccount.CreationDate = DateTime.Now;
            subAccount.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainAccountExists(subAccount.SubAccountID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("SubAccounts");
            }
            var mainAccList = new List<MainAccount>();
            mainAccList = _context.MainAccount.Where(r => r.CompanyID == CompID).ToList();
            mainAccList.RemoveAll(r => r.MainAccountNumber == "00");

            ViewData["MainAccount"] = new SelectList(mainAccList, "MainAccountID", "AccountName");
            return View(subAccount);
        }


        [Route("[action]")]
        public async Task<IActionResult> EditTransactionAccount(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var transactionAccount = await _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r=>r.SubAccount.MainAccount).SingleOrDefaultAsync(m => m.TransactionAccountID == id);
            if (transactionAccount == null)
            {
                return NotFound();
            }

            var mainAccList = new List<SubAccount>();
            mainAccList = _context.SubAccount.Where(r => r.CompanyID == CompID).ToList();
            //mainAccList.RemoveAll(r => r.SubAccountNumber == "0000");

            ViewData["SubAccount"] = new SelectList(mainAccList, "SubAccountID", "AccountName");
            
            return PartialView(transactionAccount);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTransactionAccount(TransactionAccount transactionAccount)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            //if (_context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == transactionAccount.SubAccountID).Where(r => r.TransactionAccountNumber == transactionAccount.TransactionAccountNumber).Any() == true)
            //{
            //    ViewData["SameCodeError"] = "Transaction Account Code is already assigned to account.";
            //    return View("TransactionAccounts", await _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r => r.SubAccount).Include(r => r.SubAccount.MainAccount).OrderBy(r => r.SubAccount.MainAccountID).ToListAsync());
            //}

            transactionAccount.CompanyID = CompID;
            transactionAccount.CreationDate = DateTime.Now;
            transactionAccount.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainAccountExists(transactionAccount.TransactionAccountID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("TransactionAccounts");
            }

            var mainAccList = new List<SubAccount>();
            mainAccList = _context.SubAccount.Where(r => r.CompanyID == CompID).ToList();
            mainAccList.RemoveAll(r => r.SubAccountNumber == "0000");

            ViewData["SubAccount"] = new SelectList(mainAccList, "SubAccountID", "AccountName");
            return View(transactionAccount);
        }


        #endregion

        #region Extra

        [Route("[action]")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainAccount = await _context.MainAccount.SingleOrDefaultAsync(m => m.MainAccountID == id);
            if (mainAccount == null)
            {
                return NotFound();
            }

            return View(mainAccount);
        }

        [Route("[action]")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainAccount = await _context.MainAccount.SingleOrDefaultAsync(m => m.MainAccountID == id);
            if (mainAccount == null)
            {
                return NotFound();
            }

            return View(mainAccount);
        }


        [Route("[action]")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mainAccount = await _context.MainAccount.SingleOrDefaultAsync(m => m.MainAccountID == id);
            _context.MainAccount.Remove(mainAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        private bool MainAccountExists(int id)
        {
            return _context.MainAccount.Any(e => e.MainAccountID == id);
        }
        #endregion
    }
}
