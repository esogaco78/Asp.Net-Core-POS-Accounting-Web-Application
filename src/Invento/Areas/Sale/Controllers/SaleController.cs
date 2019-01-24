using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Product.Models;
using Invento.Areas.Finance.Models;
using Invento.Areas.Sale.Models;
using Invento.Areas.Reports.Models;

namespace Invento.Areas.Sale.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Sale")]
    [Route("Sale/[controller]")]
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Sale Invoices")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Canceled Sale Invoices")]
        public IActionResult CancelInvoices()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Cancel Sale Invoices")]
        public IActionResult CancelInvoice(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            SaleBillVM model = new SaleBillVM();
            var saleBill = _context.SaleBill.SingleOrDefault(m => m.SaleBillID == id);
            
            model.SaleBillID = saleBill.SaleBillID;
            model.Balance = saleBill.Balance;
            model.BillDate = saleBill.BillDate;
            model.CashPaid = saleBill.CashPaid;
            model.ContactNumber = saleBill.ContactNumber;
            model.ContactPerson = saleBill.ContactPerson;
            model.CreditDays = saleBill.CreditDays;
            model.CurrencyID = saleBill.CurrencyID;
            model.ExchangeRate = saleBill.ExchangeRate;
            model.ExternalRef = saleBill.ExternalRef;
            model.GrossTotal = saleBill.GrossTotal;
            model.NetAmount = saleBill.NetAmount;
            model.PartiesID = saleBill.PartiesID;
            model.PayTerms = saleBill.PayTerms;
            model.SaleBillNo = saleBill.SaleBillID.ToString();
            model.RefDate = saleBill.RefDate;
            model.Remarks = saleBill.Remarks;
            model.TDiscount = saleBill.TDiscount;
            model.TotalQuantity = saleBill.TotalQuantity;
            model.SB_D_1 = saleBill.SB_D_1;

            model.SaleBillItem_List = _context.SaleBillItem.Include(r => r.Item).Where(r => r.SaleBillID == id).ToList();
            
            return PartialView(model);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Cancel Sale Invoices")]
        public IActionResult CancelInvoice(SaleBill salBill, IEnumerable<SaleBillItem> SaleBillItem_List)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            salBill.SB_B_1 = true;
            salBill.CreatedBy = User.Identity.Name;
            salBill.CompanyID = CompID;
            int total = _context.SaleBillItem.Where(r => r.CompanyID == CompID).Where(r => r.SaleBillID == salBill.SaleBillID).Count();
            for(int i= 0; i< total;i++)
            {
                SaleBillItem SBI = new SaleBillItem();
                SBI = _context.SaleBillItem.Where(r => r.SaleBillID == salBill.SaleBillID).Where(r => r.SaleBillExtraBool == false).FirstOrDefault();
                SBI.SaleBillExtraBool = true;
                _context.Update(SBI);
                _context.SaveChanges();
            }         

            _context.Update(salBill);
            _context.SaleTransaction.RemoveRange(_context.SaleTransaction.Where(x => x.SaleBillID == salBill.SaleBillID));
            _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.SaleBillID == salBill.SaleBillID));
            _context.SaveChanges();

            // Decreasing Item Quantity from ITEM Table
            decimal StockQuantity, final;
            foreach (var item in SaleBillItem_List)
            {
                Item itemObj = new Item();
                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                StockQuantity = itemObj.Quantity;
                final = StockQuantity + item.Quantity;

                itemObj.Quantity = final;
                _context.Update(itemObj);
                _context.SaveChanges();
            }
            // Decreasing Item Quantity from ITEM Table 

            return RedirectToAction("Index");
        }



        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Sale Invoices Details")]
        public async Task<IActionResult> Details(int? id)
        {
            SaleBillVM model = new SaleBillVM();
            var saleBill = await _context.SaleBill.SingleOrDefaultAsync(m => m.SaleBillID == id);
            if (saleBill == null)
            {
                return NotFound();
            }
            model.SaleBillID = saleBill.SaleBillID;
            model.Balance = saleBill.Balance;

            model.SB_S_1 = saleBill.BillDate.ToString("dd/MM/yyyy");
            model.CashPaid = saleBill.CashPaid;
            model.ContactNumber = saleBill.ContactNumber;
            model.ContactPerson = saleBill.ContactPerson;
            model.CreditDays = saleBill.CreditDays;
            model.CurrencyID = saleBill.CurrencyID;
            model.ExchangeRate = saleBill.ExchangeRate;
            model.ExternalRef = saleBill.ExternalRef;
            model.GrossTotal = saleBill.GrossTotal;
            model.NetAmount = saleBill.NetAmount;
            model.PartiesID = saleBill.PartiesID;
            model.PayTerms = saleBill.PayTerms;
            model.SaleBillNo = saleBill.SaleBillID.ToString();
            model.SB_S_2 = saleBill.RefDate.ToString("dd/MM/yyyy");
            model.SB_B_1 = saleBill.SB_B_1;

            model.Remarks = saleBill.Remarks;
            model.TDiscount = saleBill.TDiscount;
            model.TotalQuantity = saleBill.TotalQuantity;
            model.SaleBillItem_List = _context.SaleBillItem.Include(r => r.Item).Where(r => r.SaleBillID == id).ToList();
            model.SaleTransactionList = _context.SaleTransaction.Where(r => r.SaleBillID == id).ToList();
            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Sale Invoices")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (_context.SaleBill.Any() == true)
            {
                int BillNo = _context.SaleBill.LastOrDefault().SaleBillID;
                int BillNo_ADD = BillNo + 1;
                string BillNo_String = BillNo_ADD.ToString();
                ViewData["BillNo"] = BillNo_String;
            }
            else
            {
                ViewData["BillNo"] = "1";
            }

            ViewData["Ids"] = _context.Bank.Where(r=>r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");

            return View();
        }
 
        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Sale Invoices")]
        public async Task<IActionResult> Create(SaleBill saleBill, IEnumerable<Item> ItemList, IEnumerable<SaleTransaction> SaleTransactionList)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main Sale Bill
            saleBill.CreatedBy = User.Identity.Name;
            saleBill.CompanyID = CompID;

            decimal discountDeci = saleBill.GrossTotal - saleBill.NetAmount;
            saleBill.SB_D_1 = discountDeci;

            _context.SaleBill.Add(saleBill);
            await _context.SaveChangesAsync();

            // Main Sale Bill            

            int SalBillID = saleBill.SaleBillID;
            
            var TransactionList_Result = SaleTransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.SaleTransactionID == 777777);
            TransactionList_Result.ForEach(x => x.SaleTransactionID = 0);
            foreach (var item in TransactionList_Result)
            {
                SaleTransaction transaction = new SaleTransaction();
                transaction.CompanyID = CompID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.SaleBillID = SalBillID;
                _context.SaleTransaction.Add(transaction);
                await _context.SaveChangesAsync();
            }

            // Sale Items List Entry 
            var ItemList_Result = ItemList.ToList();
            ItemList_Result.RemoveAll(x => x.CompanyID == 777777);
            ItemList_Result.ForEach(x => x.CompanyID = CompID);
            foreach (var item in ItemList_Result)
            {
                SaleBillItem salBillItem = new SaleBillItem();

                salBillItem.ItemID = item.ItemID;
                salBillItem.SaleBillID = SalBillID;
                salBillItem.SalePrice = item.LCPrice;
                salBillItem.Quantity = item.Quantity;
                salBillItem.CompanyID = CompID;

                _context.SaleBillItem.Add(salBillItem);
                await _context.SaveChangesAsync();

                // Decrease Quantity of Items in Stock (Item Table)
                Item itemObj = new Item();
                decimal Old_Quantity, New_Quantity;

                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                Old_Quantity = itemObj.Quantity;
                New_Quantity = Old_Quantity - item.Quantity;

                itemObj.Quantity = New_Quantity;
                _context.Update(itemObj);
                await _context.SaveChangesAsync();
                // Decrease Quantity of Items in Stock (Item Table)
            }
            // Sale Items List Entry

            if (saleBill.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = saleBill.NetAmount;
                CF.SaleBillID = SalBillID;
                CF.CompanyID = CompID;

                // Cash Sale 03-0001-0001
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID= SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Sale Bill Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = saleBill.NetAmount;
                CF1.SaleBillID = SalBillID;
                CF1.CompanyID = CompID;

                // Cash In Hand  01-0001-0001
                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Sale Bill Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }
            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Debit = saleBill.NetAmount;
                CF2.SaleBillID = SalBillID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = saleBill.PartiesID;
                // Party Debit
                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == saleBill.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc1;
                CF2.SubAccountID = PartSubAcc1;
                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.VoucherType = "Sale Bill Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Credit = saleBill.NetAmount;
                CF3.SaleBillID = SalBillID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = saleBill.PartiesID;
                // Sale Credit 03-0001-0002
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0002").FirstOrDefault().TransactionAccountID;

                CF3.MainAccountID = MainAccID;
                CF3.SubAccountID = SubAccID;
                CF3.TransactionAccountID = TraAccID;
                CF3.VoucherType = "Sale Bill Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();


                if (SaleTransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Credit = SaleTransactionList.FirstOrDefault().Amount;
                    CF4.SaleBillID = SalBillID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = saleBill.PartiesID;
                    // Party Credit
                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == saleBill.PartiesID).FirstOrDefault().TransactionAccountID;
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
                        CF5.Debit = SaleTransactionList.FirstOrDefault().Amount;
                        CF5.SaleBillID = SalBillID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = saleBill.PartiesID;
                        // Cash in Hand Debit 01-0001-0001
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID= SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Sale Bill Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (SaleTransactionList.FirstOrDefault().Mode == Models.PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Debit = SaleTransactionList.FirstOrDefault().Amount;
                        CF6.SaleBillID = SalBillID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = saleBill.PartiesID;
                        // Bank Debit
                        int? bankID = SaleTransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Sale Bill Voucher";
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }
            }
                // Sale Transaction Entry
                         
            return RedirectToAction("Index");
        }

         
        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Sale Invoices")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }
            SaleBillVM model = new SaleBillVM();

            var saleBill = await _context.SaleBill.SingleOrDefaultAsync(m => m.SaleBillID == id);
            if (saleBill == null)
            {
                return NotFound();
            }

            model.SaleBillID = saleBill.SaleBillID;
            model.Balance = saleBill.Balance;
            model.BillDate = saleBill.BillDate;
            model.CashPaid = saleBill.CashPaid;
            model.ContactNumber = saleBill.ContactNumber;
            model.ContactPerson = saleBill.ContactPerson;
            model.CreditDays = saleBill.CreditDays;
            model.CurrencyID = saleBill.CurrencyID;
            model.ExchangeRate = saleBill.ExchangeRate;
            model.ExternalRef = saleBill.ExternalRef;
            model.GrossTotal = saleBill.GrossTotal;
            model.NetAmount = saleBill.NetAmount;
            model.PartiesID = saleBill.PartiesID;
            model.PayTerms = saleBill.PayTerms;
            model.SaleBillNo = saleBill.SaleBillID.ToString();
            model.RefDate = saleBill.RefDate;
            model.Remarks = saleBill.Remarks;
            model.TDiscount = saleBill.TDiscount;
            model.TotalQuantity = saleBill.TotalQuantity;
            model.SB_D_1 = saleBill.SB_D_1;

            model.SaleBillItem_List = _context.SaleBillItem.Include(r => r.Item).Where(r => r.SaleBillID == id).ToList();

            model.SaleTransactionList = _context.SaleTransaction.Where(r => r.SaleBillID == id).ToList();
            
            ViewData["Ids"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", saleBill.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", saleBill.PartiesID);
            return View(model);
        }


        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Sale Invoices")]
        public async Task<IActionResult> Edit(SaleBill saleBill, IEnumerable<SaleBillItem> SaleBillItem_List, IEnumerable<SaleTransaction> SaleTransactionList)
        {
            int SalBillID = saleBill.SaleBillID;

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main Sale Bill
            saleBill.CreatedBy = User.Identity.Name;
            saleBill.CompanyID = CompID;

            _context.Update(saleBill);
            await _context.SaveChangesAsync();

            _context.SaleTransaction.RemoveRange(_context.SaleTransaction.Where(x => x.SaleBillID == saleBill.SaleBillID));
            _context.SaleBillItem.RemoveRange(_context.SaleBillItem.Where(x => x.SaleBillID == saleBill.SaleBillID));
            _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.SaleBillID == saleBill.SaleBillID));
            _context.SaveChanges();

            // Sale Items List Entry 

            var TransactionList_Result = SaleTransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.SaleTransactionID == 777777);
            TransactionList_Result.ForEach(x => x.SaleTransactionID = 0);

            foreach (var item in TransactionList_Result)
            {
                SaleTransaction transaction = new SaleTransaction();
                transaction.CompanyID = CompID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.SaleBillID = SalBillID;
                _context.SaleTransaction.Add(transaction);
                await _context.SaveChangesAsync();
            }

            var ItemList_Result = SaleBillItem_List.ToList();
            ItemList_Result.RemoveAll(x => x.SaleBillExtraInt_1 == 777777);

            foreach (var item in ItemList_Result)
            {
                SaleBillItem salBillItem = new SaleBillItem();
                
                if (item.SaleBillExtraInt_2 != 0)
                {
                    // Increase Quantity of Items in Stock (Item Table)
                    Item itemObj = new Item();
                    decimal OldBillQuantity, StockQuantity, final;

                    OldBillQuantity = item.SaleBillExtraInt; // Old Quantity

                    itemObj = _context.Item.Where(r => r.ItemID == item.SaleBillExtraInt_2).SingleOrDefault();
                    StockQuantity = itemObj.Quantity;
                    final = StockQuantity + OldBillQuantity;

                    itemObj.Quantity = final;
                    _context.Update(itemObj);
                    await _context.SaveChangesAsync();
                    // Increase Quantity of Items in Stock (Item Table)
                }
                salBillItem.ItemID = item.ItemID;
                salBillItem.SaleBillID = SalBillID;
                salBillItem.SalePrice = item.SalePrice;
                salBillItem.Quantity = item.Quantity;
                salBillItem.CompanyID = CompID;

                _context.SaleBillItem.Add(salBillItem);
                await _context.SaveChangesAsync();

                // Increase Quantity of Items in Stock (Item Table)

                // Decrease Quantity of Items in Stock (Item Table)
                Item itemObj1 = new Item();
                decimal Old_Quantity, New_Quantity;

                itemObj1 = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                Old_Quantity = itemObj1.Quantity;
                New_Quantity = Old_Quantity - item.Quantity;

                itemObj1.Quantity = New_Quantity;
                _context.Update(itemObj1);
                await _context.SaveChangesAsync();
                // Decrease Quantity of Items in Stock (Item Table)
            }
            // Sale Items List Entry

            if (saleBill.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = saleBill.NetAmount;
                CF.SaleBillID = SalBillID;
                CF.CompanyID = CompID;

                // Cash Sale 03-0001-0001
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Sale Bill Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = saleBill.NetAmount;
                CF1.SaleBillID = SalBillID;
                CF1.CompanyID = CompID;

                // Cash In Hand  01-0001-0001
                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Sale Bill Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }
            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Debit = saleBill.NetAmount;
                CF2.SaleBillID = SalBillID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = saleBill.PartiesID;
                // Party Debit
                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == saleBill.PartiesID).FirstOrDefault().TransactionAccountID;
                int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.MainAccountID = PartMainAcc1;
                CF2.SubAccountID = PartSubAcc1;
                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.VoucherType = "Sale Bill Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Credit = saleBill.NetAmount;
                CF3.SaleBillID = SalBillID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = saleBill.PartiesID;
                // Sale Credit 03-0001-0002
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0002").FirstOrDefault().TransactionAccountID;

                CF3.MainAccountID = MainAccID;
                CF3.SubAccountID = SubAccID;
                CF3.TransactionAccountID = TraAccID;
                CF3.VoucherType = "Sale Bill Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();
                
                if (SaleTransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Credit = SaleTransactionList.FirstOrDefault().Amount;
                    CF4.SaleBillID = SalBillID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = saleBill.PartiesID;
                    // Party Credit
                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == saleBill.PartiesID).FirstOrDefault().TransactionAccountID;
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
                        CF5.Debit = SaleTransactionList.FirstOrDefault().Amount;
                        CF5.SaleBillID = SalBillID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = saleBill.PartiesID;
                        // Cash in Hand Debit 01-0001-0001
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID = SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Sale Bill Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (SaleTransactionList.FirstOrDefault().Mode == Models.PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Debit = SaleTransactionList.FirstOrDefault().Amount;
                        CF6.SaleBillID = SalBillID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = saleBill.PartiesID;
                        // Bank Debit
                        int? bankID = SaleTransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Sale Bill Voucher";
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            // Sale Transaction Entry
            
            return RedirectToAction("Index");
        }
        
        private bool SaleBillExists(int id)
        {
            return _context.SaleBill.Any(e => e.SaleBillID == id);
        }
        
        [Route("[action]")]
        [Authorize]
        public JsonResult LoadOEMNoAutoComplete(string Prefix)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var OEMNo = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.OEMNo.Contains(Prefix)).Select(p => p.OEMNo).ToList();
            return Json(OEMNo);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadCrossRefAutoComplete(string Prefix)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var CrossRef = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.CrossRef.Contains(Prefix)).Select(p => p.CrossRef).ToList();
            return Json(CrossRef);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadDescription(string name)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            //var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.OEMNo == name).Include(r=>r.SaleBillItem).Select(x => new { x.ProductDescription , x.ItemID, SalePrice = x.SaleBillItem.Where(r=>r.SaleBillExtraBool == false).OrderByDescending(r => r.SaleBillID).FirstOrDefault()} ).FirstOrDefault();            

            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.OEMNo == name).Select(x => new { x.ProductDescription, x.ItemID, SalePrice = x.SalePrice }).FirstOrDefault();

            return Json(a);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadDescriptionByCrossRef(string name)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.CrossRef == name).Select(x => new { x.ProductDescription, x.ItemID, SalePrice = x.SalePrice }).FirstOrDefault();

            return Json(a);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadSaleBills()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.SaleBill.Where(r => r.CompanyID == CompID).Where(r=>r.SB_B_1 == false).Include(r => r.Parties).Select(x => new { x.SaleBillID, billDate = x.BillDate.ToString("d"), x.Balance, x.ContactPerson, x.GrossTotal, x.TDiscount, x.NetAmount, x.Parties.PartyName, x.Remarks, x.TotalQuantity, x.Parties.Phone1, x.Currency.CurrencyName ,x.CreatedBy}).ToList();

            return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadCancelBills()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.SaleBill.Where(r => r.CompanyID == CompID).Where(r => r.SB_B_1 == true).Include(r => r.Parties).Select(x => new { x.SaleBillID, billDate = x.BillDate.ToString("d"), x.Balance, x.ContactPerson, x.GrossTotal, x.TDiscount, x.NetAmount, x.Parties.PartyName, x.Remarks, x.TotalQuantity, x.Parties.Phone1, x.Currency.CurrencyName, x.CreatedBy }).ToList();

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

        [Route("[action]")]
        [Authorize]
        public JsonResult ItemBriefDetails(int ItemID)
        {
            ProductLedgerVM model = new ProductLedgerVM();
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var lst = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                            .Include(r => r.Item)
                            .Where(r => r.SaleBillExtraBool == false)
                            .Where(r => r.ItemID == ItemID)
                            .OrderByDescending(r => r.SaleBill.BillDate)
                            .ToList();

            model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                              .Where(r => r.ItemID == ItemID).Sum(r => r.Quantity);

            model.LastPurchasePrice = lst.FirstOrDefault().SalePrice;
            model.OemNo = lst.FirstOrDefault().Item.OEMNo;
            model.CrossRef = lst.FirstOrDefault().Item.CrossRef;

            int TotalRowsCount = lst.Count();
            model.TotalRowsCount = TotalRowsCount;

            decimal TotalQuantity = lst.Sum(r => r.Quantity);
            model.TotalQuantity = TotalQuantity;

            decimal TotalPrice = lst.Sum(r => r.SalePrice);
            TotalPrice = Math.Round(TotalPrice, 2);
            model.TotalPrice = TotalPrice;

            decimal AveragePrice = TotalPrice / TotalRowsCount;
            AveragePrice = Math.Round(AveragePrice, 2);
            model.AveragePrice = AveragePrice;

            return Json(new { data = model });
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Sale Invoices Details")]
        public async Task<IActionResult> InvoicePrint(int? id)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            SaleBillVM model = new SaleBillVM();
            var saleBill = await _context.SaleBill.SingleOrDefaultAsync(m => m.SaleBillID == id);
            if (saleBill == null)
            {
                return NotFound();
            }

            if (_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == true)
            {
                model.CompanyName = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyName;
            }

            model.SaleBillID = saleBill.SaleBillID;
            model.Balance = saleBill.Balance;

            model.SB_S_1 = saleBill.BillDate.ToString("dd/MM/yyyy");
            model.CashPaid = saleBill.CashPaid;
            model.ContactNumber = saleBill.ContactNumber;
            model.ContactPerson = saleBill.ContactPerson;
            model.CreditDays = saleBill.CreditDays;
            model.CurrencyID = saleBill.CurrencyID;
            model.ExchangeRate = saleBill.ExchangeRate;
            model.ExternalRef = saleBill.ExternalRef;
            model.GrossTotal = saleBill.GrossTotal;
            model.NetAmount = saleBill.NetAmount;
            model.PartiesID = saleBill.PartiesID;
            model.PayTerms = saleBill.PayTerms;
            model.SaleBillNo = saleBill.SaleBillID.ToString();
            model.SB_S_2 = saleBill.RefDate.ToString("dd/MM/yyyy");
            model.SB_B_1 = saleBill.SB_B_1;

            model.Remarks = saleBill.Remarks;
            model.TDiscount = saleBill.TDiscount;
            model.TotalQuantity = saleBill.TotalQuantity;
            model.SaleBillItem_List = _context.SaleBillItem.Include(r => r.Item).Where(r => r.SaleBillID == id).ToList();
            model.SaleTransactionList = _context.SaleTransaction.Where(r => r.SaleBillID == id).ToList();
            return View(model);
        }


    }
}