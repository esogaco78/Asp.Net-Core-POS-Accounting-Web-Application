using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Reports.Models;
using System;
using System.Collections.Generic;
using Invento.Areas.CompanyAdmin.Models.Company;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Invento.Areas.Reports.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Reports")]
    [Route("Reports/[controller]")]
    public class AnalysisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalysisController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("[action]")]
        [Authorize(Roles = "Reports,CompanyAdmin")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "Reports,CompanyAdmin")]
        public IActionResult AmountView(AnalysisVM model)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            DateTime.Now.AddYears(-1);

            int MainPartyAcc = _context.MainAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountNumber == "00").FirstOrDefault().MainAccountID;
            int SubPartyAcc = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.MainAccountID == MainPartyAcc).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

            List<decimal> listReceviable= new List<decimal>();
            List<decimal> listPayable = new List<decimal>();

            if(model.Party == null)
            {
                var Receivable = (from r in _context.CashFlow
                                  where r.CompanyID == CompID
                                  where r.MainAccountID == MainPartyAcc
                                  where r.SubAccountID == SubPartyAcc
                                  where r.PartiesID != null
                                  group r by r.PartiesID into g
                                  select new
                                  {
                                      ID = (int)g.Key,
                                      OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                      credit = g.Sum(x => x.Credit),
                                      debit = g.Sum(x => x.Debit),
                                  }).ToList();

                foreach (var item in Receivable)
                {
                    decimal XYZ = item.OpeningBalance + item.debit - item.credit;
                    if (XYZ > 0)
                    {
                        listReceviable.Add(XYZ);
                    }
                }
                listReceviable.Sum();

                var Payable = (from r in _context.CashFlow
                               where r.CompanyID == CompID
                               where r.MainAccountID == MainPartyAcc
                               where r.SubAccountID == SubPartyAcc
                               where r.PartiesID != null
                               group r by r.PartiesID into g
                               select new
                               {
                                   ID = (int)g.Key,
                                   OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                   credit = g.Sum(x => x.Credit),
                                   debit = g.Sum(x => x.Debit)
                               }).ToList();
                foreach (var item in Payable)
                {
                    decimal XYZ = item.OpeningBalance - item.credit + item.debit;
                    if (XYZ < 0)
                    {
                        listPayable.Add(XYZ);
                    }
                }
                model.Deci1 = listReceviable.Sum();
                model.Deci2 = listPayable.Sum();
            }
            else
            {
                var Receivable = (from r in _context.CashFlow
                                  where r.CompanyID == CompID
                                  where r.MainAccountID == MainPartyAcc
                                  where r.SubAccountID == SubPartyAcc
                                  where r.PartiesID == model.Party
                                  group r by r.PartiesID into g
                                  select new
                                  {
                                      ID = (int)g.Key,
                                      OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                      credit = g.Sum(x => x.Credit),
                                      debit = g.Sum(x => x.Debit),
                                  }).ToList();

                foreach (var item in Receivable)
                {
                    decimal XYZ = item.OpeningBalance + item.debit - item.credit;
                    if (XYZ > 0)
                    {
                        listReceviable.Add(XYZ);
                    }
                }
                listReceviable.Sum();

                var Payable = (from r in _context.CashFlow
                               where r.CompanyID == CompID
                               where r.MainAccountID == MainPartyAcc
                               where r.SubAccountID == SubPartyAcc
                               where r.PartiesID == model.Party
                               group r by r.PartiesID into g
                               select new
                               {
                                   ID = (int)g.Key,
                                   OpeningBalance = g.Select(x => x.TransactionAccount.OpeningBalance).FirstOrDefault(),
                                   credit = g.Sum(x => x.Credit),
                                   debit = g.Sum(x => x.Debit)
                               }).ToList();
                foreach (var item in Payable)
                {
                    decimal XYZ = item.OpeningBalance - item.credit + item.debit;
                    if (XYZ < 0)
                    {
                        listPayable.Add(XYZ);
                    }
                }
                model.Deci1 = listReceviable.Sum();
                model.Deci2 = listPayable.Sum();
            }

            var partyList = new List<Parties>();
            partyList = _context.Parties.Where(r => r.CompanyID == CompID).ToList();
            model.PartyList = new SelectList(partyList, "PartiesID", "PartyName");
           
            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "Reports,CompanyAdmin")]
        public IActionResult VoucherView(AnalysisVM model)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
 
            if (model.Period == 0)
            {
                DateTime SearchDate = DateTime.Now.AddYears(-1);

                model.Deci1 = _context.CashInBank.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci2 = _context.CashPayment.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci3 = _context.CashReceipt.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci4 = _context.ChequePayment.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci5 = _context.ChequeReceipt.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci6 = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.Voucher.Date >= SearchDate).Sum(r => r.Credit);
                model.Deci7 = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.Voucher.Date >= SearchDate).Sum(r => r.Debit);
            }
            else if (model.Period == 1)
            {
                DateTime SearchDate = DateTime.Now.AddMonths(-1);

                model.Deci1 = _context.CashInBank.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci2 = _context.CashPayment.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci3 = _context.CashReceipt.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci4 = _context.ChequePayment.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci5 = _context.ChequeReceipt.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci6 = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.Voucher.Date >= SearchDate).Sum(r => r.Credit);
                model.Deci7 = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.Voucher.Date >= SearchDate).Sum(r => r.Debit);
            }
            else if (model.Period == 2)
            {
                DateTime SearchDate = DateTime.Now.AddDays(-1);

                model.Deci1 = _context.CashInBank.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci2 = _context.CashPayment.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci3 = _context.CashReceipt.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci4 = _context.ChequePayment.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci5 = _context.ChequeReceipt.Where(r => r.CompanyID == CompID).Where(r => r.Date >= SearchDate).Sum(r => r.Amount);
                model.Deci6 = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.Voucher.Date >= SearchDate).Sum(r => r.Credit);
                model.Deci7 = _context.VoucherItems.Where(r => r.CompanyID == CompID).Where(r => r.Voucher.Date >= SearchDate).Sum(r => r.Debit);
            }
           
            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "Reports,CompanyAdmin")]
        public IActionResult PurchaseSaleViewYearly(AnalysisVM model)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            
            if(model.Period == 0)
            {
                model.Period = DateTime.Now.Year;
            }
            var purList = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PB_B_1 == false).Where(r => r.BillDate.Year == model.Period).ToList();
            var salList = _context.SaleBill.Where(r => r.CompanyID == CompID).Where(r => r.SB_B_1 == false).Where(r => r.BillDate.Year == model.Period).ToList();

            int a = purList.Count(r => r.BillDate.Month == 1);
            int b = purList.Count(r => r.BillDate.Month == 2);
            int c = purList.Count(r => r.BillDate.Month == 3);
            int d = purList.Count(r => r.BillDate.Month == 4);
            int e = purList.Count(r => r.BillDate.Month == 5);
            int f = purList.Count(r => r.BillDate.Month == 6);
            int g = purList.Count(r => r.BillDate.Month == 7);
            int h = purList.Count(r => r.BillDate.Month == 8);
            int i = purList.Count(r => r.BillDate.Month == 9);
            int j = purList.Count(r => r.BillDate.Month == 10);
            int k = purList.Count(r => r.BillDate.Month == 11);
            int l = purList.Count(r => r.BillDate.Month == 12);            
            model.Data = string.Concat(a+","+b + "," + c + "," + d + "," + e + "," + f + "," + g + "," + h + "," + i + "," + j + "," + k + "," + l);

            int a1 = salList.Count(r => r.BillDate.Month == 1);
            int b1 = salList.Count(r => r.BillDate.Month == 2);
            int c1 = salList.Count(r => r.BillDate.Month == 3);
            int d1 = salList.Count(r => r.BillDate.Month == 4);
            int e1 = salList.Count(r => r.BillDate.Month == 5);
            int f1 = salList.Count(r => r.BillDate.Month == 6);
            int g1 = salList.Count(r => r.BillDate.Month == 7);
            int h1 = salList.Count(r => r.BillDate.Month == 8);
            int i1 = salList.Count(r => r.BillDate.Month == 9);
            int j1 = salList.Count(r => r.BillDate.Month == 10);
            int k1 = salList.Count(r => r.BillDate.Month == 11);
            int l1 = salList.Count(r => r.BillDate.Month == 12);
            model.Data_1 = string.Concat(a1 + "," + b1 + "," + c1 + "," + d1 + "," + e1 + "," + f1 + "," + g1 + "," + h1 + "," + i1 + "," + j1 + "," + k1 + "," + l1);

            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "Reports,CompanyAdmin")]
        public IActionResult PurchaseSaleViewMonthly(AnalysisVM model)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (model.Period == 0)
            {
                model.Period = DateTime.Now.Year;
                model.Period_1 = 1;
            }

            var purList = _context.PurchaseBill.Where(r => r.CompanyID == CompID).Where(r => r.PB_B_1 == false).Where(r => r.BillDate.Year == model.Period).Where(r => r.BillDate.Month == model.Period_1).ToList();
            var salList = _context.SaleBill.Where(r => r.CompanyID == CompID).Where(r => r.SB_B_1 == false).Where(r => r.BillDate.Year == model.Period).ToList();

            int a1 = purList.Count(r => r.BillDate.Day == 1);
            int a2 = purList.Count(r => r.BillDate.Day == 2);
            int a3 = purList.Count(r => r.BillDate.Day == 3);
            int a4 = purList.Count(r => r.BillDate.Day == 4);
            int a5 = purList.Count(r => r.BillDate.Day == 5);
            int a6 = purList.Count(r => r.BillDate.Day == 6);
            int a7 = purList.Count(r => r.BillDate.Day == 7);
            int a8 = purList.Count(r => r.BillDate.Day == 8);
            int a9 = purList.Count(r => r.BillDate.Day == 9);
            int a10 = purList.Count(r => r.BillDate.Day == 10);
            int a11 = purList.Count(r => r.BillDate.Day == 11);
            int a12 = purList.Count(r => r.BillDate.Day == 12);
            int a13 = purList.Count(r => r.BillDate.Day == 13);
            int a14 = purList.Count(r => r.BillDate.Day == 14);
            int a15 = purList.Count(r => r.BillDate.Day == 15);
            int a16 = purList.Count(r => r.BillDate.Day == 16);
            int a17 = purList.Count(r => r.BillDate.Day == 17);
            int a18 = purList.Count(r => r.BillDate.Day == 18);
            int a19 = purList.Count(r => r.BillDate.Day == 19);
            int a20 = purList.Count(r => r.BillDate.Day == 20);
            int a21 = purList.Count(r => r.BillDate.Day == 21);
            int a22 = purList.Count(r => r.BillDate.Day == 22);
            int a23 = purList.Count(r => r.BillDate.Day == 23);
            int a24 = purList.Count(r => r.BillDate.Day == 24);
            int a25 = purList.Count(r => r.BillDate.Day == 25);
            int a26 = purList.Count(r => r.BillDate.Day == 26);
            int a27 = purList.Count(r => r.BillDate.Day == 27);
            int a28 = purList.Count(r => r.BillDate.Day == 28);
            int a29 = purList.Count(r => r.BillDate.Day == 29);
            int a30 = purList.Count(r => r.BillDate.Day == 30);
            int a31 = purList.Count(r => r.BillDate.Day == 31);


            model.Data = string.Concat(a1 +","+a2+"," + a3 + "," + a4 + "," + a5 + "," + a6 + "," + a7 + "," + a8 + "," + a9 + ","
                + a10 + ","+ a11 + "," + a12 + "," + a13 + "," + a14 + "," + a15 + "," + a16 + "," + a17 + "," + a18 + "," + a19 + ","
                + a20 + "," + a21 + "," + a22 + "," + a23 + "," + a24 + "," + a25 + "," + a26 + "," + a27 + "," + a28 + "," + a29 + "," + a30 + "," + a31);

            int aa1 = salList.Count(r => r.BillDate.Day == 1);
            int aa2 = salList.Count(r => r.BillDate.Day == 2);
            int aa3 = salList.Count(r => r.BillDate.Day == 3);
            int aa4 = salList.Count(r => r.BillDate.Day == 4);
            int aa5 = salList.Count(r => r.BillDate.Day == 5);
            int aa6 = salList.Count(r => r.BillDate.Day == 6);
            int aa7 = salList.Count(r => r.BillDate.Day == 7);
            int aa8 = salList.Count(r => r.BillDate.Day == 8);
            int aa9 = salList.Count(r => r.BillDate.Day == 9);
            int aa10 = salList.Count(r => r.BillDate.Day == 10);
            int aa11 = salList.Count(r => r.BillDate.Day == 11);
            int aa12 = salList.Count(r => r.BillDate.Day == 12);
            int aa13 = salList.Count(r => r.BillDate.Day == 13);
            int aa14 = salList.Count(r => r.BillDate.Day == 14);
            int aa15 = salList.Count(r => r.BillDate.Day == 15);
            int aa16 = salList.Count(r => r.BillDate.Day == 16);
            int aa17 = salList.Count(r => r.BillDate.Day == 17);
            int aa18 = salList.Count(r => r.BillDate.Day == 18);
            int aa19 = salList.Count(r => r.BillDate.Day == 19);
            int aa20 = salList.Count(r => r.BillDate.Day == 20);
            int aa21 = salList.Count(r => r.BillDate.Day == 21);
            int aa22 = salList.Count(r => r.BillDate.Day == 22);
            int aa23 = salList.Count(r => r.BillDate.Day == 23);
            int aa24 = salList.Count(r => r.BillDate.Day == 24);
            int aa25 = salList.Count(r => r.BillDate.Day == 25);
            int aa26 = salList.Count(r => r.BillDate.Day == 26);
            int aa27 = purList.Count(r => r.BillDate.Day == 27);
            int aa28 = salList.Count(r => r.BillDate.Day == 28);
            int aa29 = salList.Count(r => r.BillDate.Day == 29);
            int aa30 = salList.Count(r => r.BillDate.Day == 30);
            int aa31 = salList.Count(r => r.BillDate.Day == 31);
            model.Data_1 = string.Concat(aa1 + "," + aa2 + "," + aa3 + "," + aa4 + "," + aa5 + "," + aa6 + "," + aa7 + "," + aa8 + "," + aa9 + ","
               + aa10 + "," + aa11 + "," + aa12 + "," + aa13 + "," + aa14 + "," + aa15 + "," + aa16 + "," + aa17 + "," + aa18 + "," + aa19 + ","
               + aa20 + "," + aa21 + "," + aa22 + "," + aa23 + "," + aa24 + "," + aa25 + "," + aa26 + "," + aa27 + "," + aa28 + "," + aa29 + "," + aa30 + "," + aa31);

            return View(model);
        }
    }
}