using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Invento.Models;
using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Areas.Product.Models;
using Invento.Areas.Purchase.Models;
using Invento.Areas.Finance.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Invento.Areas.Sale.Models;
using Invento.Areas.Payment.Models;

namespace Invento.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Cascade;                
            }


            foreach (var property in builder.Model.GetEntityTypes()
                  .SelectMany(t => t.GetProperties())
                  .Where(p => p.ClrType == typeof(decimal)))
                        {
                            property.Relational().ColumnType = "decimal(18, 6)";
                        }

            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        // Site Admin Start
        public DbSet<Country> Country { get; set; }
        public DbSet<Currency> Currency { get; set; }
        // Site Admin End

        // Company Admin Start
        public DbSet<CompanyProfile> CompanyProfile { get; set; }
        public DbSet<Bank> Bank { get; set; }
        // Company Admin End

        // Company User Start
        public DbSet<Parties> Parties { get; set; }
        public DbSet<ProductGroup> ProductGroup { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<GRN> GRN { get; set; }
        public DbSet<GRNItem> GRNItem { get; set; }
        // Company User End

        // Purchase Start
        public DbSet<PurchaseBill> PurchaseBill { get; set; }
        public DbSet<PurchaseBillItem> PurchaseBillItem { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<PurchaseReturn> PurchaseReturn { get; set; }
        public DbSet<PurchaseReturnTransaction> PurchaseReturnTransaction { get; set; }

        // Purchase End

        // Sale Start
        public DbSet<SaleBill> SaleBill { get; set; }
        public DbSet<SaleBillItem> SaleBillItem { get; set; }
        public DbSet<SaleTransaction> SaleTransaction { get; set; }
        public DbSet<SaleReturn> SaleReturn { get; set; }
        public DbSet<SaleReturnTransaction> SaleReturnTransaction { get; set; }
        // Sale End

        // Finance Start
        public DbSet<CashFlow> CashFlow { get; set; }
        public DbSet<MainAccount> MainAccount { get; set; }
        public DbSet<SubAccount> SubAccount { get; set; }
        public DbSet<TransactionAccount> TransactionAccount { get; set; }
        // Finance Start

        // Voucher
        public DbSet<CashPayment> CashPayment { get; set; }
        public DbSet<CashReceipt> CashReceipt { get; set; }
        public DbSet<CashInBank> CashInBank { get; set; }
        public DbSet<ChequePayment> ChequePayment { get; set; }
        public DbSet<ChequeReceipt> ChequeReceipt { get; set; }
        public DbSet<Voucher> Voucher { get; set; }
        public DbSet<VoucherItems> VoucherItems { get; set; }
        // Voucher

    }
}
