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
using Invento.Areas.Reports.Models;

namespace Invento.Areas.Purchase.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Purchase")]
    [Route("Purchase/[controller]")]
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Purchase Invoices")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Canceled Purchase Invoices")]
        public IActionResult CancelInvoices()
        {
            return View();
        }
  
        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Purchase Invoices Details")]
        public async Task<IActionResult> Details(int? id)
        {
            PurchaseBillVM model = new PurchaseBillVM();
            var purchaseBill = await _context.PurchaseBill.Include(r => r.Parties).SingleOrDefaultAsync(m => m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }
            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;

            model.PB_S_1 = purchaseBill.BillDate.ToString("dd/MM/yyyy");
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            if (purchaseBill.Parties != null)
            {
                model.PartyName = purchaseBill.Parties.PartyName;
            }
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.PB_S_2 = purchaseBill.RefDate.ToString("dd/MM/yyyy");
            model.PB_B_1 = purchaseBill.PB_B_1;

            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();
            model.TransactionList = _context.Transaction.Where(r => r.PurchaseBillID == id).ToList();
            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Purchase Invoices")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (_context.PurchaseBill.Any() == true)
            {
                int BillNo = _context.PurchaseBill.LastOrDefault().PurchaseBillID;
                int BillNo_ADD = BillNo + 1;
                string BillNo_String = BillNo_ADD.ToString();
                ViewData["BillNo"] = BillNo_String;
            }
            else
            {
                ViewData["BillNo"] = "1";
            }

            ViewData["Ids"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Purchase Invoices")]
        public async Task<IActionResult> Create(PurchaseBill purchaseBill, IEnumerable<Item> ItemList, IEnumerable<Transaction> TransactionList)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main Purchase Bill
            purchaseBill.CreatedBy = User.Identity.Name;
            purchaseBill.CompanyID = CompID;

            decimal discountDeci = purchaseBill.GrossTotal - purchaseBill.NetAmount;
            purchaseBill.PB_D_1 = discountDeci;


            _context.PurchaseBill.Add(purchaseBill);
            await _context.SaveChangesAsync();

            // Main Purchase Bill            

            int PurBillID = purchaseBill.PurchaseBillID;

            // Purchase Items List Entry 
            var ItemList_Result = ItemList.ToList();
            ItemList_Result.RemoveAll(x => x.CompanyID == 777777);
            ItemList_Result.ForEach(x => x.CompanyID = CompID);
            foreach (var item in ItemList_Result)
            {
                PurchaseBillItem purBillItem = new PurchaseBillItem();

                purBillItem.ItemID = item.ItemID;
                purBillItem.PurchaseBillID = PurBillID;
                purBillItem.PurchasePrice = item.LCPrice;
                purBillItem.Quantity = item.Quantity;
                purBillItem.CompanyID = CompID;

                _context.PurchaseBillItem.Add(purBillItem);
                await _context.SaveChangesAsync();

                // Increase Quantity of Items in Stock (Item Table)
                Item itemObj = new Item();
                decimal Old_Quantity, New_Quantity;

                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                Old_Quantity = itemObj.Quantity;
                New_Quantity = Old_Quantity + item.Quantity;

                itemObj.Quantity = New_Quantity;
                _context.Update(itemObj);
                await _context.SaveChangesAsync();
                // Increase Quantity of Items in Stock (Item Table)
            }
            // Purchase Items List Entry
            // Transaction Entry

            if (purchaseBill.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = purchaseBill.NetAmount;
                CF.PurchaseBillID = PurBillID;
                CF.CompanyID = CompID;
                //Cash In hand
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Purchase Bill Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = purchaseBill.NetAmount;
                CF1.PurchaseBillID = PurBillID;
                CF1.CompanyID = CompID;

                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Purchase Bill Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }
            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Credit = purchaseBill.NetAmount;
                CF2.PurchaseBillID = PurBillID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = purchaseBill.PartiesID;

                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;

                int PartMainAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.MainAccountID = PartMainAcc;
                CF2.SubAccountID = PartSubAcc;
                CF2.VoucherType = "Purchase Bill Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = purchaseBill.NetAmount;
                CF3.PurchaseBillID = PurBillID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = purchaseBill.PartiesID;

                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0002").FirstOrDefault().TransactionAccountID;

                CF3.TransactionAccountID = TraAccID;
                CF3.SubAccountID = SubAccID;
                CF3.MainAccountID = MainAccID;
                CF3.VoucherType = "Purchase Bill Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();


                if (TransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Debit = TransactionList.FirstOrDefault().Amount;
                    CF4.PurchaseBillID = PurBillID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = purchaseBill.PartiesID;

                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF4.MainAccountID = PartMainAcc1;
                    CF4.SubAccountID = PartSubAcc1;
                    CF4.TransactionAccountID = (int)PartyTransID_1;
                    CF4.VoucherType = "Purchase Bill Voucher";
                    CF4.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF4);
                    await _context.SaveChangesAsync();


                    if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cash) // Mode 0 Means ==> Cash
                    {
                        CashFlow CF5 = new CashFlow();
                        CF5.Credit = TransactionList.FirstOrDefault().Amount;
                        CF5.PurchaseBillID = PurBillID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = purchaseBill.PartiesID;
                        // Cash in hand
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID = SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Purchase Bill Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Credit = TransactionList.FirstOrDefault().Amount;
                        CF6.PurchaseBillID = PurBillID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = purchaseBill.PartiesID;
                        // Bank credit
                        int? bankID = TransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Purchase Bill Voucher";
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }

            }

            //if(purchaseBill.PartiesID != null)
            //{ 
            var TransactionList_Result = TransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.TransactionID == 777777);
            TransactionList_Result.ForEach(x => x.TransactionID = 0);

            foreach (var item in TransactionList_Result)
            {
                Transaction transaction = new Transaction();
                transaction.CompanyID = CompID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.PurchaseBillID = PurBillID;

                _context.Transaction.Add(transaction);

                await _context.SaveChangesAsync();

            }
            //}
            // Transaction Entry
            return RedirectToAction("Details", "Purchase", new { id = PurBillID });
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Cancel Purchase Invoices,Cancel Import Bill")]
        public IActionResult CancelInvoice(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            var purchaseBill = _context.PurchaseBill.SingleOrDefault(m => m.PurchaseBillID == id);

            PurchaseBillVM model = new PurchaseBillVM();

            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;
            model.BillDate = purchaseBill.BillDate;
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            model.PartiesID = purchaseBill.PartiesID;
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.RefDate = purchaseBill.RefDate;
            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PB_D_1 = purchaseBill.PB_D_1;
            model.TLandingExpenses = purchaseBill.TLandingExpenses;
            model.PurchaseImport = purchaseBill.PurchaseImport;

            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();

            return PartialView(model);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Cancel Purchase Invoices,Cancel Import Bill")]
        public IActionResult CancelInvoice(PurchaseBill purBill, IEnumerable<PurchaseBillItem> PurchaseBillItem_List)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            purBill.PB_B_1 = true;
            purBill.CreatedBy = User.Identity.Name;
            purBill.CompanyID = CompID;

            int total = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID).Where(r => r.PurchaseBillID == purBill.PurchaseBillID).Count();
            for (int i = 0; i < total; i++)
            {
                PurchaseBillItem PBI = new PurchaseBillItem();
                PBI = _context.PurchaseBillItem.Where(r => r.PurchaseBillID == purBill.PurchaseBillID).Where(r => r.PurchaseBillExtraBool == false).FirstOrDefault();
                PBI.PurchaseBillExtraBool = true;
                _context.Update(PBI);
                _context.SaveChanges();
            }
            _context.Update(purBill);
            _context.Transaction.RemoveRange(_context.Transaction.Where(x => x.PurchaseBillID == purBill.PurchaseBillID));
            _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.PurchaseBillID == purBill.PurchaseBillID));
            _context.SaveChanges();

            // Decreasing Item Quantity from ITEM Table
            decimal StockQuantity, final;
            foreach (var item in PurchaseBillItem_List)
            {
                Item itemObj = new Item();
                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                StockQuantity = itemObj.Quantity;
                final = StockQuantity - item.Quantity;

                itemObj.Quantity = final;
                _context.Update(itemObj);
                _context.SaveChanges();
            }
            // Decreasing Item Quantity from ITEM Table  

            if (User.IsInRole("CompanyAdmin"))
            {
                return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "CompanyUser" });
            }
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Purchase Invoices")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }
            PurchaseBillVM model = new PurchaseBillVM();

            var purchaseBill = await _context.PurchaseBill.SingleOrDefaultAsync(m => m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }

            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;
            model.BillDate = purchaseBill.BillDate;
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            model.PartiesID = purchaseBill.PartiesID;
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.RefDate = purchaseBill.RefDate;
            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PB_D_1 = purchaseBill.PB_D_1;


            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();

            model.TransactionList = _context.Transaction.Where(r => r.PurchaseBillID == id).ToList();

            ViewData["Ids"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", purchaseBill.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", purchaseBill.PartiesID);
            return View(model);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Purchase Invoices")]
        public async Task<IActionResult> Edit(PurchaseBill purchaseBill, IEnumerable<PurchaseBillItem> PurchaseBillItem_List, IEnumerable<Transaction> TransactionList)
        {
            int PurBillID = purchaseBill.PurchaseBillID;

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main Purchase Bill
            purchaseBill.CreatedBy = User.Identity.Name;
            purchaseBill.CompanyID = CompID;

            _context.Update(purchaseBill);
            await _context.SaveChangesAsync();

            _context.Transaction.RemoveRange(_context.Transaction.Where(x => x.PurchaseBillID == purchaseBill.PurchaseBillID));
            _context.PurchaseBillItem.RemoveRange(_context.PurchaseBillItem.Where(x => x.PurchaseBillID == purchaseBill.PurchaseBillID));
            _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.PurchaseBillID == purchaseBill.PurchaseBillID));
            _context.SaveChanges();

            var TransactionList_Result = TransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.TransactionID == 777777);
            TransactionList_Result.ForEach(x => x.TransactionID = 0);
            foreach (var item in TransactionList_Result)
            {
                Transaction transaction = new Transaction();
                transaction.CompanyID = CompID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.PurchaseBillID = PurBillID;
                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
            }

            //Purchase Items List Entry

            var ItemList_Result = PurchaseBillItem_List.ToList();
            ItemList_Result.RemoveAll(x => x.PurchaseBillExtraInt_1 == 777777);

            foreach (var item in ItemList_Result)
            {
                PurchaseBillItem purBillItem = new PurchaseBillItem();

                if (item.PurchaseBillExtraInt_2 != 0)
                {
                    // Decrease Quantity of Items in Stock (Item Table)
                    Item itemObj = new Item();
                    decimal OldBillQuantity, StockQuantity, final;

                    OldBillQuantity = item.PurchaseBillExtraInt; // Old Quantity

                    itemObj = _context.Item.Where(r => r.ItemID == item.PurchaseBillExtraInt_2).SingleOrDefault();
                    StockQuantity = itemObj.Quantity;
                    final = StockQuantity - OldBillQuantity;

                    itemObj.Quantity = final;
                    _context.Update(itemObj);
                    await _context.SaveChangesAsync();
                    // Decrease Quantity of Items in Stock (Item Table)
                }
                purBillItem.ItemID = item.ItemID;
                purBillItem.PurchaseBillID = PurBillID;
                purBillItem.PurchasePrice = item.PurchasePrice;
                purBillItem.Quantity = item.Quantity;
                purBillItem.CompanyID = CompID;

                _context.PurchaseBillItem.Add(purBillItem);
                await _context.SaveChangesAsync();

                // Increase Quantity of Items in Stock (Item Table)
                Item itemObj1 = new Item();
                decimal Old_Quantity, New_Quantity;

                itemObj1 = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                Old_Quantity = itemObj1.Quantity;
                New_Quantity = Old_Quantity + item.Quantity;

                itemObj1.Quantity = New_Quantity;
                _context.Update(itemObj1);
                await _context.SaveChangesAsync();
                // Increase Quantity of Items in Stock (Item Table)
            }
            // Purchase Items List Entry

            // Transaction Entry
            if (purchaseBill.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = purchaseBill.NetAmount;
                CF.PurchaseBillID = PurBillID;
                CF.CompanyID = CompID;
                //Cash In hand
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Purchase Bill Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = purchaseBill.NetAmount;
                CF1.PurchaseBillID = PurBillID;
                CF1.CompanyID = CompID;

                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Purchase Bill Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }
            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Credit = purchaseBill.NetAmount;
                CF2.PurchaseBillID = PurBillID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = purchaseBill.PartiesID;

                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;

                int PartMainAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.MainAccountID = PartMainAcc;
                CF2.SubAccountID = PartSubAcc;
                CF2.VoucherType = "Purchase Bill Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = purchaseBill.NetAmount;
                CF3.PurchaseBillID = PurBillID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = purchaseBill.PartiesID;

                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0002").FirstOrDefault().TransactionAccountID;

                CF3.TransactionAccountID = TraAccID;
                CF3.SubAccountID = SubAccID;
                CF3.MainAccountID = MainAccID;
                CF3.VoucherType = "Purchase Bill Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();


                if (TransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Debit = TransactionList.FirstOrDefault().Amount;
                    CF4.PurchaseBillID = PurBillID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = purchaseBill.PartiesID;

                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF4.MainAccountID = PartMainAcc1;
                    CF4.SubAccountID = PartSubAcc1;
                    CF4.TransactionAccountID = (int)PartyTransID_1;
                    CF4.VoucherType = "Purchase Bill Voucher";
                    CF4.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF4);
                    await _context.SaveChangesAsync();


                    if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cash) // Mode 0 Means ==> Cash
                    {
                        CashFlow CF5 = new CashFlow();
                        CF5.Credit = TransactionList.FirstOrDefault().Amount;
                        CF5.PurchaseBillID = PurBillID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = purchaseBill.PartiesID;
                        // Cash in hand
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID = SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Purchase Bill Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Credit = TransactionList.FirstOrDefault().Amount;
                        CF6.PurchaseBillID = PurBillID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = purchaseBill.PartiesID;
                        // Bank credit
                        int? bankID = TransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Purchase Bill Voucher";
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }

            }
            return RedirectToAction("Index");
        }

        private bool PurchaseBillExists(int id)
        {
            return _context.PurchaseBill.Any(e => e.PurchaseBillID == id);
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

            //  _context.PurchaseBillItem.Where(r => r.CompanyID == CompID).Where(p => p.Item.OEMNo == name).OrderByDescending(r => r.ItemID).Select(x => x.Item.SalePrice).FirstOrDefault();

            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.OEMNo == name).Include(r => r.PurchaseBillItem).Select(x => new { x.ProductDescription, x.ItemID, PurchasePrice = x.PurchaseBillItem.Where(r => r.PurchaseBillExtraBool == false).OrderByDescending(r => r.PurchaseBillID).FirstOrDefault() }).FirstOrDefault();

            return Json(a);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadDescriptionByCrossRef(string name)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.CrossRef == name).Include(r => r.PurchaseBillItem).Select(x => new { x.ProductDescription, x.ItemID, PurchasePrice = x.PurchaseBillItem.Where(r => r.PurchaseBillExtraBool == false).OrderByDescending(r => r.PurchaseBillID).FirstOrDefault() }).FirstOrDefault();

            return Json(a);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadItemID(string name)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            int a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.OEMNo == name).Select(p => p.ItemID).FirstOrDefault();
            return Json(a);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadPurchaseBills()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PB_B_1 == false).Where(r=>r.PurchaseImport == PurchaseType.Local).Include(r => r.Parties).OrderByDescending(x => x.PurchaseBillID).Select(x => new { x.PurchaseBillID, billDate = x.BillDate.ToString("d"), x.Balance, x.ContactPerson, x.GrossTotal, x.TDiscount, x.NetAmount, x.Parties.PartyName, x.Remarks, x.TotalQuantity, x.Parties.Phone1, x.Currency.CurrencyName, x.CreatedBy}).ToList();

            return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadCancelBills()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PB_B_1 == true).Include(r => r.Parties).Select(x => new { x.PurchaseBillID, billDate = x.BillDate.ToString("d"), x.Balance, x.ContactPerson, x.GrossTotal, x.TDiscount, x.NetAmount, x.Parties.PartyName, x.Remarks, x.TotalQuantity, x.Parties.Phone1, x.Currency.CurrencyName , x.CreatedBy}).OrderByDescending(r => r.PurchaseBillID).ToList();

            return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult ItemBriefDetails(int ItemID)
        {
            ProductLedgerVM model = new ProductLedgerVM();
            int? CompID = HttpContext.Session.GetInt32("CompanyID");

            var lst = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                            .Include(r => r.Item)
                            .Where(r => r.PurchaseBillExtraBool == false)
                            .Where(r => r.ItemID == ItemID)
                            .OrderByDescending(r => r.PurchaseBill.BillDate)
                            .ToList();

            model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                              .Where(r => r.ItemID == ItemID).Sum(r => r.Quantity);

            model.LastPurchasePrice = lst.FirstOrDefault().PurchasePrice;
            model.OemNo = lst.FirstOrDefault().Item.OEMNo;
            model.CrossRef = lst.FirstOrDefault().Item.CrossRef;

            int TotalRowsCount = lst.Count();
            model.TotalRowsCount = TotalRowsCount;

            decimal TotalQuantity = lst.Sum(r => r.Quantity);
            model.TotalQuantity = TotalQuantity;

            decimal TotalPrice = lst.Sum(r => r.PurchasePrice);
            TotalPrice = Math.Round(TotalPrice, 2);
            model.TotalPrice = TotalPrice;

            decimal AveragePrice = TotalPrice / TotalRowsCount;
            AveragePrice = Math.Round(AveragePrice, 2);
            model.AveragePrice = AveragePrice;

            return Json(new { data = model });
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Purchase Invoices Details")]
        public async Task<IActionResult> InvoicePrint(int? id)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            PurchaseBillVM model = new PurchaseBillVM();
            var purchaseBill = await _context.PurchaseBill.Include(r => r.Parties).SingleOrDefaultAsync(m => m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }
            if (_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == true)
            {
                model.CompanyName = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyName;
            }

            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;

            model.PB_S_1 = purchaseBill.BillDate.ToString("dd/MM/yyyy");
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            if (purchaseBill.Parties != null)
            {
                model.PartyName = purchaseBill.Parties.PartyName;
            }
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.PB_S_2 = purchaseBill.RefDate.ToString("dd/MM/yyyy");
            model.PB_B_1 = purchaseBill.PB_B_1;

            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();
            model.TransactionList = _context.Transaction.Where(r => r.PurchaseBillID == id).ToList();
            return View(model);
        }

        #region IMPORT

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Import Bill")]
        public IActionResult CreateImportBill()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (_context.PurchaseBill.Any() == true)
            {
                int BillNo = _context.PurchaseBill.LastOrDefault().PurchaseBillID;
                int BillNo_ADD = BillNo + 1;
                string BillNo_String = BillNo_ADD.ToString();
                ViewData["BillNo"] = BillNo_String;
            }
            else
            {
                ViewData["BillNo"] = "1";
            }
            ViewData["Ids"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName");
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Import Bill")]
        public async Task<IActionResult> CreateImportBill(PurchaseBill purchaseBill, IEnumerable<Item> ItemList, IEnumerable<Transaction> TransactionList)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main Purchase Bill
            purchaseBill.CreatedBy = User.Identity.Name;
            purchaseBill.CompanyID = CompID;
            purchaseBill.PurchaseImport = PurchaseType.ImportExport;

            decimal discountDeci = purchaseBill.GrossTotal - purchaseBill.NetAmount;
            purchaseBill.PB_D_1 = discountDeci;


            _context.PurchaseBill.Add(purchaseBill);
            await _context.SaveChangesAsync();

            // Main Purchase Bill            

            int PurBillID = purchaseBill.PurchaseBillID;

            // Purchase Items List Entry 
            var ItemList_Result = ItemList.ToList();
            ItemList_Result.RemoveAll(x => x.CompanyID == 777777);
            ItemList_Result.ForEach(x => x.CompanyID = CompID);
            foreach (var item in ItemList_Result)
            {
                PurchaseBillItem purBillItem = new PurchaseBillItem();

                purBillItem.ItemID = item.ItemID;
                purBillItem.PurchaseBillID = PurBillID;
                purBillItem.PurchasePrice = item.LCPrice;
                purBillItem.Quantity = item.Quantity;
                purBillItem.CompanyID = CompID;

                _context.PurchaseBillItem.Add(purBillItem);
                await _context.SaveChangesAsync();

                // Increase Quantity of Items in Stock (Item Table)
                Item itemObj = new Item();
                decimal Old_Quantity, New_Quantity;

                itemObj = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                Old_Quantity = itemObj.Quantity;
                New_Quantity = Old_Quantity + item.Quantity;

                itemObj.Quantity = New_Quantity;
                _context.Update(itemObj);
                await _context.SaveChangesAsync();
                // Increase Quantity of Items in Stock (Item Table)
            }
            // Purchase Items List Entry
            // Transaction Entry

            if (purchaseBill.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = purchaseBill.NetAmount;
                CF.PurchaseBillID = PurBillID;
                CF.CompanyID = CompID;
                //Cash In hand
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Import Bill Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = purchaseBill.NetAmount;
                CF1.PurchaseBillID = PurBillID;
                CF1.CompanyID = CompID;

                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Import Bill Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }
            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Credit = purchaseBill.NetAmount;
                CF2.PurchaseBillID = PurBillID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = purchaseBill.PartiesID;

                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;

                int PartMainAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.MainAccountID = PartMainAcc;
                CF2.SubAccountID = PartSubAcc;
                CF2.VoucherType = "Import Bill Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = purchaseBill.NetAmount;
                CF3.PurchaseBillID = PurBillID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = purchaseBill.PartiesID;

                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0002").FirstOrDefault().TransactionAccountID;

                CF3.TransactionAccountID = TraAccID;
                CF3.SubAccountID = SubAccID;
                CF3.MainAccountID = MainAccID;
                CF3.VoucherType = "Import Bill Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();


                if (TransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Debit = TransactionList.FirstOrDefault().Amount;
                    CF4.PurchaseBillID = PurBillID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = purchaseBill.PartiesID;

                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF4.MainAccountID = PartMainAcc1;
                    CF4.SubAccountID = PartSubAcc1;
                    CF4.TransactionAccountID = (int)PartyTransID_1;
                    CF4.VoucherType = "Import Bill Voucher";
                    CF4.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF4);
                    await _context.SaveChangesAsync();
                    
                    if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cash) // Mode 0 Means ==> Cash
                    {
                        CashFlow CF5 = new CashFlow();
                        CF5.Credit = TransactionList.FirstOrDefault().Amount;
                        CF5.PurchaseBillID = PurBillID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = purchaseBill.PartiesID;
                        // Cash in hand
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID = SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Import Bill Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Credit = TransactionList.FirstOrDefault().Amount;
                        CF6.PurchaseBillID = PurBillID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = purchaseBill.PartiesID;
                        // Bank credit
                        int? bankID = TransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Import Bill Voucher";
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
                Transaction transaction = new Transaction();
                transaction.CompanyID = CompID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.PurchaseBillID = PurBillID;
                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ImportDetails", "Purchase", new { id = PurBillID });
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Import Bills")]
        public async Task<IActionResult> EditImportBill(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }
            PurchaseBillVM model = new PurchaseBillVM();

            var purchaseBill = await _context.PurchaseBill.SingleOrDefaultAsync(m => m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }
            model.TLandingExpenses = purchaseBill.TLandingExpenses;
            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;
            model.BillDate = purchaseBill.BillDate;
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            model.PartiesID = purchaseBill.PartiesID;
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.RefDate = purchaseBill.RefDate;
            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PB_D_1 = purchaseBill.PB_D_1;
            model.PurchaseImport = purchaseBill.PurchaseImport;

            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();

            model.TransactionList = _context.Transaction.Where(r => r.PurchaseBillID == id).ToList();

            ViewData["Ids"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankID).ToList();
            ViewData["Name"] = _context.Bank.Where(r => r.CompanyID == CompID).Select(m => m.BankName).ToList();
            ViewData["BankID"] = new SelectList(_context.Bank.Where(r => r.CompanyID == CompID), "BankID", "BankName");
            ViewData["CurrencyID"] = new SelectList(_context.Currency.Where(r => r.CompanyID == CompID), "CurrencyID", "CurrencyName", purchaseBill.CurrencyID);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", purchaseBill.PartiesID);
            return View(model);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Import Bills")]
        public async Task<IActionResult> EditImportBill(PurchaseBill purchaseBill, IEnumerable<PurchaseBillItem> PurchaseBillItem_List, IEnumerable<Transaction> TransactionList)
        {
            int PurBillID = purchaseBill.PurchaseBillID;

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main Purchase Bill
            purchaseBill.CreatedBy = User.Identity.Name;
            purchaseBill.CompanyID = CompID;

            _context.Update(purchaseBill);
            await _context.SaveChangesAsync();

            _context.Transaction.RemoveRange(_context.Transaction.Where(x => x.PurchaseBillID == purchaseBill.PurchaseBillID));
            _context.PurchaseBillItem.RemoveRange(_context.PurchaseBillItem.Where(x => x.PurchaseBillID == purchaseBill.PurchaseBillID));
            _context.CashFlow.RemoveRange(_context.CashFlow.Where(x => x.PurchaseBillID == purchaseBill.PurchaseBillID));
            _context.SaveChanges();

            var TransactionList_Result = TransactionList.ToList();
            TransactionList_Result.RemoveAll(x => x.TransactionID == 777777);
            TransactionList_Result.ForEach(x => x.TransactionID = 0);
            foreach (var item in TransactionList_Result)
            {
                Transaction transaction = new Transaction();
                transaction.CompanyID = CompID;
                transaction.Amount = item.Amount;
                transaction.BankID = item.BankID;
                transaction.Cheque = item.Cheque;
                transaction.Date = item.Date;
                transaction.Mode = item.Mode;
                transaction.Paid = item.Paid;
                transaction.PurchaseBillID = PurBillID;
                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
            }

            //Purchase Items List Entry

            var ItemList_Result = PurchaseBillItem_List.ToList();
            ItemList_Result.RemoveAll(x => x.PurchaseBillExtraInt_1 == 777777);

            foreach (var item in ItemList_Result)
            {
                PurchaseBillItem purBillItem = new PurchaseBillItem();

                if (item.PurchaseBillExtraInt_2 != 0)
                {
                    // Decrease Quantity of Items in Stock (Item Table)
                    Item itemObj = new Item();
                    decimal OldBillQuantity, StockQuantity, final;

                    OldBillQuantity = item.PurchaseBillExtraInt; // Old Quantity

                    itemObj = _context.Item.Where(r => r.ItemID == item.PurchaseBillExtraInt_2).SingleOrDefault();
                    StockQuantity = itemObj.Quantity;
                    final = StockQuantity - OldBillQuantity;

                    itemObj.Quantity = final;
                    _context.Update(itemObj);
                    await _context.SaveChangesAsync();
                    // Decrease Quantity of Items in Stock (Item Table)
                }
                purBillItem.ItemID = item.ItemID;
                purBillItem.PurchaseBillID = PurBillID;
                purBillItem.PurchasePrice = item.PurchasePrice;
                purBillItem.Quantity = item.Quantity;
                purBillItem.CompanyID = CompID;

                _context.PurchaseBillItem.Add(purBillItem);
                await _context.SaveChangesAsync();

                // Increase Quantity of Items in Stock (Item Table)
                Item itemObj1 = new Item();
                decimal Old_Quantity, New_Quantity;

                itemObj1 = _context.Item.Where(r => r.ItemID == item.ItemID).SingleOrDefault();
                Old_Quantity = itemObj1.Quantity;
                New_Quantity = Old_Quantity + item.Quantity;

                itemObj1.Quantity = New_Quantity;
                _context.Update(itemObj1);
                await _context.SaveChangesAsync();
                // Increase Quantity of Items in Stock (Item Table)
            }
            // Purchase Items List Entry

            // Transaction Entry
            if (purchaseBill.PartiesID == null)
            {
                CashFlow CF = new CashFlow();
                CF.Credit = purchaseBill.NetAmount;
                CF.PurchaseBillID = PurBillID;
                CF.CompanyID = CompID;
                //Cash In hand
                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF.MainAccountID = MainAccID;
                CF.SubAccountID = SubAccID;
                CF.TransactionAccountID = TraAccID;
                CF.VoucherType = "Import Bill Voucher";
                CF.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF);
                await _context.SaveChangesAsync();

                CashFlow CF1 = new CashFlow();
                CF1.Debit = purchaseBill.NetAmount;
                CF1.PurchaseBillID = PurBillID;
                CF1.CompanyID = CompID;

                int MainAccID_1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID_1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_1).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID_1 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_1).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                CF1.MainAccountID = MainAccID_1;
                CF1.SubAccountID = SubAccID_1;
                CF1.TransactionAccountID = TraAccID_1;
                CF1.VoucherType = "Import Bill Voucher";
                CF1.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF1);
                await _context.SaveChangesAsync();
            }
            else
            {
                CashFlow CF2 = new CashFlow();
                CF2.Credit = purchaseBill.NetAmount;
                CF2.PurchaseBillID = PurBillID;
                CF2.CompanyID = CompID;
                CF2.PartiesID = purchaseBill.PartiesID;

                int? PartyTransID = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;

                int PartMainAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                int PartSubAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                CF2.TransactionAccountID = (int)PartyTransID;
                CF2.MainAccountID = PartMainAcc;
                CF2.SubAccountID = PartSubAcc;
                CF2.VoucherType = "Import Bill Voucher";
                CF2.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF2);
                await _context.SaveChangesAsync();

                CashFlow CF3 = new CashFlow();
                CF3.Debit = purchaseBill.NetAmount;
                CF3.PurchaseBillID = PurBillID;
                CF3.CompanyID = CompID;
                CF3.PartiesID = purchaseBill.PartiesID;

                int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
                int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0002").FirstOrDefault().TransactionAccountID;

                CF3.TransactionAccountID = TraAccID;
                CF3.SubAccountID = SubAccID;
                CF3.MainAccountID = MainAccID;
                CF3.VoucherType = "Import Bill Voucher";
                CF3.DateCreation = DateTime.Now.Date;

                _context.CashFlow.Add(CF3);
                await _context.SaveChangesAsync();


                if (TransactionList.FirstOrDefault().Paid == true)
                {
                    CashFlow CF4 = new CashFlow();
                    CF4.Debit = TransactionList.FirstOrDefault().Amount;
                    CF4.PurchaseBillID = PurBillID;
                    CF4.CompanyID = CompID;
                    CF4.PartiesID = purchaseBill.PartiesID;

                    int? PartyTransID_1 = _context.Parties.Where(r => r.CompanyID == CompID).Where(r => r.PartiesID == purchaseBill.PartiesID).FirstOrDefault().TransactionAccountID;
                    int PartMainAcc1 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
                    int PartSubAcc1 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                    CF4.MainAccountID = PartMainAcc1;
                    CF4.SubAccountID = PartSubAcc1;
                    CF4.TransactionAccountID = (int)PartyTransID_1;
                    CF4.VoucherType = "Import Bill Voucher";
                    CF4.DateCreation = DateTime.Now.Date;

                    _context.CashFlow.Add(CF4);
                    await _context.SaveChangesAsync();


                    if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cash) // Mode 0 Means ==> Cash
                    {
                        CashFlow CF5 = new CashFlow();
                        CF5.Credit = TransactionList.FirstOrDefault().Amount;
                        CF5.PurchaseBillID = PurBillID;
                        CF5.CompanyID = CompID;
                        CF5.PartiesID = purchaseBill.PartiesID;
                        // Cash in hand
                        int MainAccID_2 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_2 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_2).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
                        int TraAccID_2 = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID_2).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

                        CF5.MainAccountID = MainAccID_2;
                        CF5.SubAccountID = SubAccID_2;
                        CF5.TransactionAccountID = TraAccID_2;
                        CF5.VoucherType = "Import Bill Voucher";
                        CF5.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF5);
                        await _context.SaveChangesAsync();
                    }
                    else if (TransactionList.FirstOrDefault().Mode == PaymentMode.Cheque) // Mode 0 Means ==> Cheque
                    {
                        CashFlow CF6 = new CashFlow();
                        CF6.Credit = TransactionList.FirstOrDefault().Amount;
                        CF6.PurchaseBillID = PurBillID;
                        CF6.CompanyID = CompID;
                        CF6.PartiesID = purchaseBill.PartiesID;
                        // Bank credit
                        int? bankID = TransactionList.FirstOrDefault().BankID;
                        int transBankID = _context.Bank.Where(r => r.CompanyID == CompID).Where(r => r.BankID == bankID).FirstOrDefault().TransactionAccountID;
                        int MainAccID_3 = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
                        int SubAccID_3 = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID_3).Where(r => r.SubAccountNumber == "0002").FirstOrDefault().SubAccountID;

                        CF6.MainAccountID = MainAccID_3;
                        CF6.SubAccountID = SubAccID_3;
                        CF6.TransactionAccountID = transBankID;
                        CF6.VoucherType = "Import Bill Voucher";
                        CF6.DateCreation = DateTime.Now.Date;

                        _context.CashFlow.Add(CF6);
                        await _context.SaveChangesAsync();
                    }
                }

            }
            return RedirectToAction("ImportBills","Purchase",new { area="Purchase"});
        }


        [Route("[action]")]
        [Authorize]
        public JsonResult LoadVouchers(string BillNum)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            List<VoucherLoad> VoucherLoadList = new List<VoucherLoad>();
            var Cash = _context.CashPayment.Where(r => r.CompanyID == CompID).Where(r => r.ImportExportID == BillNum).ToList();
            var Cheque = _context.ChequePayment.Where(r => r.CompanyID == CompID).Where(r => r.ImportExportID == BillNum).ToList();
            var Voucher = _context.Voucher.Where(r => r.CompanyID == CompID).Where(r => r.ImportExportID == BillNum).Include(r=>r.VoucherItems).ToList();

            foreach(var item in Cash)
            {
                VoucherLoad Obj = new VoucherLoad();

                Obj.VoucherID = "Cash Payment Voucher "+item.CashPaymentID;
                Obj.Date = item.Date.ToString("dd/M/yyyy");                 
                Obj.Narration = item.Particulars;
                Obj.Amount = item.Amount;

                VoucherLoadList.Add(Obj);
            }

            foreach (var item in Cheque)
            {
                VoucherLoad Obj = new VoucherLoad();

                Obj.VoucherID = "Cheque Payment Voucher " + item.ChequePaymentID;
                Obj.Date = item.Date.ToString("dd/M/yyyy");
                Obj.Narration = item.Particulars;
                Obj.Amount = item.Amount;

                VoucherLoadList.Add(Obj);
            }

            foreach (var item in Voucher)
            {
                VoucherLoad Obj = new VoucherLoad();

                Obj.VoucherID = "Journal Voucher " + item.VoucherID;
                Obj.Date = item.Date.ToString("dd/M/yyyy");
                Obj.Narration = item.Particulars;
                Obj.Amount = item.VoucherItems.Sum(r=>r.Debit);

                VoucherLoadList.Add(Obj);
            }

            return Json(new { data = VoucherLoadList });
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Import Bills")]
        public IActionResult ImportBills()
        {
            return View();
        }
        
        [Route("[action]")]
        [Authorize]
        public JsonResult LoadImportBills()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PB_B_1 == false).Where(r => r.PurchaseImport == PurchaseType.ImportExport).Include(r => r.Parties).OrderByDescending(x => x.PurchaseBillID).Select(x => new { x.PurchaseBillID, billDate = x.BillDate.ToString("d"), x.Balance, x.ContactPerson, x.GrossTotal, x.TDiscount, x.NetAmount, x.Parties.PartyName, x.Remarks, x.TotalQuantity, x.Parties.Phone1, x.Currency.CurrencyName ,x.TLandingExpenses ,x.CreatedBy}).ToList();

            return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Import Bills Details")]
        public async Task<IActionResult> ImportDetails(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            PurchaseBillVM model = new PurchaseBillVM();
            var purchaseBill = await _context.PurchaseBill.Where(r=>r.CompanyID == CompID).Include(r => r.Parties).SingleOrDefaultAsync(m => m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }
            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;

            model.PB_S_1 = purchaseBill.BillDate.ToString("dd/MM/yyyy");
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            model.TLandingExpenses = purchaseBill.TLandingExpenses;

            if (purchaseBill.Parties != null)
            {
                model.PartyName = purchaseBill.Parties.PartyName;
            }
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.PB_S_2 = purchaseBill.RefDate.ToString("dd/MM/yyyy");
            model.PB_B_1 = purchaseBill.PB_B_1;

            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();

            string BillNum = id.ToString();
            List<VoucherLoad> VoucherLoadList = new List<VoucherLoad>();
            var Cash = _context.CashPayment.Where(r => r.CompanyID == CompID).Where(r => r.ImportExportID == BillNum).ToList();
            var Cheque = _context.ChequePayment.Where(r => r.CompanyID == CompID).Where(r => r.ImportExportID == BillNum).ToList();
            var Voucher = _context.Voucher.Where(r => r.CompanyID == CompID).Where(r => r.ImportExportID == BillNum).Include(r => r.VoucherItems).ToList();

            foreach (var item in Cash)
            {
                VoucherLoad Obj = new VoucherLoad();

                Obj.VoucherID = "Cash Payment Voucher " + item.CashPaymentID;
                Obj.Date = item.Date.ToString("dd/M/yyyy");
                Obj.Narration = item.Particulars;
                Obj.Amount = item.Amount;

                VoucherLoadList.Add(Obj);
            }

            foreach (var item in Cheque)
            {
                VoucherLoad Obj = new VoucherLoad();

                Obj.VoucherID = "Cheque Payment Voucher " + item.ChequePaymentID;
                Obj.Date = item.Date.ToString("dd/M/yyyy");
                Obj.Narration = item.Particulars;
                Obj.Amount = item.Amount;

                VoucherLoadList.Add(Obj);
            }

            foreach (var item in Voucher)
            {
                VoucherLoad Obj = new VoucherLoad();

                Obj.VoucherID = "Journal Voucher " + item.VoucherID;
                Obj.Date = item.Date.ToString("dd/M/yyyy");
                Obj.Narration = item.Particulars;
                Obj.Amount = item.VoucherItems.Sum(r => r.Debit);

                VoucherLoadList.Add(Obj);
            }

            model.VoucherLoadList = VoucherLoadList;

            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Import Bills Details")]
        public async Task<IActionResult> ImportPrint(int? id)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            PurchaseBillVM model = new PurchaseBillVM();
            var purchaseBill = await _context.PurchaseBill.Include(r => r.Parties).SingleOrDefaultAsync(m => m.PurchaseBillID == id);
            if (purchaseBill == null)
            {
                return NotFound();
            }
            if (_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == true)
            {
                model.CompanyName = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyName;
            }

            model.PurchaseBillID = purchaseBill.PurchaseBillID;
            model.Balance = purchaseBill.Balance;

            model.PB_S_1 = purchaseBill.BillDate.ToString("dd/MM/yyyy");
            model.CashPaid = purchaseBill.CashPaid;
            model.ContactNumber = purchaseBill.ContactNumber;
            model.ContactPerson = purchaseBill.ContactPerson;
            model.CreditDays = purchaseBill.CreditDays;
            model.CurrencyID = purchaseBill.CurrencyID;
            model.ExchangeRate = purchaseBill.ExchangeRate;
            model.ExternalRef = purchaseBill.ExternalRef;
            model.GrossTotal = purchaseBill.GrossTotal;
            model.NetAmount = purchaseBill.NetAmount;
            if (purchaseBill.Parties != null)
            {
                model.PartyName = purchaseBill.Parties.PartyName;
            }
            model.PayTerms = purchaseBill.PayTerms;
            model.PurchaseBillNo = purchaseBill.PurchaseBillID.ToString();
            model.PB_S_2 = purchaseBill.RefDate.ToString("dd/MM/yyyy");
            model.PB_B_1 = purchaseBill.PB_B_1;
            model.TLandingExpenses = purchaseBill.TLandingExpenses;

            model.Remarks = purchaseBill.Remarks;
            model.TDiscount = purchaseBill.TDiscount;
            model.TotalQuantity = purchaseBill.TotalQuantity;
            model.PurchaseBillItem_List = _context.PurchaseBillItem.Include(r => r.Item).Where(r => r.PurchaseBillID == id).ToList();
            model.TransactionList = _context.Transaction.Where(r => r.PurchaseBillID == id).ToList();
            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Canceled Import Bills")]
        public IActionResult CancelImports()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult LoadCanceImportBills()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PB_B_1 == true).Where(r=>r.PurchaseImport == PurchaseType.ImportExport).Include(r => r.Parties).Select(x => new { x.PurchaseBillID, billDate = x.BillDate.ToString("d"), x.Balance, x.ContactPerson, x.GrossTotal, x.TDiscount, x.NetAmount, x.Parties.PartyName, x.Remarks, x.TotalQuantity, x.Parties.Phone1, x.Currency.CurrencyName, x.TLandingExpenses , x.CreatedBy}).OrderByDescending(r => r.PurchaseBillID).ToList();

            return Json(new { data = data });
        }

        #endregion

    }
}