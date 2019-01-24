using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Purchase.Models;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Product.Models;
using Invento.Areas.Finance.Models;

namespace Invento.Areas.Purchase.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Purchase")]
    [Route("Purchase/[controller]")]
    public class PurchaseReturnController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseReturnController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Purchase Returns")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Import Returns")]
        public IActionResult ImportReturns()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Purchase Returns,Add Import Returns")]
        public async Task<IActionResult> Create(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }
            PurchaseReturnVM model = new PurchaseReturnVM();
            PurchaseBill purchaseBill = new PurchaseBill();

            purchaseBill = await _context.PurchaseBill.Where(r => r.CompanyID == CompID).Include(r=>r.Parties).FirstOrDefaultAsync(m=>m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }
            model.TLandingExpenses = purchaseBill.TLandingExpenses;
            model.PurchaseImport = purchaseBill.PurchaseImport;
            model.PurchaseBillID = purchaseBill.PurchaseBillID;
             
            model.BillDate_OldBill = purchaseBill.BillDate.ToString("d");
            
            model.ContactNumber_OldBill = purchaseBill.ContactNumber;
            model.ContactPerson_OldBill = purchaseBill.ContactPerson;            
            model.GrossTotal_OldBill = purchaseBill.GrossTotal;
            model.NetAmount_OldBill = purchaseBill.NetAmount;
            if(purchaseBill.PartiesID == null)
            {
                model.PartiesID = null;
                model.PR_S_1 = "Cash Purchase";
            }
            else
            {
                model.PartiesID = purchaseBill.PartiesID;
                model.PR_S_1 = purchaseBill.Parties.PartyName;
            }            
            model.Remarks_OldBill = purchaseBill.Remarks;
            model.TDiscount_OldBill = purchaseBill.TDiscount;
            model.TotalQuantity_OldBill = purchaseBill.TotalQuantity;

            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();
           
            ViewData["Ids"] = _context.Bank.Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r=>r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", purchaseBill.CurrencyID);
            //ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", purchaseBill.PartiesID);
          
            ViewData["ItemID"] = new SelectList(_context.Item.Where(r => r.CompanyID == CompID), "ItemID", "ItemID");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");
            ViewData["PurchaseBillID"] = new SelectList(_context.PurchaseBill.Where(r => r.CompanyID == CompID), "PurchaseBillID", "PurchaseBillNo");
            return View(model);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Purchase Returns,Add Import Returns")]
        public async Task<IActionResult> Create(PurchaseReturn purchaseReturn, IEnumerable<PurchaseBillItem> PurchaseBillItem_List, IEnumerable<Transaction> TransactionList)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            int purReturnID = 0;

            var PurchaseBillItem_List_1 = PurchaseBillItem_List.ToList();
            PurchaseBillItem_List_1.RemoveAll(x => x.PurchaseBillExtraInt_2 == 0);
            
            foreach (var item in PurchaseBillItem_List_1)
            {
                PurchaseReturn obj = new PurchaseReturn();

                obj.PR_B_1 = purchaseReturn.PR_B_1;
                obj.CreatedBy = User.Identity.Name;
                obj.CompanyID = CompID;
                obj.PurBillReturnDate = DateTime.Now;

                obj.PurchaseBillID = purchaseReturn.PurchaseBillID;
                obj.ItemID = item.ItemID;
                if (purchaseReturn.PartiesID == 0)
                {
                    obj.PartiesID = null;
                }
                else
                {
                    obj.PartiesID = purchaseReturn.PartiesID;
                }                

                obj.OldQuantity = item.Quantity;
                obj.ReturnQuantity = item.PurchaseBillExtraInt_2;
                obj.AmountToReceive = item.PurchaseBillExtraDecimal_1;

                obj.Remarks = purchaseReturn.Remarks;
                 
                _context.Add(obj);
                await _context.SaveChangesAsync();

                purReturnID = obj.PurchaseReturnID;

                // Decreasing Item Quantity from ITEM Table
                Item itemObj = new Item();
                decimal StockQuantity, final;
                
                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                StockQuantity = itemObj.Quantity;
                final = StockQuantity - item.PurchaseBillExtraInt_2;

                itemObj.Quantity = final;
                _context.Update(itemObj);
                await _context.SaveChangesAsync();
                // Decreasing Item Quantity from ITEM Table                
            }
            

            if (purchaseReturn.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = purchaseReturn.AmountToReceive;
                CF.PurchaseReturnID = purReturnID;
                CF.CompanyID = CompID;
                
                // Purchase Return  03-0002-0001
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                if(purchaseReturn.PR_B_1 == false)
                {
                    CF.VoucherType = "Purchase Return Voucher";
                }
                else
                {
                    CF.VoucherType = "Import Return Voucher";
                }
                
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = purchaseReturn.AmountToReceive;
                CF1.PurchaseReturnID = purReturnID;
                CF1.CompanyID = CompID;

                // Cash In Hand 01-0001-0001
                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                if (purchaseReturn.PR_B_1 == false)
                {
                    CF1.VoucherType = "Purchase Return Voucher";
                }
                else
                {
                    CF1.VoucherType = "Import Return Voucher";
                }
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }

            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Debit = purchaseReturn.AmountToReceive;
                CF2.PurchaseReturnID = purReturnID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = purchaseReturn.PartiesID;

                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseReturn.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc;
                CF2.SubAccountID = PartSubAcc;
                CF2.TransactionAccountID = (int)PartyTransID;
                if (purchaseReturn.PR_B_1 == false)
                {
                    CF2.VoucherType = "Purchase Return Voucher";
                }
                else
                {
                    CF2.VoucherType = "Import Return Voucher";
                }
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Credit = purchaseReturn.AmountToReceive;
                CF3.PurchaseReturnID = purReturnID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = purchaseReturn.PartiesID;

                // Purchase Return  03-0002-0001
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF3.MainAccountID = MainAccID;
                CF3.SubAccountID = SubAccID;
                CF3.TransactionAccountID = TraAccID;
                if (purchaseReturn.PR_B_1 == false)
                {
                    CF3.VoucherType = "Purchase Return Voucher";
                }
                else
                {
                    CF3.VoucherType = "Import Return Voucher";
                }
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();

                if (TransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Credit = TransactionList.FirstOrDefault().Amount;
                    CF4.PurchaseReturnID = purReturnID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = purchaseReturn.PartiesID;

                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseReturn.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc_a = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc_a = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF4.MainAccountID = PartMainAcc_a;
                    CF4.SubAccountID = PartSubAcc_a;
                    CF4.TransactionAccountID = (int)PartyTransID_1;
                    if (purchaseReturn.PR_B_1 == false)
                    {
                        CF4.VoucherType = "Purchase Return Voucher";
                    }
                    else
                    {
                        CF4.VoucherType = "Import Return Voucher";
                    }
                    CF4.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF4);
                    await _context.SaveChangesAsync();


                    if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cash) // Mode 0 Means ==> Cash
                    {
                        CashFlow CF5 = new CashFlow();
                        CF5.Debit = TransactionList.FirstOrDefault().Amount;
                        CF5.PurchaseReturnID = purReturnID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = purchaseReturn.PartiesID;
                        // Cash In Hand 01-0001-0001
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.SubAccountID= SubAccID_2;
                        CF5.MainAccountID = MainAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        if (purchaseReturn.PR_B_1 == false)
                        {
                            CF5.VoucherType = "Purchase Return Voucher";
                        }
                        else
                        {
                            CF5.VoucherType = "Import Return Voucher";
                        }
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Debit = TransactionList.FirstOrDefault().Amount;
                        CF6.PurchaseReturnID = purReturnID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = purchaseReturn.PartiesID;

                        int? bankID = TransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;

                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;

                        CF6.TransactionAccountID = transBankID;
                        if (purchaseReturn.PR_B_1 == false)
                        {
                            CF6.VoucherType = "Purchase Return Voucher";
                        }
                        else
                        {
                            CF6.VoucherType = "Import Return Voucher";
                        }
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }

            }

            var TransactionList_Result = TransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.TransactionID == 777777);
            TransactionList_Result.ForEach(x => x.TransactionID = 0);

            foreach (var item in TransactionList_Result)
            {
                PurchaseReturnTransaction transaction = new PurchaseReturnTransaction();
                transaction.CompanyID = CompID;
                transaction.PurchaseReturnID = purReturnID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.PurchaseBillID = purchaseReturn.PurchaseBillID;

                _context.PurchaseReturnTransaction.Add(transaction);

                await _context.SaveChangesAsync();

            }
            if (User.IsInRole("CompanyAdmin"))
            {
                return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "CompanyUser" });
            }            
        }

        private bool PurchaseReturnExists(int id)
        {
            return _context.PurchaseReturn.Any(e => e.PurchaseReturnID == id);
        }
        
        [Route("[action]")]
        [Authorize]
        public JsonResult LoadPurchaseReturns()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.PurchaseReturn.Where(r => r.CompanyID == CompID).Where(r => r.PR_B_1 == false).Include(r => r.Parties).Select(x => new { x.PurchaseReturnID, billDate = x.PurBillReturnDate.ToString("d")  , x.Parties.PartyName , x.ContactPerson, x.ReturnQuantity, x.AmountToReceive , x.Remarks ,x.Item.OEMNo , x.OldQuantity , x.PurchaseBillID,x.ContactNumber ,x.Item.ItemName ,x.CreatedBy}).ToList();
            return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadImportReturns()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.PurchaseReturn.Where(r => r.CompanyID == CompID).Where(r=>r.PR_B_1 == true).Include(r => r.Parties).Select(x => new { x.PurchaseReturnID, billDate = x.PurBillReturnDate.ToString("d"), x.Parties.PartyName, x.ContactPerson, x.ReturnQuantity, x.AmountToReceive, x.Remarks, x.Item.OEMNo, x.OldQuantity, x.PurchaseBillID, x.ContactNumber, x.Item.ItemName, x.CreatedBy }).ToList();
            return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult CheckStock(int ItemID)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.ItemID == ItemID).Select(x => new { x.Quantity }).FirstOrDefault();
            return Json(a);
        }
        
    }
}