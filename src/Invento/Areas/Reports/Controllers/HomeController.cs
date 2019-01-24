using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Invento.Data;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Reports.Models;
using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System.Collections.Generic;

namespace Invento.Areas.Reports.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Reports")]
    [Route("Reports/[controller]")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
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
        [Authorize(Roles = "CompanyAdmin,Reports,Product Purchase Ledger")]
        public IActionResult ProductPurchaseLedger(string Item, DateTime? DateFrom, DateTime? DateTo)
        {
            ProductLedgerVM model = new ProductLedgerVM();
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (DateFrom != null && DateTo == null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                         .Where(r => r.PurchaseBillExtraBool == false)
                                         .Where(r => r.Item.OEMNo == Item)
                                         .Where(r => r.PurchaseBill.BillDate >= DateFrom)
                                         .Include(r => r.Item)
                                         .Include(r => r.PurchaseBill)
                                         .OrderByDescending(r => r.PurchaseBill.BillDate)
                                         .ToList();

                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                      .Where(r => r.PurchaseBillExtraBool == false)
                                      .Where(r => r.PurchaseBill.BillDate >= DateFrom)
                                      .Include(r => r.Item)
                                      .Include(r => r.PurchaseBill)
                                      .OrderByDescending(r => r.PurchaseBill.BillDate)
                                      .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }
            else if (DateFrom == null && DateTo != null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                         .Include(r => r.Item)
                                         .Include(r => r.PurchaseBill)
                                         .Where(r => r.PurchaseBillExtraBool == false)
                                         .Where(r => r.Item.OEMNo == Item)
                                         .Where(r => r.PurchaseBill.BillDate <= DateTo)
                                         .OrderByDescending(r => r.PurchaseBill.BillDate)
                                         .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                      .Include(r => r.Item)
                                      .Include(r => r.PurchaseBill)
                                      .Where(r => r.PurchaseBillExtraBool == false)
                                      .Where(r => r.PurchaseBill.BillDate <= DateTo)
                                      .OrderByDescending(r => r.PurchaseBill.BillDate)
                                      .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }
            else if (DateFrom != null && DateTo != null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                         .Where(r => r.PurchaseBillExtraBool == false)
                                         .Where(r => r.Item.OEMNo == Item)
                                         .Where(r => r.PurchaseBill.BillDate >= DateFrom)
                                         .Where(r => r.PurchaseBill.BillDate <= DateTo)
                                         .Include(r => r.Item)
                                         .Include(r => r.PurchaseBill)
                                         .OrderByDescending(r => r.PurchaseBill.BillDate)
                                         .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                      .Where(r => r.PurchaseBillExtraBool == false)
                                      .Include(r => r.Item)
                                      .Include(r => r.PurchaseBill)
                                       .Where(r => r.PurchaseBill.BillDate >= DateFrom)
                                      .Where(r => r.PurchaseBill.BillDate <= DateTo)
                                      .OrderByDescending(r => r.PurchaseBill.BillDate)
                                      .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                         .Where(r => r.PurchaseBillExtraBool == false)
                                         .Where(r => r.Item.OEMNo == Item)
                                         .Include(r => r.Item)
                                         .Include(r => r.PurchaseBill)
                                         .OrderByDescending(r => r.PurchaseBill.BillDate)
                                         .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListPurchase = _context.PurchaseBillItem.Where(r => r.CompanyID == CompID)
                                      .Where(r => r.PurchaseBillExtraBool == false)
                                      .Include(r => r.Item)
                                      .Include(r => r.PurchaseBill)
                                      .OrderByDescending(r => r.PurchaseBill.BillDate)
                                      .ToList();
                    if (model.MainListPurchase.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListPurchase.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListPurchase.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListPurchase.Sum(r => r.PurchasePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }

            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Reports,Product Sale Ledger")]
        public IActionResult ProductSaleLedger(string Item, DateTime? DateFrom, DateTime? DateTo)
        {
            ProductLedgerVM model = new ProductLedgerVM();

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (DateFrom != null && DateTo == null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.Item.OEMNo == Item)
                                     .Where(r => r.SaleBill.BillDate >= DateFrom)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.SaleBill.BillDate >= DateFrom)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }
            else if (DateFrom == null && DateTo != null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.Item.OEMNo == Item)
                                     .Where(r => r.SaleBill.BillDate <= DateTo)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.SaleBill.BillDate <= DateTo)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }
            else if (DateFrom != null && DateTo != null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.Item.OEMNo == Item)
                                     .Where(r => r.SaleBill.BillDate >= DateFrom)
                                     .Where(r => r.SaleBill.BillDate <= DateTo)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.SaleBill.BillDate >= DateFrom)
                                     .Where(r => r.SaleBill.BillDate <= DateTo)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)
                                     .Where(r => r.Item.OEMNo == Item)
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                             .Where(r => r.OEMNo == Item).Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
                else
                {
                    model.MainListSale = _context.SaleBillItem.Where(r => r.CompanyID == CompID)
                                     .Where(r => r.SaleBillExtraBool == false)                                     
                                     .Include(r => r.Item)
                                     .Include(r => r.SaleBill)
                                     .OrderByDescending(r => r.SaleBill.BillDate)
                                     .ToList();

                    if (model.MainListSale.Count != 0)
                    {
                        model.QuantityAvailable = _context.Item.Where(r => r.CompanyID == CompID)
                                            .Sum(r => r.Quantity);

                        int TotalRowsCount = model.MainListSale.Count();
                        model.TotalRowsCount = TotalRowsCount;

                        decimal TotalQuantity = model.MainListSale.Sum(r => r.Quantity);
                        model.TotalQuantity = TotalQuantity;

                        decimal TotalPrice = model.MainListSale.Sum(r => r.SalePrice);
                        TotalPrice = Math.Round(TotalPrice, 2);
                        model.TotalPrice = TotalPrice;

                        decimal AveragePrice = TotalPrice / TotalRowsCount;
                        AveragePrice = Math.Round(AveragePrice, 2);
                        model.AveragePrice = AveragePrice;
                    }
                }
            }

            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Reports,Product Profit Ledger")]
        public IActionResult ProductProfitLedger(string Item, DateTime? DateFrom, DateTime? DateTo)
        {
            ProductLedgerVM model = new ProductLedgerVM();

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            List<PurchaseBillItem> MainListPurchase = new List<PurchaseBillItem>();
            List<SaleBillItem> MainListSale = new List<SaleBillItem>();
            List<ProductLedgerVM> ListProfitItem = new List<ProductLedgerVM>();
            
            if (DateFrom != null && DateTo == null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.Item.OEMNo == Item
                                        where r.PurchaseBill.BillDate >= DateFrom
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                                 .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.Item.OEMNo == Item
                                    where r.SaleBill.BillDate >= DateFrom
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.PurchaseBill.BillDate >= DateFrom
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                             .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.SaleBill.BillDate >= DateFrom
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
            }            
            else if (DateFrom == null && DateTo != null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.Item.OEMNo == Item
                                        where r.PurchaseBill.BillDate <= DateTo
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                                 .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.Item.OEMNo == Item
                                    where r.SaleBill.BillDate <= DateTo
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.PurchaseBill.BillDate <= DateTo
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                             .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.SaleBill.BillDate <= DateTo                                    
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
            }
            else if (DateFrom != null && DateTo != null)
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.Item.OEMNo == Item
                                        where r.PurchaseBill.BillDate >= DateFrom
                                        where r.PurchaseBill.BillDate <= DateTo
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                                 .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.Item.OEMNo == Item
                                    where r.SaleBill.BillDate >= DateFrom
                                    where r.SaleBill.BillDate <= DateTo
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.PurchaseBill.BillDate >= DateFrom
                                        where r.PurchaseBill.BillDate <= DateTo
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                             .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.SaleBill.BillDate >= DateFrom
                                    where r.SaleBill.BillDate <= DateTo
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(Item))
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false
                                        where r.Item.OEMNo == Item                                        
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                                 .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false
                                    where r.Item.OEMNo == Item                                    
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();
                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;
                                    ListProfitItem.Add(PL);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MainListPurchase = (from r in _context.PurchaseBillItem
                                        where r.CompanyID == CompID
                                        where r.PurchaseBillExtraBool == false                                        
                                        group r by r.ItemID into g
                                        select new PurchaseBillItem
                                        {
                                            ItemID = g.Select(r=>r.ItemID).FirstOrDefault(),
                                            PurchaseBillExtraString = g.Select(r=>r.Item.OEMNo).FirstOrDefault(),
                                            PurchaseBillExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                            PurchaseBillExtraDecimal = g.Sum(x => x.PurchasePrice)
                                        }).OrderBy(x => x.ItemID)
                                             .ToList();
                    MainListSale = (from r in _context.SaleBillItem
                                    where r.CompanyID == CompID
                                    where r.SaleBillExtraBool == false                                    
                                    group r by r.ItemID into g
                                    select new SaleBillItem
                                    {
                                        ItemID = g.Select(r => r.ItemID).FirstOrDefault(),
                                        SaleBillExtraString = g.Select(r => r.Item.OEMNo).FirstOrDefault(),
                                        SaleExtraString_2 = g.Select(r => r.Item.CrossRef).FirstOrDefault(),
                                        SaleBillExtraDecimal = g.Sum(x => x.SalePrice)
                                    }).OrderBy(x => x.ItemID)
                                                 .ToList();

                    if (MainListPurchase.Count != 0 && MainListSale.Count != 0)
                    {
                        foreach (var item in MainListSale)
                        {
                            foreach (var itemA in MainListPurchase)
                            {
                                if (item.ItemID == itemA.ItemID)
                                {
                                    ProductLedgerVM PL = new ProductLedgerVM();

                                    PL.OemNo = itemA.PurchaseBillExtraString;
                                    PL.CrossRef = itemA.PurchaseBillExtraString;
                                    PL.TotalPurcahasePrice = itemA.PurchaseBillExtraDecimal;
                                    PL.TotalSalePrice = item.SaleBillExtraDecimal;
                                    PL.TotalProfit = item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal;
                                    PL.ProfitPercentage = (item.SaleBillExtraDecimal - itemA.PurchaseBillExtraDecimal) / (item.SaleBillExtraDecimal) * 100;

                                    ListProfitItem.Add(PL);                                    
                                }
                            }
                        }
                    }
                }
            }
            model.ListProfitItem = ListProfitItem; 
            return View(model);
        }
    }
}