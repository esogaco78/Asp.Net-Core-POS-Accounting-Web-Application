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
using Invento.Areas.Payment.Models;
using Invento.Areas.Sale.Models;
using Invento.Areas.Purchase.Models;
using System.Globalization;

namespace Invento.Areas.Finance.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Finance")]
    [Route("Finance/[controller]")]
    public class LedgerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LedgerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Balance Sheet")]
        public IActionResult BalanceSheet(DateTime? DateFrom, DateTime? DateTo)
        {
            GeneralLedgerVM GL = new GeneralLedgerVM();
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            decimal income = 0;
            decimal expense = 0;

            int Income = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
            int Expenses = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
            int Assets = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
            int Liabilities = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "02").FirstOrDefault().MainAccountID;
            int Parties = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;

            decimal ExpensesOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r=>r.SubAccount.MainAccountID == Expenses).Sum(r => r.OpeningBalance);
            decimal IncomeOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == Income).Sum(r => r.OpeningBalance);
            decimal AssestsOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == Assets).Sum(r => r.OpeningBalance);
            decimal LiabilitiesOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == Liabilities).Sum(r => r.OpeningBalance);
            decimal PartiesOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == Parties).Sum(r => r.OpeningBalance);
            
            if (DateFrom == null && DateTo != null)
            {
                GL.LedgerList = (from r in _context.CashFlow
                                 where r.CompanyID == CompID                                 
                                 where r.DateCreation <= DateTo
                                 group r by r.TransactionAccountID into g
                                 select new LedgerListVM
                                 {
                                     TransactionAccountID = (int)g.Key,
                                     Balance_Credit_Minus = g.Sum(r => r.TransactionAccount.OpeningBalance),
                                     MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                     MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                     MainAccountName = g.Select(r => r.MainAccount.AccountName).FirstOrDefault(),
                                     Credit = g.Sum(x => x.Credit),
                                     Debit = g.Sum(x => x.Debit),
                                     Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                                 }).OrderBy(x => x.MainAccountNumber)
                              .ToList();
            }
            else
            {
                GL.LedgerList = (from r in _context.CashFlow
                                 where r.CompanyID == CompID
                                 where r.DateCreation >= DateFrom
                                 where r.DateCreation <= DateTo
                                 group r by r.TransactionAccountID into g
                                 select new LedgerListVM
                                 {
                                     TransactionAccountID = (int)g.Key,
                                     Balance_Credit_Minus = g.Sum(r => r.TransactionAccount.OpeningBalance),
                                     MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                     MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                     MainAccountName = g.Select(r => r.MainAccount.AccountName).FirstOrDefault(),
                                     Credit = g.Sum(x => x.Credit),
                                     Debit = g.Sum(x => x.Debit),
                                     Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                                 }).OrderBy(x => x.MainAccountNumber)
                              .ToList();
            }

            GL.ListPlus = GL.LedgerList.GroupBy(r => r.MainAccountID).Select(x => new PlusAllSubAccountsVM { ID = x.Key, TotalAdd = x.Sum(r => r.Balance) }).ToList();

            decimal PartyCredit = GL.LedgerList.GroupBy(r => r.MainAccountID).Select(x => x.Sum(r => r.Credit)).FirstOrDefault();
            decimal PartyDebit = GL.LedgerList.GroupBy(r => r.MainAccountID).Select(x => x.Sum(r => r.Debit)).FirstOrDefault();
            
            decimal AA = 0;
            if (PartiesOB < 0)
            {
                PartiesOB = PartiesOB * -1;
                AA = PartiesOB + PartyCredit;
                GL.PartyCreditors = AA;
                GL.PartyDebitors = PartyDebit;
            }
            else if(PartiesOB > 0)
            {                
                AA = PartiesOB + PartyDebit;
                GL.PartyDebitors = AA;
                GL.PartyCreditors = PartyCredit;
            }
            else 
            {
                GL.PartyDebitors = PartyDebit;
                GL.PartyCreditors = PartyCredit;
            }
            if (GL.ListPlus.Where(r => r.ID == Income).Any())
            {
                income = GL.ListPlus.Where(r => r.ID == Income).FirstOrDefault().TotalAdd;
                income = IncomeOB - income;
            }
            if (GL.ListPlus.Where(r => r.ID == Expenses).Any())
            {
                expense = GL.ListPlus.Where(r => r.ID == Expenses).FirstOrDefault().TotalAdd;
                expense = ExpensesOB -expense;
                expense = expense * -1;
            }           
            decimal ProfitLoss = income - expense;

            if(ProfitLoss > 0)
            {                
                GL.Profit = ProfitLoss;
            }
            else
            {
                ProfitLoss = ProfitLoss * -1;
                GL.Loss = ProfitLoss;
            }
            if(GL.ListPlus.Where(r => r.ID == Assets).Any())
            {
                decimal ass = GL.ListPlus.Where(r => r.ID == Assets).FirstOrDefault().TotalAdd;
                if(ass < 0)
                {
                    ass = ass * -1;
                    GL.AssetVal = AssestsOB + ass;
                }
                ass = AssestsOB + ass;
                GL.AssetVal = ass;
            }          
            if (GL.ListPlus.Where(r => r.ID == Liabilities).Any())
            {
                decimal lib = GL.ListPlus.Where(r => r.ID == Liabilities).FirstOrDefault().TotalAdd;
                decimal LibVAL = LiabilitiesOB - lib;
                if (LibVAL < 0)
                {                    
                    GL.LiabilityVal = LibVAL * -1;                    
                }                
                GL.LiabilityVal = LibVAL;
            }
            
            return View(GL);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Profit Loss")]
        public IActionResult ProfitLoss(DateTime? DateFrom, DateTime? DateTo)
        {
            GeneralLedgerVM GL = new GeneralLedgerVM();
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            int Income = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "03").FirstOrDefault().MainAccountID;
            int Expenses = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "04").FirstOrDefault().MainAccountID;
            GL.IncomeID = Income;
            GL.ExpenseID = Expenses;
            decimal OB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Sum(r => r.OpeningBalance);

            decimal ExpensesOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == Expenses).Sum(r => r.OpeningBalance);
            decimal IncomeOB = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == Income).Sum(r => r.OpeningBalance);
            
            if(DateFrom != null && DateTo != null)
            {            
                GL.LedgerList = (from r in _context.CashFlow
                             where r.CompanyID == CompID
                             where r.DateCreation >= DateFrom
                             where r.DateCreation <= DateTo
                             group r by r.TransactionAccountID into g
                             select new LedgerListVM
                             {
                                 TransactionAccountID = (int)g.Key,
                                 MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),                                 
                                 MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                 MainAccountName = g.Select(r => r.MainAccount.AccountName).FirstOrDefault(),                                 
                                 Credit = g.Sum(x => x.Credit),
                                 Debit = g.Sum(x => x.Debit),
                                 Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                             }).OrderBy(x => x.MainAccountNumber)
                              .ToList();
            
                GL.ListPlus = GL.LedgerList.GroupBy(r => r.MainAccountID).Select(x => new PlusAllSubAccountsVM { ID = x.Key, TotalAdd = x.Sum(r => r.Balance) }).ToList();
                if(GL.ListPlus.Where(r => r.ID == Income).Any())
                {
                    decimal inc = GL.ListPlus.Where(r => r.ID == Income).FirstOrDefault().TotalAdd;
                    GL.IncomeVal = IncomeOB - inc;                
                }
                if(GL.ListPlus.Where(r => r.ID == Expenses).Any())
                {
                    decimal exp = GL.ListPlus.Where(r => r.ID == Expenses).FirstOrDefault().TotalAdd;
                    decimal aa = ExpensesOB - exp;
                    GL.ExpenseVal = aa * -1; 
                }
            }
           
            return View(GL);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Notes to the Accounts")]
        public IActionResult AccountNotes(DateTime? DateFrom, DateTime? DateTo)
        {
            GeneralLedgerVM GL = new GeneralLedgerVM();
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);            

            if(_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == true)
            {
                GL.AccountName = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyName;
            }
            
            if (DateFrom != null && DateTo != null)
            {
                GL.LedgerList = (from r in _context.CashFlow
                                 where r.CompanyID == CompID
                                 where r.DateCreation >= DateFrom
                                 where r.DateCreation <= DateTo
                                 group r by r.TransactionAccountID into g
                                 select new LedgerListVM
                                 {
                                     TransactionAccountID = (int)g.Key,
                                     MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                     SubAccountNumber = g.Select(r => r.SubAccount.SubAccountNumber).FirstOrDefault(),
                                     SubAccountName = g.Select(r => r.SubAccount.AccountName).FirstOrDefault(),
                                     TransactionAccountNumber = g.Select(r => r.TransactionAccount.TransactionAccountNumber).FirstOrDefault(),
                                     TransactionAccountName = g.Select(r => r.TransactionAccount.AccountName).FirstOrDefault(),
                                     MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                     MainAccountName = g.Select(r => r.MainAccount.AccountName).FirstOrDefault(),
                                     Balance_Credit_Minus = g.Select(r => r.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                     Credit = g.Sum(x => x.Credit),
                                     Debit = g.Sum(x => x.Debit),
                                     Balance = g.Select(r => r.TransactionAccount.OpeningBalance).FirstOrDefault() + g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                                 }).OrderBy(x => x.MainAccountNumber)
                              .ToList();

                GL.ListPlus = GL.LedgerList.GroupBy(r => r.MainAccountID).Select(x => new PlusAllSubAccountsVM { ID = x.Key, TotalAdd = x.Sum(r => r.Balance) }).ToList();
            }
            GL.DateFrom = DateFrom;
            GL.DateTo = DateTo;
            return View(GL);
        }
 
        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Trial Balance")]
        public IActionResult TrialBalance(string RBtn, DateTime? DateFrom, DateTime? DateTo)
        {
            GeneralLedgerVM GL = new GeneralLedgerVM();
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
           
            if(DateFrom != null && DateTo != null)
            {
                if (RBtn == "Transaction")
                {
                    var aa = from r in _context.TransactionAccount
                             where r.CompanyID == CompID                             
                             group r by r.TransactionAccountID into g
                             select new AA
                             {
                                 id = g.Key,
                                 subID = g.Select(r => r.SubAccountID).FirstOrDefault(),
                                 mainID = g.Select(r => r.SubAccount.MainAccountID).FirstOrDefault(),
                                 OpeningBalance = g.Sum(r => r.OpeningBalance)
                             };
                    List<AA> z = new List<AA>();
                    z = aa.ToList();
                    List<TransactionAccount> E = new List<TransactionAccount>();
                    E = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r => r.SubAccount).Include(r => r.SubAccount.MainAccount).ToList();

                    List<LedgerListVM> tempList = new List<LedgerListVM>();
                    tempList = (from r in _context.CashFlow
                                where r.CompanyID == CompID
                                where r.DateCreation >= DateFrom
                                where r.DateCreation <= DateTo
                                join s in _context.SubAccount on r.SubAccountID equals s.SubAccountID
                                join m in _context.TransactionAccount on r.TransactionAccountID equals m.TransactionAccountID
                                join k in _context.MainAccount on r.MainAccountID equals k.MainAccountID
                                group r by r.TransactionAccountID into g
                                select new LedgerListVM
                                {
                                    TransactionAccountID = (int)g.Key,
                                    MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                    MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                    SubAccountID = (int)g.Select(r => r.SubAccountID).FirstOrDefault(),
                                    SubAccountNumber = g.Select(r => r.SubAccount.SubAccountNumber).FirstOrDefault(),
                                    TransactionAccountName = g.Select(r => r.TransactionAccount.AccountName).FirstOrDefault(),
                                    TransactionAccountNumber = g.Select(r => r.TransactionAccount.TransactionAccountNumber).FirstOrDefault(),
                                    Credit = g.Sum(x => x.Credit),
                                    Debit = g.Sum(x => x.Debit),
                                })
                                   .ToList();
                    List<LedgerListVM> Second = new List<LedgerListVM>();

                    for (int i = 0; i < z.Count; i++)
                    {
                        for (int j = 0; j < tempList.Count; j++)
                        {
                            if (z.ElementAt(i).id == tempList.ElementAt(j).TransactionAccountID)
                            {
                                decimal openbalance = 0;
                                LedgerListVM obj = new LedgerListVM();
                                obj.TransactionAccountID = tempList.ElementAt(j).TransactionAccountID;
                                obj.MainAccountNumber = tempList.ElementAt(j).MainAccountNumber;
                                obj.SubAccountNumber = tempList.ElementAt(j).SubAccountNumber;
                                obj.TransactionAccountName = tempList.ElementAt(j).TransactionAccountName;
                                obj.TransactionAccountNumber = tempList.ElementAt(j).TransactionAccountNumber;
                                obj.Credit = tempList.ElementAt(j).Credit;
                                obj.Debit = tempList.ElementAt(j).Debit;

                                openbalance = openbalance + z.ElementAt(i).OpeningBalance;
                                obj.Balance = openbalance + obj.Debit - obj.Credit;
                                Second.Add(obj);
                            }
                        }
                    }
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        E.RemoveAll(r => r.TransactionAccountID == tempList.ElementAt(i).TransactionAccountID);
                    }

                    for (int j = 0; j < E.Count; j++)
                    {
                        LedgerListVM obj = new LedgerListVM();
                        obj.MainAccountNumber = E.ElementAt(j).SubAccount.MainAccount.MainAccountNumber;
                        obj.SubAccountNumber = E.ElementAt(j).SubAccount.SubAccountNumber;
                        obj.TransactionAccountName = E.ElementAt(j).AccountName;
                        obj.TransactionAccountNumber = E.ElementAt(j).TransactionAccountNumber;
                        obj.Balance = E.ElementAt(j).OpeningBalance;
                        Second.Add(obj);
                    }
                    Second.RemoveAll(r => r.Balance == 0);
                    GL.LedgerList = Second.OrderBy(r => r.MainAccountNumber).ThenBy(r => r.SubAccountNumber).ThenBy(r => r.TransactionAccountNumber).ToList();
                }
                else if (RBtn == "Sub")
                {
                    var aa = from r in _context.TransactionAccount
                             where r.CompanyID == CompID
                             group r by r.SubAccountID into g
                             select new AA
                             {
                                 id = g.Key,
                                 subID = g.Select(r => r.SubAccountID).FirstOrDefault(),
                                 mainID = g.Select(r => r.SubAccount.MainAccountID).FirstOrDefault(),
                                 OpeningBalance = g.Sum(r => r.OpeningBalance)
                             };
                    List<AA> z = new List<AA>();
                    z = aa.ToList();
                    List<LedgerListVM> tempList = new List<LedgerListVM>();
                    tempList = (from r in _context.CashFlow
                                where r.CompanyID == CompID
                                where r.DateCreation >= DateFrom
                                where r.DateCreation <= DateTo
                                group r by r.SubAccountID into g
                                select new LedgerListVM
                                {
                                    SubAccountID = (int)g.Key,
                                    MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                    MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                    TransactionAccountID = (int)g.Select(r => r.TransactionAccountID).FirstOrDefault(),
                                    SubAccountNumber = g.Select(r => r.SubAccount.SubAccountNumber).FirstOrDefault(),
                                    TransactionAccountName = g.Select(r => r.SubAccount.AccountName).FirstOrDefault(),
                                    Credit = g.Sum(x => x.Credit),
                                    Debit = g.Sum(x => x.Debit),
                                    //Balance = g.Where(r => r.SubAccountID == (int)g.Key).Sum(r => r.TransactionAccount.OpeningBalance)  + g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                                })
                                   .ToList();
                    List<LedgerListVM> Second = new List<LedgerListVM>();
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        decimal openbalance = 0;
                        LedgerListVM obj = new LedgerListVM();
                        obj.MainAccountNumber = tempList.ElementAt(i).MainAccountNumber;
                        obj.SubAccountNumber = tempList.ElementAt(i).SubAccountNumber;
                        obj.TransactionAccountName = tempList.ElementAt(i).TransactionAccountName;
                        obj.Credit = tempList.ElementAt(i).Credit;
                        obj.Debit = tempList.ElementAt(i).Debit;
                        for (int j = 0; j < z.Count; j++)
                        {
                            if (z.ElementAt(j).subID == tempList.ElementAt(i).SubAccountID)
                            {
                                openbalance = openbalance + z.ElementAt(j).OpeningBalance;
                            }
                        }
                        obj.Balance = openbalance + obj.Debit - obj.Credit;
                        Second.Add(obj);
                    }
                    GL.LedgerList = Second.OrderBy(r => r.MainAccountNumber).ThenBy(r => r.SubAccountNumber).ToList();
                }
                else
                {
                    var aa = from r in _context.TransactionAccount
                             where r.CompanyID == CompID
                             group r by r.SubAccount.MainAccountID into g
                             select new AA
                             {
                                 id = g.Key,
                                 subID = g.Select(r => r.SubAccount.MainAccountID).FirstOrDefault(),
                                 OpeningBalance = g.Sum(r => r.OpeningBalance)
                             };
                    List<AA> z = new List<AA>();
                    z = aa.ToList();
                    List<LedgerListVM> tempList = new List<LedgerListVM>();

                    tempList = (from r in _context.CashFlow
                                where r.CompanyID == CompID
                                where r.DateCreation >= DateFrom
                                where r.DateCreation <= DateTo
                                group r by r.MainAccountID into g
                                select new LedgerListVM
                                {
                                    MainAccountID = (int)g.Key,
                                    MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                    TransactionAccountName = g.Select(r => r.MainAccount.AccountName).FirstOrDefault(),
                                    Balance_Credit_Minus = g.Select(r => r.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                    Credit = g.Sum(x => x.Credit),
                                    Debit = g.Sum(x => x.Debit),
                                })
                                    .ToList();

                    List<LedgerListVM> Second = new List<LedgerListVM>();
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        decimal openbalance = 0;
                        LedgerListVM obj = new LedgerListVM();
                        obj.MainAccountNumber = tempList.ElementAt(i).MainAccountNumber;
                        obj.SubAccountNumber = tempList.ElementAt(i).SubAccountNumber;
                        obj.TransactionAccountName = tempList.ElementAt(i).TransactionAccountName;
                        obj.Credit = tempList.ElementAt(i).Credit;
                        obj.Debit = tempList.ElementAt(i).Debit;
                        for (int j = 0; j < z.Count; j++)
                        {
                            if (z.ElementAt(j).subID == tempList.ElementAt(i).MainAccountID)
                            {
                                openbalance = z.ElementAt(j).OpeningBalance;
                            }
                        }
                        obj.Balance = openbalance + obj.Debit - obj.Credit;
                        Second.Add(obj);
                    }
                    GL.LedgerList = Second;

                }
            }
            GL.AccountName = RBtn;
            return View(GL);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Cash Book")]
        public IActionResult CashBook(DateTime? DateFrom, DateTime? DateTo)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);            

            int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
            int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
            int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

            GeneralLedgerVM GL = new GeneralLedgerVM();

            string MainAccuntNumber = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).FirstOrDefault().MainAccountNumber;
            string SubAccuntNumber = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).FirstOrDefault().SubAccountNumber;
            string TranAccuntNumber = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TraAccID).FirstOrDefault().TransactionAccountNumber;
            GL.AccountNumber = MainAccuntNumber + " - " + SubAccuntNumber + " - " + TranAccuntNumber;

            string MainAccuntName = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).FirstOrDefault().AccountName;
            string SubAccuntName = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).FirstOrDefault().AccountName;
            string TranAccuntName = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TraAccID).FirstOrDefault().AccountName;
            GL.AccountName = MainAccuntName + " - " + SubAccuntName + " - " + TranAccuntName;            

            if (DateFrom != null && DateTo != null)
            {

                GL.CashBook_DR = (from r in _context.CashFlow
                                 where r.CompanyID == CompID
                                 where r.TransactionAccountID == TraAccID
                                 where r.DateCreation >= DateFrom
                                 where r.DateCreation <= DateTo
                                 where r.Debit != 0
                                 select new LedgerListVM
                                 {                                   
                                     Narration_1 = r.CashReceipt.Particulars,
                                     Voucher_1 = r.CashReceipt.CashReceiptID,
                                     Date_1 = r.CashReceipt.Date,
                                     
                                     Narration_2 = r.PurchaseReturn.Remarks,
                                     Voucher_2 = r.PurchaseReturn.PurchaseReturnID,
                                     Date_2 = r.PurchaseReturn.PurBillReturnDate,

                                     Narration_3 = r.SaleBill.Remarks,
                                     Voucher_3 = r.SaleBill.SaleBillID,
                                     Date_3 = r.SaleBill.BillDate,

                                     Narration_4 = r.VoucherItems.Narration,
                                     Voucher_4 = r.VoucherItems.VoucherID,
                                     Date_4 = r.VoucherItems.Voucher.Date,
                                     
                                     Debit = r.Debit
                                 }).ToList();


                GL.CashBook_CR = (from r in _context.CashFlow
                                  where r.CompanyID == CompID
                                  where r.TransactionAccountID == TraAccID
                                  where r.DateCreation >= DateFrom
                                  where r.DateCreation <= DateTo
                                  where r.Credit != 0
                                  select new LedgerListVM
                                  {
                                      Narration_1 = r.CashPayment.Particulars,
                                      Voucher_1 = r.CashPayment.CashPaymentID,
                                      Date_1 = r.CashPayment.Date,

                                      Narration_2 = r.SaleReturn.Remarks,
                                      Voucher_2 = r.SaleReturn.SaleReturnID,
                                      Date_2 = r.SaleReturn.SaleBillReturnDate,

                                      Narration_3 = r.PurchaseBill.Remarks,
                                      Voucher_3 = r.PurchaseBill.PurchaseBillID,
                                      Date_3 = r.PurchaseBill.BillDate,
                                      
                                      Narration_4 = r.VoucherItems.Narration,
                                      Voucher_4 = r.VoucherItems.VoucherID,
                                      Date_4 = r.VoucherItems.Voucher.Date,

                                      Credit = r.Credit                                      
                                  }).ToList();
            }
            return View(GL);
        }
         
        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,General Ledger")]
        public IActionResult GeneralLedger(int? MainAccount, int? SubAccount, int? TranAccount, DateTime? DateFrom , DateTime? DateTo)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);            

            GeneralLedgerVM GL = new GeneralLedgerVM();
            
            if (DateFrom != null && DateTo != null)
            {
                if (MainAccount != null && SubAccount == null && TranAccount == null)
                {
                    string MainAccuntNumber = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).FirstOrDefault().MainAccountNumber;
                    GL.AccountNumber = MainAccuntNumber;

                    string MainAccuntName = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).FirstOrDefault().AccountName;
                    GL.AccountName = MainAccuntName;

                    GL.LedgerList = (from r in _context.CashFlow
                                     where r.CompanyID == CompID
                                     where r.MainAccountID == MainAccount
                                     where r.DateCreation >= DateFrom
                                     where r.DateCreation <= DateTo
                                     select new LedgerListVM
                                     {
                                         MainAccountNumber = r.MainAccount.MainAccountNumber,
                                         SubAccountNumber = r.SubAccount.SubAccountNumber,
                                         TransactionAccountNumber = r.TransactionAccount.TransactionAccountNumber,
                                         TransactionAccountName = r.TransactionAccount.AccountName,

                                         //Narration_1 = r.CashInBank.Particulars,
                                         Narration_2 = r.CashPayment.Particulars,
                                         Narration_3 = r.CashReceipt.Particulars,
                                         //Narration_4 = r.ChequePayment.Particulars,
                                         //Narration_5 = r.ChequeReceipt.Particulars,
                                         Narration_6 = r.PurchaseBill.Remarks,
                                         Narration_7 = r.PurchaseReturn.Remarks,
                                         Narration_8 = r.SaleBill.Remarks,
                                         Narration_9 = r.SaleReturn.Remarks,
                                         Narration_10 = r.VoucherItems.Narration,

                                         Credit = r.Credit,
                                         Debit = r.Debit
                                     }).OrderBy(x => x.MainAccountNumber).ThenBy(r => r.SubAccountNumber).ThenBy(r => r.TransactionAccountNumber)
                                     .ToList();

                    List<SubAccount> SubList = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).ToList();
                    List<TransactionAccount> TransList = new List<TransactionAccount>();

                    decimal balanceAmount = 0;
                    balanceAmount = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccount.MainAccountID == MainAccount).Sum(r => r.OpeningBalance);
                    GL.OpeningBalance = balanceAmount;
                }
                else if (MainAccount != null && SubAccount != null && TranAccount == null)
                {
                    string MainAccuntNumber = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).FirstOrDefault().MainAccountNumber;
                    string SubAccuntNumber = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccount).FirstOrDefault().SubAccountNumber;
                    GL.AccountNumber = MainAccuntNumber + " - " + SubAccuntNumber;

                    string MainAccuntName = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).FirstOrDefault().AccountName;
                    string SubAccuntName = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccount).FirstOrDefault().AccountName;
                    GL.AccountName = MainAccuntName + " - " + SubAccuntName;

                    decimal balanceAmount = 0;
                    balanceAmount = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r => r.SubAccount).Include(r => r.SubAccount.MainAccount).Where(r => r.SubAccount.MainAccountID == MainAccount).Sum(r => r.OpeningBalance);
                    GL.OpeningBalance = balanceAmount;

                    GL.LedgerList = (from r in _context.CashFlow
                                     where r.CompanyID == CompID
                                     where r.SubAccountID == SubAccount
                                     where r.MainAccountID == MainAccount
                                     where r.DateCreation >= DateFrom
                                     where r.DateCreation <= DateTo
                                     select new LedgerListVM
                                     {
                                         MainAccountNumber = r.MainAccount.MainAccountNumber,
                                         SubAccountNumber = r.SubAccount.SubAccountNumber,
                                         TransactionAccountNumber = r.TransactionAccount.TransactionAccountNumber,
                                         TransactionAccountName = r.TransactionAccount.AccountName,

                                         //Narration_1 = r.CashInBank.Particulars,
                                         Narration_2 = r.CashPayment.Particulars,
                                         Narration_3 = r.CashReceipt.Particulars,
                                         //Narration_4 = r.ChequePayment.Particulars,
                                         //Narration_5 = r.ChequeReceipt.Particulars,
                                         Narration_6 = r.PurchaseBill.Remarks,
                                         Narration_7 = r.PurchaseReturn.Remarks,
                                         Narration_8 = r.SaleBill.Remarks,
                                         Narration_9 = r.SaleReturn.Remarks,
                                         Narration_10 = r.VoucherItems.Narration,

                                         Credit = r.Credit,
                                         Debit = r.Debit
                                     }).OrderBy(x => x.MainAccountNumber).ThenBy(r => r.SubAccountNumber)
                                     .ToList();

                    GL.OpeningBalance = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccount).Sum(r => r.OpeningBalance);
                }
                else if (MainAccount != null && SubAccount != null && TranAccount != null)
                {
                    string MainAccuntNumber = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).FirstOrDefault().MainAccountNumber;
                    string SubAccuntNumber = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccount).FirstOrDefault().SubAccountNumber;
                    string TranAccuntNumber = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TranAccount).FirstOrDefault().TransactionAccountNumber;
                    GL.AccountNumber = MainAccuntNumber + " - " + SubAccuntNumber + " - " + TranAccuntNumber;

                    string MainAccuntName = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccount).FirstOrDefault().AccountName;
                    string SubAccuntName = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccount).FirstOrDefault().AccountName;
                    string TranAccuntName = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TranAccount).FirstOrDefault().AccountName;
                    GL.AccountName = MainAccuntName + " - " + SubAccuntName + " - " + TranAccuntName;
                    GL.Check_GL_Level = true;

                    GL.LedgerList = (from r in _context.CashFlow
                                     where r.CompanyID == CompID
                                     where r.MainAccountID == MainAccount
                                     where r.SubAccountID == SubAccount
                                     where r.TransactionAccountID == TranAccount
                                     where r.DateCreation >= DateFrom
                                     where r.DateCreation <= DateTo
                                     select new LedgerListVM
                                     {
                                         MainAccountNumber = r.MainAccount.MainAccountNumber,
                                         SubAccountNumber = r.SubAccount.SubAccountNumber,
                                         TransactionAccountNumber = r.TransactionAccount.TransactionAccountNumber,
                                         TransactionAccountName = r.TransactionAccount.AccountName,

                                         //Narration_1 = r.CashInBank.Particulars,
                                         //Voucher_1 = r.CashInBank.CashInBankID,
                                         //Date_1 = r.CashInBank.Date,

                                         Narration_2 = r.CashPayment.Particulars,
                                         Voucher_2 = r.CashPayment.CashPaymentID,
                                         Date_2 = r.CashPayment.Date,

                                         Narration_3 = r.CashReceipt.Particulars,
                                         Voucher_3 = r.CashReceipt.CashReceiptID,
                                         Date_3 = r.CashReceipt.Date,

                                         //Narration_4 = r.ChequePayment.Particulars,
                                         //Voucher_4 = r.ChequePayment.ChequePaymentID,
                                         //Date_4 = r.ChequePayment.DateOfMature,
                                        
                                         //Narration_5 = r.ChequeReceipt.Particulars,
                                         //Voucher_5 = r.ChequeReceipt.ChequeReceiptID,
                                         //Date_5 = r.ChequeReceipt.DateOfMature,

                                         Narration_6 = r.PurchaseBill.Remarks,
                                         Voucher_6 = r.PurchaseBill.PurchaseBillID,
                                         Date_6 = r.PurchaseBill.BillDate,

                                         Narration_7 = r.PurchaseReturn.Remarks,
                                         Voucher_7 = r.PurchaseReturn.PurchaseReturnID,
                                         Date_7 = r.PurchaseReturn.PurBillReturnDate,

                                         Narration_8 = r.SaleBill.Remarks,
                                         Voucher_8 = r.SaleBill.SaleBillID,
                                         Date_8 = r.SaleBill.BillDate,

                                         Narration_9 = r.SaleReturn.Remarks,
                                         Voucher_9 = r.SaleReturn.SaleReturnID,
                                         Date_9 = r.SaleReturn.SaleBillReturnDate,

                                         Narration_10 = r.VoucherItems.Narration,
                                         Voucher_10 = r.VoucherItems.VoucherID,
                                         Date_10 = r.VoucherItems.Voucher.Date,
                                         
                                         Credit = r.Credit,
                                         Debit = r.Debit
                                     }).OrderBy(x => x.MainAccountNumber).ThenBy(r => r.SubAccountNumber).ThenBy(r => r.TransactionAccountNumber)
                                    .ToList();

                    GL.OpeningBalance = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TranAccount).Sum(r => r.OpeningBalance);
                }
                else
                {
                    var aa = from r in _context.TransactionAccount
                             where r.CompanyID == CompID
                             group r by r.TransactionAccountID into g
                             select new AA
                             {
                                 id = g.Key,
                                 subID = g.Select(r => r.SubAccountID).FirstOrDefault(),
                                 mainID = g.Select(r => r.SubAccount.MainAccountID).FirstOrDefault(),
                                 OpeningBalance = g.Sum(r => r.OpeningBalance)
                             };
                    List<AA> z = new List<AA>();
                    z = aa.ToList();
                    List<TransactionAccount> E = new List<TransactionAccount>();
                    E = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Include(r => r.SubAccount).Include(r => r.SubAccount.MainAccount).ToList();

                    List<LedgerListVM> tempList = new List<LedgerListVM>();
                    tempList = (from r in _context.CashFlow
                                where r.CompanyID == CompID
                                join s in _context.SubAccount on r.SubAccountID equals s.SubAccountID
                                join m in _context.TransactionAccount on r.TransactionAccountID equals m.TransactionAccountID
                                join k in _context.MainAccount on r.MainAccountID equals k.MainAccountID
                                where r.DateCreation >= DateFrom
                                where r.DateCreation <= DateTo
                                group r by r.TransactionAccountID into g
                                select new LedgerListVM
                                {
                                    TransactionAccountID = (int)g.Key,
                                    MainAccountID = (int)g.Select(r => r.MainAccountID).FirstOrDefault(),
                                    MainAccountNumber = g.Select(r => r.MainAccount.MainAccountNumber).FirstOrDefault(),
                                    SubAccountID = (int)g.Select(r => r.SubAccountID).FirstOrDefault(),
                                    SubAccountNumber = g.Select(r => r.SubAccount.SubAccountNumber).FirstOrDefault(),
                                    TransactionAccountName = g.Select(r => r.TransactionAccount.AccountName).FirstOrDefault(),
                                    TransactionAccountNumber = g.Select(r => r.TransactionAccount.TransactionAccountNumber).FirstOrDefault(),

                                    Narration_1 = g.Select(r => r.CashInBank.Particulars).FirstOrDefault(),
                                    Narration_2 = g.Select(r => r.CashPayment.Particulars).FirstOrDefault(),
                                    Narration_3 = g.Select(r => r.CashReceipt.Particulars).FirstOrDefault(),
                                    Narration_4 = g.Select(r => r.ChequePayment.Particulars).FirstOrDefault(),
                                    Narration_5 = g.Select(r => r.ChequeReceipt.Particulars).FirstOrDefault(),
                                    Narration_6 = g.Select(r => r.PurchaseBill.Remarks).FirstOrDefault(),
                                    Narration_7 = g.Select(r => r.PurchaseReturn.Remarks).FirstOrDefault(),
                                    Narration_8 = g.Select(r => r.SaleBill.Remarks).FirstOrDefault(),
                                    Narration_9 = g.Select(r => r.SaleReturn.Remarks).FirstOrDefault(),
                                    Narration_10 = g.Select(r => r.VoucherItems.Narration).FirstOrDefault(),


                                    Credit = g.Sum(x => x.Credit),
                                    Debit = g.Sum(x => x.Debit),
                                })
                                   .ToList();
                    List<LedgerListVM> Second = new List<LedgerListVM>();

                    for (int i = 0; i < z.Count; i++)
                    {
                        for (int j = 0; j < tempList.Count; j++)
                        {
                            if (z.ElementAt(i).id == tempList.ElementAt(j).TransactionAccountID)
                            {
                                LedgerListVM obj = new LedgerListVM();
                                obj.TransactionAccountID = tempList.ElementAt(j).TransactionAccountID;
                                obj.MainAccountNumber = tempList.ElementAt(j).MainAccountNumber;
                                obj.SubAccountNumber = tempList.ElementAt(j).SubAccountNumber;
                                obj.TransactionAccountName = tempList.ElementAt(j).TransactionAccountName;
                                obj.TransactionAccountNumber = tempList.ElementAt(j).TransactionAccountNumber;

                                //obj.Narration_1 = tempList.ElementAt(j).Narration_1;
                                //obj.Narration_2 = tempList.ElementAt(j).Narration_2;
                                //obj.Narration_3 = tempList.ElementAt(j).Narration_3;
                                //obj.Narration_4 = tempList.ElementAt(j).Narration_4;
                                //obj.Narration_5 = tempList.ElementAt(j).Narration_5;
                                //obj.Narration_6 = tempList.ElementAt(j).Narration_6;
                                //obj.Narration_7 = tempList.ElementAt(j).Narration_7;
                                //obj.Narration_8 = tempList.ElementAt(j).Narration_8;
                                //obj.Narration_9 = tempList.ElementAt(j).Narration_9;
                                //obj.Narration_10 = tempList.ElementAt(j).Narration_10;

                                decimal Bal = z.ElementAt(i).OpeningBalance + tempList.ElementAt(j).Debit - tempList.ElementAt(j).Credit;
                                if (Bal > 0)
                                {
                                    obj.Debit = Bal;
                                }
                                else
                                {
                                    Bal = Bal * -1;
                                    obj.Credit = Bal;
                                }

                                Second.Add(obj);
                            }
                        }
                    }
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        E.RemoveAll(r => r.TransactionAccountID == tempList.ElementAt(i).TransactionAccountID);
                    }

                    for (int j = 0; j < E.Count; j++)
                    {
                        LedgerListVM obj = new LedgerListVM();
                        obj.MainAccountNumber = E.ElementAt(j).SubAccount.MainAccount.MainAccountNumber;
                        obj.SubAccountNumber = E.ElementAt(j).SubAccount.SubAccountNumber;
                        obj.TransactionAccountName = E.ElementAt(j).AccountName;
                        obj.TransactionAccountNumber = E.ElementAt(j).TransactionAccountNumber;
                        obj.Balance = E.ElementAt(j).OpeningBalance;
                        if (E.ElementAt(j).OpeningBalance > 0)
                        {
                            obj.Debit = E.ElementAt(j).OpeningBalance;
                        }
                        else
                        {
                            decimal ab = E.ElementAt(j).OpeningBalance;
                            ab = ab * -1;
                            obj.Credit = ab;
                        }
                        Second.Add(obj);
                    }
                    for (int i = 0; i < Second.Count; i++)
                    {
                        if (Second.ElementAt(i).Credit == 0 && Second.ElementAt(i).Debit == 0)
                        {
                            Second.Remove(Second.ElementAt(i));
                        }
                    }
                    for (int i = 0; i < Second.Count; i++)
                    {
                        if (Second.ElementAt(i).Debit == Second.ElementAt(i).Credit)
                        {
                            Second.Remove(Second.ElementAt(i));
                        }
                    }
                    GL.LedgerList = Second.OrderBy(r => r.MainAccountNumber).ThenBy(r => r.SubAccountNumber).ThenBy(r => r.TransactionAccountNumber).ToList();

                    GL.OpeningBalance = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Sum(r => r.OpeningBalance);
                }
            }
            
            var mainAccList = new List<MainAccount>();
            mainAccList = _context.MainAccount.Where(r => r.CompanyID == CompID).OrderBy(r=>r.MainAccountID).ToList();
            GL.mainAccList = new SelectList(mainAccList, "MainAccountID", "AccountName");
            GL.DateFrom = DateFrom;
            GL.DateTo = DateTo;
            return View(GL);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Statement Of Account")]
        public IActionResult StatementOfAccount(int? TranAccount, DateTime? DateFrom, DateTime? DateTo)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            var transAccList = new List<TransactionAccount>();

            GeneralLedgerVM GL = new GeneralLedgerVM();

            if (DateFrom != null && DateTo != null)
            { 
                string TranAccuntNumber = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TranAccount).FirstOrDefault().TransactionAccountNumber;
                string TranAccuntName = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TranAccount).FirstOrDefault().AccountName;

                GL.AccountNumber = TranAccuntNumber;
                GL.AccountName = TranAccuntName;
                
                GL.Check_GL_Level = true;

                GL.LedgerList = (from r in _context.CashFlow
                                 where r.CompanyID == CompID 
                                 where r.TransactionAccountID == TranAccount
                                 where r.DateCreation >= DateFrom
                                 where r.DateCreation <= DateTo
                                 select new LedgerListVM
                                 {
                                     TransactionAccountNumber = r.TransactionAccount.TransactionAccountNumber,
                                     TransactionAccountName = r.TransactionAccount.AccountName,

                                     Narration_1 = r.CashInBank.Particulars,
                                     Voucher_1 = r.CashInBank.CashInBankID,
                                     Date_1 = r.CashInBank.Date,

                                     Narration_2 = r.CashPayment.Particulars,
                                     Voucher_2 = r.CashPayment.CashPaymentID,
                                     Date_2 = r.CashPayment.Date,

                                     Narration_3 = r.CashReceipt.Particulars,
                                     Voucher_3 = r.CashReceipt.CashReceiptID,
                                     Date_3 = r.CashReceipt.Date,

                                     Narration_4 = r.ChequePayment.Particulars,
                                     Voucher_4 = r.ChequePayment.ChequePaymentID,
                                     Date_4 = r.ChequePayment.DateOfMature,

                                     Narration_5 = r.ChequeReceipt.Particulars,
                                     Voucher_5 = r.ChequeReceipt.ChequeReceiptID,
                                     Date_5 = r.ChequeReceipt.DateOfMature,

                                     Narration_6 = r.PurchaseBill.Remarks,
                                     Voucher_6 = r.PurchaseBill.PurchaseBillID,
                                     Date_6 = r.PurchaseBill.BillDate,

                                     Narration_7 = r.PurchaseReturn.Remarks,
                                     Voucher_7 = r.PurchaseReturn.PurchaseReturnID,
                                     Date_7 = r.PurchaseReturn.PurBillReturnDate,

                                     Narration_8 = r.SaleBill.Remarks,
                                     Voucher_8 = r.SaleBill.SaleBillID,
                                     Date_8 = r.SaleBill.BillDate,

                                     Narration_9 = r.SaleReturn.Remarks,
                                     Voucher_9 = r.SaleReturn.SaleReturnID,
                                     Date_9 = r.SaleReturn.SaleBillReturnDate,

                                     Narration_10 = r.VoucherItems.Narration,
                                     Voucher_10 = r.VoucherItems.VoucherID,
                                     Date_10 = r.VoucherItems.Voucher.Date,

                                     Credit = r.Credit,
                                     Debit = r.Debit
                                 }).OrderBy(x => x.TransactionAccountNumber)
                                .ToList();

                GL.OpeningBalance = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.TransactionAccountID == TranAccount).Sum(r => r.OpeningBalance);

                
                transAccList = _context.TransactionAccount.Where(r => r.CompanyID == CompID).OrderBy(r => r.TransactionAccountID).ToList();
                GL.tranAccList = new SelectList(transAccList, "TransactionAccountID", "AccountName");

                GL.DateFrom = DateFrom;
                GL.DateTo = DateTo;
                return View(GL);
            }
             
            transAccList = _context.TransactionAccount.Where(r => r.CompanyID == CompID).OrderBy(r => r.TransactionAccountID).ToList();
            GL.tranAccList = new SelectList(transAccList, "TransactionAccountID", "AccountName");

            GL.DateFrom = DateFrom;
            GL.DateTo = DateTo;
            return View(GL);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Cash Flow Statment")]
        public IActionResult CashFlowStatment(DateTime? DateFrom , DateTime? DateTo)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            //Cash In hand
            int MainAccID = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "01").FirstOrDefault().MainAccountID;
            int SubAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainAccID).Where(r => r.SubAccountNumber == "0001").FirstOrDefault().SubAccountID;
            int TraAccID = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == SubAccID).Where(r => r.TransactionAccountNumber == "0001").FirstOrDefault().TransactionAccountID;

            CashFlowVM model = new CashFlowVM();
            
            List<CashFlow> CFList_A = new List<CashFlow>();
            
            model.Balance = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r=>r.TransactionAccountID == TraAccID).Sum(r => r.OpeningBalance);

            model.DateFrom = DateFrom;
            model.DateTo = DateTo;

            if (DateFrom == DateTo && DateFrom != null && DateTo != null)
            {
                model.CFList = _context.CashFlow.Where(r => r.CompanyID == CompID)
                    .Where(r => r.TransactionAccountID == TraAccID)
                    .Where(r => r.DateCreation == DateTo)
                            .Include(r => r.Parties)
                            .Include(r => r.CashInBank)
                            .Include(r => r.CashPayment)
                            .Include(r => r.CashReceipt)
                            .Include(r => r.ChequePayment)
                            .Include(r => r.ChequeReceipt)
                            .Include(r => r.PurchaseBill)
                            .Include(r => r.PurchaseReturn)
                            .Include(r => r.SaleBill)
                            .Include(r => r.SaleReturn)
                            .ToList();
            }            
            else if (DateFrom != null && DateTo == null)
            {
                model.CFList = _context.CashFlow.Where(r => r.CompanyID == CompID)
                    .Where(r=>r.TransactionAccountID == TraAccID)
                    .Where(r=>r.DateCreation >= DateFrom)                          
                            .Include(r => r.Parties)
                            .Include(r => r.CashInBank)
                            .Include(r => r.CashPayment)
                            .Include(r => r.CashReceipt)
                            .Include(r => r.ChequePayment)
                            .Include(r => r.ChequeReceipt)
                            .Include(r => r.PurchaseBill)
                            .Include(r => r.PurchaseReturn)
                            .Include(r => r.SaleBill)
                            .Include(r => r.SaleReturn)
                            .ToList();                
            }
            else if (DateFrom == null && DateTo != null)
            {
                model.CFList = _context.CashFlow.Where(r => r.CompanyID == CompID)
                    .Where(r => r.TransactionAccountID == TraAccID)
                    .Where(r => r.DateCreation <= DateTo)
                            .Include(r => r.Parties)
                            .Include(r => r.CashInBank)
                            .Include(r => r.CashPayment)
                            .Include(r => r.CashReceipt)
                            .Include(r => r.ChequePayment)
                            .Include(r => r.ChequeReceipt)
                            .Include(r => r.PurchaseBill)
                            .Include(r => r.PurchaseReturn)
                            .Include(r => r.SaleBill)
                            .Include(r => r.SaleReturn)
                            .ToList();                
            }
            else if (DateFrom != null && DateTo != null && DateFrom != DateTo)
            {
                model.CFList = _context.CashFlow.Where(r => r.CompanyID == CompID)
                    .Where(r => r.TransactionAccountID == TraAccID)
                    .Where(r => r.DateCreation >= DateFrom)
                    .Where(r => r.DateCreation <= DateTo)
                            .Include(r => r.Parties)
                            .Include(r => r.CashInBank)
                            .Include(r => r.CashPayment)
                            .Include(r => r.CashReceipt)
                            .Include(r => r.ChequePayment)
                            .Include(r => r.ChequeReceipt)
                            .Include(r => r.PurchaseBill)
                            .Include(r => r.PurchaseReturn)
                            .Include(r => r.SaleBill)
                            .Include(r => r.SaleReturn)
                            .ToList();                                
            }
            else
            {
                model.CFList = _context.CashFlow.Where(r => r.CompanyID == CompID)
                    .Where(r => r.TransactionAccountID == TraAccID)
                            .Include(r => r.Parties)
                            .Include(r => r.CashInBank)
                            .Include(r => r.CashPayment)
                            .Include(r => r.CashReceipt)
                            .Include(r => r.ChequePayment)
                            .Include(r => r.ChequeReceipt)
                            .Include(r => r.PurchaseBill)
                            .Include(r => r.PurchaseReturn)
                            .Include(r => r.SaleBill)
                            .Include(r => r.SaleReturn)
                            .ToList();
            }
            
            return View(model);
        }
     
        [Route("[action]")]
        public IActionResult CashFlowDetails_VoucherItems(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            VoucherItems ContextData = new VoucherItems();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.VoucherItems.Where(r => r.CompanyID == CompID).Include(r=>r.Voucher).Where(r => r.VoucherItemsID == id).FirstOrDefault();

            model.Type = "Voucher";
            model.Id = ContextData.VoucherID;
            model.Title = ContextData.Voucher.Particulars;
            
            if(ContextData.Credit != 0)
            {
                model.Amount = ContextData.Credit;
                model.Name_2 = "Cr";
            }
            else
            {
                model.Amount = ContextData.Debit;
                model.Name_2 = "Dr";
            }
            model.date = ContextData.Voucher.Date;
            model.Name_1 = ContextData.Voucher.ExternalRef;            

            return PartialView("CashFlowDetails_JournalVoucher", model);
        }

        [Route("[action]")]
        public IActionResult CashFlowDetails_CashPayment(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            CashPayment ContextData = new CashPayment();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.CashPayment.Where(r => r.CompanyID == CompID).Where(r => r.CashPaymentID == id).Include(r => r.Parties).FirstOrDefault();

            model.Type = "Cash Payment";
            model.Id = ContextData.CashPaymentID;
            model.Title = ContextData.Parties.PartyName;
            model.Amount = ContextData.Amount;
            model.date = ContextData.Date;
            model.Name_1 = ContextData.AmountInWords;
            model.Name_2 = ContextData.Payee;
            model.Name_3 = ContextData.Particulars;            
           
            return PartialView("CashFlowDetails_CashPayment", model);            
        }

        [Route("[action]")]
        public IActionResult CashFlowDetails_CashReceipt(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            CashReceipt ContextData = new CashReceipt();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.CashReceipt.Where(r => r.CompanyID == CompID).Where(r => r.CashReceiptID == id).Include(r => r.Parties).FirstOrDefault();

            model.Type = "Cash Receipt";
            model.Id = ContextData.CashReceiptID;
            model.Title = ContextData.Parties.PartyName;
            model.Amount = ContextData.Amount;
            model.date = ContextData.Date;
            model.Name_1 = ContextData.AmountInWords;
            model.Name_2 = ContextData.PaidBy;
            model.Name_3 = ContextData.Particulars;
         
            return PartialView("CashFlowDetails_CashReceipt", model);
        }
 
        [Route("[action]")]
        public IActionResult CashFlowDetails_PurchaseBill(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            PurchaseBill ContextData = new PurchaseBill();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PurchaseBillID == id).Include(r => r.Parties).FirstOrDefault();

            model.Type = "Purchase Bill";
            model.Id = ContextData.PurchaseBillID;
            if(ContextData.Parties == null)
            {
                model.Title = "Cash Purchase";
            }
            else
            {
                model.Title = ContextData.Parties.PartyName;
            }
            model.Amount = ContextData.NetAmount;
            model.date = ContextData.BillDate;
            model.Decimal_1 = ContextData.GrossTotal;
            model.Name_1 = ContextData.TDiscount;
            model.Name_2 = ContextData.Remarks;
            model.Name_3 = ContextData.CreatedBy;            
            model.Name_5 = ContextData.ContactPerson;

            return PartialView("CashFlowDetails_PurchaseBill", model);
        }

        [Route("[action]")]
        public IActionResult CashFlowDetails_PurchaseReturn(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            PurchaseReturn ContextData = new PurchaseReturn();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.PurchaseReturn.Where(r => r.CompanyID == CompID).Where(r => r.PurchaseReturnID == id).Include(r=>r.CashFlow).Include(r => r.Parties).FirstOrDefault();

            model.Type = "Purchase Return Bill";
            model.Id = ContextData.PurchaseReturnID;
            if (ContextData.Parties == null)
            {
                model.Title = "Cash Purchase";
            }
            else
            {
                model.Title = ContextData.Parties.PartyName;
            }            
            model.Amount = ContextData.AmountToReceive;
            model.date = ContextData.CashFlow.Where(r=>r.PurchaseReturnID == id).FirstOrDefault().DateCreation;            
            model.Name_2 = ContextData.Remarks;
            model.Name_3 = ContextData.CreatedBy;
             
            return PartialView("CashFlowDetails_PurchaseReturn", model);            
        }

        [Route("[action]")]
        public IActionResult CashFlowDetails_SaleBill(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            SaleBill ContextData = new SaleBill();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.SaleBill.Where(r => r.CompanyID == CompID).Where(r => r.SaleBillID == id).Include(r => r.Parties).FirstOrDefault();
            
            model.Type = "Sale Bill";
            model.Id = ContextData.SaleBillID;
            if (ContextData.Parties == null)
            {
                model.Title = "Cash Sale";
            }
            else
            {
                model.Title = ContextData.Parties.PartyName;
            }
            model.Amount = ContextData.NetAmount;
            model.date = ContextData.BillDate;
            model.Decimal_1 = ContextData.GrossTotal;
            model.Name_1 = ContextData.TDiscount;
            model.Name_2 = ContextData.Remarks;
            model.Name_3 = ContextData.CreatedBy;
            model.Name_5 = ContextData.ContactPerson;

            return PartialView("CashFlowDetails_SaleBill", model);            
        }

        [Route("[action]")]
        public IActionResult CashFlowDetails_SaleReturn(int id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            SaleReturn ContextData = new SaleReturn();
            CashFlowDetailsVM model = new CashFlowDetailsVM();
            ContextData = _context.SaleReturn.Where(r => r.CompanyID == CompID).Where(r => r.SaleReturnID == id).Include(r=>r.CashFlow).Include(r => r.Parties).FirstOrDefault();

            model.Type = "Sale Return Bill";
            model.Id = ContextData.SaleReturnID;
            if (ContextData.Parties == null)
            {
                model.Title = "Cash Sale";
            }
            else
            {
                model.Title = ContextData.Parties.PartyName;
            }
            model.Amount = ContextData.AmountToPay;
            model.date = ContextData.CashFlow.Where(r => r.SaleReturnID == id).FirstOrDefault().DateCreation;
            model.Name_2 = ContextData.Remarks;
            model.Name_3 = ContextData.CreatedBy;
        
            return PartialView("CashFlowDetails_SaleReturn", model);
        }

        [Route("[action]")]
        public JsonResult LoadSubAccounts(int id)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var a = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(p => p.MainAccountID == id).Select(x => new { x.SubAccountID , x.AccountName }).ToList();
            return Json(a);
        }

        [Route("[action]")]
        public JsonResult LoadTranAccounts(int id)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var a = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(p => p.SubAccountID == id).Select(x => new { x.TransactionAccountID, x.AccountName }).ToList();
            return Json(a);
        }


    }
}

//[Route("[action]")]
//public IActionResult CashFlowDetails_ChequePayment(int id)
//{
//    string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
//    int CompID = Convert.ToInt32(CompId);

//    ChequePayment ContextData = new ChequePayment();
//    CashFlowDetailsVM model = new CashFlowDetailsVM();
//    ContextData = _context.ChequePayment.Where(r => r.CompanyID == CompID).Where(r => r.ChequePaymentID == id).Include(r => r.Parties).Include(r => r.Bank).FirstOrDefault();

//    model.Type = "Cheque Payment";
//    model.Id = ContextData.ChequePaymentID;
//    model.Title = ContextData.Parties.PartyName;
//    model.Amount = ContextData.Amount;
//    model.date = ContextData.Date;
//    model.Name_1 = ContextData.AmountInWords;
//    model.Name_2 = ContextData.InNameOf;
//    model.Name_3 = ContextData.Particulars;
//    model.Name_4 = ContextData.Bank.BankName;

//    return PartialView("CashFlowDetails", model);
//}

//[Route("[action]")]
//public IActionResult CashFlowDetails_ChequeReceipt(int id)
//{
//    string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
//    int CompID = Convert.ToInt32(CompId);

//    ChequeReceipt ContextData = new ChequeReceipt();
//    CashFlowDetailsVM model = new CashFlowDetailsVM();
//    ContextData = _context.ChequeReceipt.Where(r => r.CompanyID == CompID).Where(r => r.ChequeReceiptID == id).Include(r => r.Parties).Include(r => r.Bank).FirstOrDefault();

//    model.Type = "Cheque Receipt";
//    model.Id = ContextData.ChequeReceiptID;
//    model.Title = ContextData.Parties.PartyName;
//    model.Amount = ContextData.Amount;
//    model.date = ContextData.Date;
//    model.Name_1 = ContextData.AmountInWords;
//    model.Name_2 = ContextData.InNameOf;
//    model.Name_3 = ContextData.Particulars;
//    model.Name_4 = ContextData.Bank.BankName;

//    return PartialView("CashFlowDetails", model);
//}
