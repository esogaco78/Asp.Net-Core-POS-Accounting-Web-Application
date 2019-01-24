using Invento.Areas.Purchase.Models;
using Invento.Areas.Sale.Models;
using System;
using System.Collections.Generic;

namespace Invento.Areas.Reports.Models
{
    public class ProductLedgerVM
    {
        public int TotalRowsCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal LastPurchasePrice { get; set; }
        public decimal QuantityAvailable { get; set; }
        public string OemNo { get; set; }
        public string CrossRef { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<PurchaseBillItem> MainListPurchase { get; set; }
        public List<SaleBillItem> MainListSale { get; set; }
        public List<ProductLedgerVM> ListProfitItem { get; set; }

        public decimal TotalPurcahasePrice { get; set; }
        public decimal TotalSalePrice { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitPercentage { get; set; }
    }    
}
