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
using System.Collections.Generic;

namespace Invento.Areas.Payment.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Payment")]
    [Route("Payment/[controller]")]
    public class VouchersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VouchersController(ApplicationDbContext context)
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
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            VoucherVM model = new VoucherVM();

            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Voucher.SingleOrDefaultAsync(m => m.VoucherID == id);            
            if (voucher == null)
            {
                return NotFound();
            }
            model.VIList = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.VoucherID == id).Include(r=>r.TransactionAccount).Include(r=>r.SubAccount).Include(r=>r.MainAccount).ToList();
            model.CreatedBy = voucher.CreatedBy;
            model.Date = voucher.Date;
            model.ExternalRef = voucher.ExternalRef;
            model.Particulars = voucher.Particulars;
            model.VoucherID = voucher.VoucherID;

            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Vouchers")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            ViewData["Ids"] = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Select(m => m.TransactionAccountID).ToList();
            ViewData["Name"] = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Select(m => m.AccountName).ToList();
            ViewData["TransactionAccountID"] = new SelectList(_context.TransactionAccount.Where(r => r.CompanyID == CompID), "TransactionAccountID", "AccountName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Vouchers")]
        public async Task<IActionResult> Create(VoucherVM voucher)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            //if (ModelState.IsValid)
            //{
                Voucher voucherMain = new Voucher();
                    voucherMain.CompanyID = CompID;
                    voucherMain.CreatedBy = User.Identity.Name;
                    voucherMain.CreationDate = DateTime.Now;
                    voucherMain.CurrencyID = voucher.CurrencyID;
                    voucherMain.Date = voucher.Date;
                    voucherMain.ExternalRef = voucher.ExternalRef;
                    voucherMain.Particulars = voucher.Particulars;
                    voucherMain.ImportExportID = voucher.ImportExportID;

            _context.Voucher.Add(voucherMain);
                await _context.SaveChangesAsync();
                int voucherMainID = voucherMain.VoucherID;

                var voucherItemList = voucher.VIList;
                voucherItemList.RemoveAll(x => x.TransactionAccountID == 000000);
                voucherItemList.ForEach(x => x.CompanyID = CompID);

                foreach (var item in voucherItemList)
                {
                    VoucherItems voucherItemsMODEL = new VoucherItems();
                    CashFlow CF = new CashFlow();

                    voucherItemsMODEL.CompanyID = CompID;
                    CF.CompanyID = CompID;
                    voucherItemsMODEL.Credit = item.Credit;
                    if(item.Credit == 0)
                    {
                        CF.Debit = item.Debit;
                    }
                    else
                    {
                        CF.Credit = item.Credit;
                    }
                    voucherItemsMODEL.Debit = item.Debit;
                    voucherItemsMODEL.Narration = item.Narration;
                    voucherItemsMODEL.TransactionAccountID = item.TransactionAccountID;
                    CF.TransactionAccountID = item.TransactionAccountID;
                    int subAccId = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == item.TransactionAccountID).FirstOrDefault().SubAccountID;
                    voucherItemsMODEL.SubAccountID = subAccId;
                    CF.SubAccountID = subAccId;
                    int mainAccId = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccId).FirstOrDefault().MainAccountID;
                    voucherItemsMODEL.MainAccountID = mainAccId;
                    CF.MainAccountID = mainAccId;
                    voucherItemsMODEL.VoucherID = voucherMainID;
                        _context.VoucherItems.Add(voucherItemsMODEL);
                        await _context.SaveChangesAsync();
                        int VIid = voucherItemsMODEL.VoucherItemsID;

                    CF.VoucherItemsID = VIid;
                    CF.DateCreation = DateTime.Now.Date;
                    CF.VoucherType = "Voucher";

                    _context.CashFlow.Add(CF);
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
            //}

            //ViewData["Ids"] = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Select(m => m.TransactionAccountID).ToList();
            //ViewData["Name"] = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Select(m => m.AccountName).ToList();
            //ViewData["TransactionAccountID"] = new SelectList(_context.TransactionAccount.Where(r => r.CompanyID == CompID), "TransactionAccountID", "AccountName");
            //ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");

            //return View(voucher);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            VoucherVM model = new VoucherVM();

            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Voucher.SingleOrDefaultAsync(m => m.VoucherID == id);
            if (voucher == null)
            {
                return NotFound();
            }
            model.VIList = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.VoucherID == id).Include(r => r.TransactionAccount).Include(r => r.SubAccount).Include(r => r.MainAccount).ToList();
            model.CreatedBy = voucher.CreatedBy;
            model.Date = voucher.Date;
            model.ExternalRef = voucher.ExternalRef;
            model.Particulars = voucher.Particulars;
            model.VoucherID = voucher.VoucherID;
            model.ImportExportID = voucher.ImportExportID;

            ViewData["Ids"] = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Select(m => m.TransactionAccountID).ToList();
            ViewData["Name"] = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Select(m => m.AccountName).ToList();
            ViewData["TransactionAccountID"] = new SelectList(_context.TransactionAccount.Where(r => r.CompanyID == CompID), "TransactionAccountID", "AccountName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", voucher.CurrencyID);

            return View(model);            
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Vouchers")]
        public async Task<IActionResult> Edit(VoucherVM voucher)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
           
            int voucherMainID = voucher.VoucherID;
          
            Voucher voucherMain = new Voucher();
            voucherMain.VoucherID= voucherMainID;
            voucherMain.Date = voucher.Date;
            voucherMain.Particulars = voucher.Particulars;
            voucherMain.CurrencyID = voucher.CurrencyID;
            voucherMain.ExternalRef = voucher.ExternalRef;
            voucherMain.CompanyID = CompID;
            voucherMain.CreationDate = DateTime.Now;
            voucherMain.CreatedBy = User.Identity.Name;
            voucherMain.ImportExportID = voucher.ImportExportID;

            var voucherItemList = voucher.VIList;
            voucherItemList.RemoveAll(x => x.TransactionAccountID == 000000);
            voucherItemList.ForEach(x => x.CompanyID = CompID);

            List<VoucherItems> listVItems = new List<VoucherItems>();

            listVItems = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.VoucherID == voucherMainID).ToList();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voucherMain);
                    _context.SaveChanges();

                    //RemoveRange(_context.VoucherItems.Where(x => x.VoucherID == voucherMainID))
                    //var vocITEM = _context.VoucherItems.Where(x => x.CompanyID == CompID).Where(x => x.VoucherID == voucherMainID);
                    //_context.SaveChanges();

                    //for(int i =0; i<aa.Count();i++)
                    //{
                    //var vocITEM = _context.VoucherItems.FirstOrDefault(m => m.VoucherID == voucherMainID);
                    //    _context.VoucherItems.Remove(vocITEM);
                    //await _context.SaveChangesAsync();
                    //}

                    // Cash Flow VOUCHER EDIT                    


                    foreach (var item in listVItems)
                    {
                        if(_context.CashFlow.Where(m => m.VoucherItemsID == item.VoucherItemsID).Any() == true)
                        {
                            var cashflow = await _context.CashFlow.SingleOrDefaultAsync(m => m.VoucherItemsID == item.VoucherItemsID);
                            _context.CashFlow.Remove(cashflow);
                            _context.SaveChanges();
                        }                        
                    }

                    //foreach (var item in listVItems)
                    //{
                    //        var vItems = await _context.VoucherItems.SingleOrDefaultAsync(m => m.VoucherItemsID == voucherMainID);
                    //        _context.VoucherItems.Remove(vItems);
                    //        _context.SaveChanges();                     
                    //}

                    _context.VoucherItems.RemoveRange(_context.VoucherItems.Where(x => x.VoucherID == voucherMainID));
                    _context.SaveChanges();


                    foreach (var item in voucherItemList)
                    {
                        VoucherItems voucherItemsMODEL = new VoucherItems();
                        CashFlow CF = new CashFlow();

                        voucherItemsMODEL.CompanyID = CompID;
                        CF.CompanyID = CompID;
                        voucherItemsMODEL.Credit = item.Credit;
                        if (item.Credit == 0)
                        {
                            CF.Debit = item.Debit;
                        }
                        else
                        {
                            CF.Credit = item.Credit;
                        }
                        voucherItemsMODEL.Debit = item.Debit;
                        voucherItemsMODEL.Narration = item.Narration;
                        voucherItemsMODEL.TransactionAccountID = item.TransactionAccountID;
                        CF.TransactionAccountID = item.TransactionAccountID;
                        int subAccId = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == item.TransactionAccountID).FirstOrDefault().SubAccountID;
                        voucherItemsMODEL.SubAccountID = subAccId;
                        CF.SubAccountID = subAccId;
                        int mainAccId = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccId).FirstOrDefault().MainAccountID;
                        voucherItemsMODEL.MainAccountID = mainAccId;
                        CF.MainAccountID = mainAccId;
                        voucherItemsMODEL.VoucherID = voucherMainID;
                        _context.VoucherItems.Add(voucherItemsMODEL);
                        await _context.SaveChangesAsync();
                        int VIid = voucherItemsMODEL.VoucherItemsID;

                        CF.VoucherItemsID = VIid;
                        CF.DateCreation = DateTime.Now.Date;
                        CF.VoucherType = "Voucher";

                        _context.CashFlow.Add(CF);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.VoucherID))
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
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", voucher.CurrencyID);
            return View(voucher);
        }

        [Route("[action]")]
        public JsonResult LoadVouchers()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.Voucher.Where(r => r.CompanyID == CompID).Select(x => new { x.VoucherID ,date = x.Date.ToString("d"), x.Particulars, x.ImportExportID, x.CreatedBy });
            return Json(new { data = data });
        }

        private bool VoucherExists(int id)
        {
            return _context.Voucher.Any(e => e.VoucherID == id);
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

//    var voucher = await _context.Voucher.SingleOrDefaultAsync(m => m.VoucherID == id);
//    if (voucher == null)
//    {
//        return NotFound();
//    }

//    return View(voucher);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var voucher = await _context.Voucher.SingleOrDefaultAsync(m => m.VoucherID == id);
//    _context.Voucher.Remove(voucher);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}