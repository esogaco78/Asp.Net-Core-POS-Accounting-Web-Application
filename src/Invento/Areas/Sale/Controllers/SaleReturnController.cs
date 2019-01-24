using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Sale.Models;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Product.Models;
using Invento.Areas.Finance.Models;

namespace Invento.Areas.Sale.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Sale")]
    [Route("Sale/[controller]")]
    public class SaleReturnController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleReturnController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Sale Returns")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Sale Returns")]
        public async Task<IActionResult> Create(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }
            SaleReturnVM model = new SaleReturnVM();
            SaleBill saleBill = new SaleBill();

            saleBill = await _context.SaleBill.Where(r => r.CompanyID == CompID).Include(r=>r.Parties).FirstOrDefaultAsync(m => m.SaleBillID == id);
            if (saleBill == null)
            {
                return NotFound();
            }

            model.SaleBillID = saleBill.SaleBillID;

            model.BillDate_OldBill = saleBill.BillDate.ToString("d");

            model.ContactNumber_OldBill = saleBill.ContactNumber;
            model.ContactPerson_OldBill = saleBill.ContactPerson;
            model.GrossTotal_OldBill = saleBill.GrossTotal;
            model.NetAmount_OldBill = saleBill.NetAmount;

            if (saleBill.PartiesID == null)
            {
                model.PartiesID = null;
                model.SR_S_1 = "Cash Sale";
            }
            else
            {
                model.PartiesID = saleBill.PartiesID;
                model.SR_S_1 = saleBill.Parties.PartyName;
            }
            
            model.Remarks_OldBill = saleBill.Remarks;
            model.TDiscount_OldBill = saleBill.TDiscount;
            model.TotalQuantity_OldBill = saleBill.TotalQuantity;

            model.SaleBillItem_List = _context.SaleBillItem.Include(r => r.Item).Where(r => r.SaleBillID == id).ToList();

            ViewData["Ids"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency, "CurrencyID", "CurrencyName", saleBill.CurrencyID);
            //ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", saleBill.PartiesID);

            ViewData["ItemID"] = new SelectList(_context.Item.Where(r => r.CompanyID == CompID), "ItemID", "ItemID");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");
            ViewData["SaleBillID"] = new SelectList(_context.PurchaseBill.Where(r => r.CompanyID == CompID), "SaleBillID", "SaleBillNo");
            return View(model);
        }


        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Sale Returns")]
        public async Task<IActionResult> Create(SaleReturn saleReturn, IEnumerable<SaleBillItem> SaleBillItem_List, IEnumerable<SaleTransaction> SaleTransactionList)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int salReturnID = 0;

            var SaleBillItem_List_1 = SaleBillItem_List.ToList();
            SaleBillItem_List_1.RemoveAll(x => x.SaleBillExtraInt_2 == 0);

            foreach (var item in SaleBillItem_List_1)
            {
                SaleReturn obj = new SaleReturn();

                obj.CreatedBy = User.Identity.Name;
                obj.CompanyID = CompID;
                obj.SaleBillReturnDate = DateTime.Now;

                obj.SaleBillID = saleReturn.SaleBillID;
                obj.ItemID = item.ItemID;
                
                    obj.PartiesID = saleReturn.PartiesID;
              
                obj.PartiesID = saleReturn.PartiesID;

                obj.OldQuantity = item.Quantity;
                obj.ReturnQuantity = item.SaleBillExtraInt_2;
                obj.AmountToPay = item.SaleBillExtraDecimal_1;

                obj.Remarks = saleReturn.Remarks;

                _context.Add(obj);
                await _context.SaveChangesAsync();

                salReturnID = obj.SaleReturnID;

                // Increasing Item Quantity from ITEM Table
                Item itemObj = new Item();
                decimal StockQuantity, final;

                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                StockQuantity = itemObj.Quantity;
                final = StockQuantity + item.SaleBillExtraInt_2;

                itemObj.Quantity = final;
                _context.Update(itemObj);
                await _context.SaveChangesAsync();
                // Increasing Item Quantity from ITEM Table                
            }


            if (saleReturn.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Debit = saleReturn.AmountToPay;
                CF.SaleReturnID = salReturnID;
                CF.CompanyID = CompID;

                // Sale Return  04-0002-0001
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Sale Return Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Credit = saleReturn.AmountToPay;
                CF1.SaleReturnID = salReturnID;
                CF1.CompanyID = CompID;

                // Cash In Hand 01-0001-0001 credit
                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;
                
                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Sale Return Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }

            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Credit = saleReturn.AmountToPay;
                CF2.SaleReturnID = salReturnID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = saleReturn.PartiesID;
                //
                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == saleReturn.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc1;
                CF2.SubAccountID = PartSubAcc1;
                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.VoucherType = "Sale Return Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = saleReturn.AmountToPay;
                CF3.SaleReturnID = salReturnID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = saleReturn.PartiesID;

                // Sale Return  04-0002-0001
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF3.MainAccountID = MainAccID;
                CF3.SubAccountID = SubAccID;
                CF3.TransactionAccountID = TraAccID;
                CF3.VoucherType = "Sale Return Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();

                if (SaleTransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Debit = SaleTransactionList.FirstOrDefault().Amount;
                    CF4.SaleReturnID = salReturnID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = saleReturn.PartiesID;

                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == saleReturn.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF4.MainAccountID = PartMainAcc1_2;
                    CF4.SubAccountID = PartSubAcc1_2;
                    CF4.TransactionAccountID = (int)PartyTransID_1;
                    CF4.VoucherType = "Sale Bill Voucher";
                    CF4.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF4);
                    await _context.SaveChangesAsync();


                    if (SaleTransactionList.FirstOrDefault().Mode == Models.PaymentMode.Cash) // Mode 0 Means ==> Cash
                    {
                        CashFlow CF5 = new CashFlow();
                        CF5.Credit = SaleTransactionList.FirstOrDefault().Amount;
                        CF5.SaleReturnID = salReturnID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = saleReturn.PartiesID;
                        // Cash In Hand 01-0001-0001
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID = SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Sale Return Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (SaleTransactionList.FirstOrDefault().Mode == Models.PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Credit = SaleTransactionList.FirstOrDefault().Amount;
                        CF6.SaleReturnID = salReturnID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = saleReturn.PartiesID;

                        int? bankID = SaleTransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Sale Return Voucher";
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }

            }


            // Sale Transaction Entry
            var TransactionList_Result = SaleTransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.SaleTransactionID == 777777);
            TransactionList_Result.ForEach(x => x.SaleTransactionID = 0);

            foreach (var item in TransactionList_Result)
            {
                SaleReturnTransaction transaction = new SaleReturnTransaction();
                transaction.CompanyID = CompID;
                transaction.SaleReturnID = salReturnID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.SaleBillID = saleReturn.SaleBillID;

                _context.SaleReturnTransaction.Add(transaction);

                await _context.SaveChangesAsync();

              
            }
                // Party Payments table data insertion  
                  
            return RedirectToAction("Index", "Sale", new { area = "Sale" });
        }
        
        private bool SaleReturnExists(int id)
        {
            return _context.SaleReturn.Any(e => e.SaleReturnID == id);
        }
        
        [Route("[action]")]
        [Authorize]
        public JsonResult LoadSaleReturns()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.SaleReturn.Where(r => r.CompanyID == CompID).Include(r => r.Parties).Select(x => new { x.SaleReturnID, billDate = x.SaleBillReturnDate.ToString("d"), x.Parties.PartyName, x.ContactPerson, x.ReturnQuantity, x.AmountToPay, x.Remarks, x.Item.OEMNo, x.OldQuantity, x.SaleBillID, x.ContactNumber, x.Item.ItemName, x.CreatedBy }).ToList();
            return Json(new { data = data });
        }

    }
}


//[Route("[action]")]
//public async Task<IActionResult> Details(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var saleReturn = await _context.SaleReturn.SingleOrDefaultAsync(m => m.SaleReturnID == id);
//    if (saleReturn == null)
//    {
//        return NotFound();
//    }

//    return View(saleReturn);
//}


//// GET: SaleReturn/Delete/5
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var saleReturn = await _context.SaleReturn.SingleOrDefaultAsync(m => m.SaleReturnID == id);
//    if (saleReturn == null)
//    {
//        return NotFound();
//    }

//    return View(saleReturn);
//}

//// POST: SaleReturn/Delete/5
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var saleReturn = await _context.SaleReturn.SingleOrDefaultAsync(m => m.SaleReturnID == id);
//    _context.SaleReturn.Remove(saleReturn);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}

//public async Task<IActionResult> Edit(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var saleReturn = await _context.SaleReturn.SingleOrDefaultAsync(m => m.SaleReturnID == id);
//    if (saleReturn == null)
//    {
//        return NotFound();
//    }
//    ViewData["ItemID"] = new SelectList(_context.Item, "ItemID", "ItemID", saleReturn.ItemID);
//    ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", saleReturn.PartiesID);
//    ViewData["SaleBillID"] = new SelectList(_context.SaleBill, "SaleBillID", "SaleBillNo", saleReturn.SaleBillID);
//    return View(saleReturn);
//}

//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> Edit(int id, [Bind("SaleReturnID,AmountToPay,CompanyID,ContactNumber,ContactPerson,CreatedBy,ItemID,OldQuantity,PartiesID,Remarks,ReturnQuantity,SR_B_1,SR_D_1,SR_D_2,SR_I_1,SR_I_2,SR_S_1,SR_S_2,SaleBillID,SaleBillReturnDate,TotalAmount")] SaleReturn saleReturn)
//{
//    if (id != saleReturn.SaleReturnID)
//    {
//        return NotFound();
//    }

//    if (ModelState.IsValid)
//    {
//        try
//        {
//            _context.Update(saleReturn);
//            await _context.SaveChangesAsync();
//        }
//        catch (DbUpdateConcurrencyException)
//        {
//            if (!SaleReturnExists(saleReturn.SaleReturnID))
//            {
//                return NotFound();
//            }
//            else
//            {
//                throw;
//            }
//        }
//        return RedirectToAction("Index");
//    }
//    ViewData["ItemID"] = new SelectList(_context.Item, "ItemID", "ItemID", saleReturn.ItemID);
//    ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", saleReturn.PartiesID);
//    ViewData["SaleBillID"] = new SelectList(_context.SaleBill, "SaleBillID", "SaleBillNo", saleReturn.SaleBillID);
//    return View(saleReturn);
//}