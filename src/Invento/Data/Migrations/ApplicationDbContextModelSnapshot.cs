using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Invento.Data;

namespace Invento.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.Bank", b =>
                {
                    b.Property<int>("BankID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BankDescription");

                    b.Property<string>("BankName")
                        .IsRequired();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<int>("TransactionAccountID");

                    b.HasKey("BankID");

                    b.ToTable("Bank");
                });

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.CompanyProfile", b =>
                {
                    b.Property<int>("CompanyProfileID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .IsRequired();

                    b.Property<string>("AlternativeContact");

                    b.Property<string>("City")
                        .IsRequired();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("CompanyWebsite");

                    b.Property<string>("ContactNumber");

                    b.Property<int>("CountryID");

                    b.Property<string>("Fax");

                    b.Property<byte[]>("FileData");

                    b.Property<string>("FileName");

                    b.Property<int>("NoOfCompanyUsersAllowed");

                    b.Property<string>("OfficeContact");

                    b.HasKey("CompanyProfileID");

                    b.HasIndex("CountryID");

                    b.ToTable("CompanyProfile");
                });

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.Country", b =>
                {
                    b.Property<int>("CountryID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("ISO");

                    b.Property<string>("Iso3");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("NiceName");

                    b.Property<string>("NumCode");

                    b.Property<string>("PhoneCode");

                    b.HasKey("CountryID");

                    b.ToTable("Country");
                });

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.Currency", b =>
                {
                    b.Property<int>("CurrencyID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("CurrencyName")
                        .IsRequired();

                    b.Property<string>("ISO");

                    b.HasKey("CurrencyID");

                    b.ToTable("Currency");
                });

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.Parties", b =>
                {
                    b.Property<int>("PartiesID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdditionalInfo1");

                    b.Property<string>("AdditionalInfo2");

                    b.Property<string>("Area");

                    b.Property<string>("BusinessRelation");

                    b.Property<string>("City");

                    b.Property<int>("CompanyID");

                    b.Property<string>("ContactPerson");

                    b.Property<int>("CountryID");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("Email");

                    b.Property<bool>("ExtraBool");

                    b.Property<string>("ExtraString");

                    b.Property<string>("Fax");

                    b.Property<string>("Observations");

                    b.Property<string>("OtherDetails");

                    b.Property<string>("PartyName")
                        .IsRequired();

                    b.Property<string>("PartyShortName");

                    b.Property<string>("Phone1");

                    b.Property<string>("Phone2");

                    b.Property<string>("Remarks");

                    b.Property<string>("Road");

                    b.Property<string>("State");

                    b.Property<int?>("TransactionAccountID");

                    b.HasKey("PartiesID");

                    b.HasIndex("CountryID");

                    b.HasIndex("TransactionAccountID")
                        .IsUnique();

                    b.ToTable("Parties");
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.CashFlow", b =>
                {
                    b.Property<int>("CashFlowID")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CashInBankID");

                    b.Property<int?>("CashPaymentID");

                    b.Property<int?>("CashReceiptID");

                    b.Property<int?>("ChequePaymentID");

                    b.Property<int?>("ChequeReceiptID");

                    b.Property<int>("CompanyID");

                    b.Property<decimal>("Credit")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<DateTime>("DateCreation");

                    b.Property<decimal>("Debit")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("Details");

                    b.Property<int?>("MainAccountID");

                    b.Property<string>("Narration");

                    b.Property<int?>("PartiesID");

                    b.Property<int?>("PurchaseBillID");

                    b.Property<int?>("PurchaseReturnID");

                    b.Property<int?>("SaleBillID");

                    b.Property<int?>("SaleReturnID");

                    b.Property<int?>("SubAccountID");

                    b.Property<int?>("TransactionAccountID");

                    b.Property<int?>("VoucherItemsID");

                    b.Property<string>("VoucherType");

                    b.HasKey("CashFlowID");

                    b.HasIndex("CashInBankID");

                    b.HasIndex("CashPaymentID");

                    b.HasIndex("CashReceiptID");

                    b.HasIndex("ChequePaymentID");

                    b.HasIndex("ChequeReceiptID");

                    b.HasIndex("MainAccountID");

                    b.HasIndex("PartiesID");

                    b.HasIndex("PurchaseBillID");

                    b.HasIndex("PurchaseReturnID");

                    b.HasIndex("SaleBillID");

                    b.HasIndex("SaleReturnID");

                    b.HasIndex("SubAccountID");

                    b.HasIndex("TransactionAccountID");

                    b.HasIndex("VoucherItemsID");

                    b.ToTable("CashFlow");
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.MainAccount", b =>
                {
                    b.Property<int>("MainAccountID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountName")
                        .IsRequired();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("MainAccountNumber")
                        .IsRequired();

                    b.HasKey("MainAccountID");

                    b.ToTable("MainAccount");
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.SubAccount", b =>
                {
                    b.Property<int>("SubAccountID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountName")
                        .IsRequired();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("MainAccountID");

                    b.Property<string>("SubAccountNumber")
                        .IsRequired();

                    b.HasKey("SubAccountID");

                    b.HasIndex("MainAccountID");

                    b.ToTable("SubAccount");
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.TransactionAccount", b =>
                {
                    b.Property<int>("TransactionAccountID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountName")
                        .IsRequired();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<decimal>("OpeningBalance")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("SubAccountID");

                    b.Property<string>("TransactionAccountNumber")
                        .IsRequired();

                    b.HasKey("TransactionAccountID");

                    b.HasIndex("SubAccountID");

                    b.ToTable("TransactionAccount");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.CashInBank", b =>
                {
                    b.Property<int>("CashInBankID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("AmountInWords");

                    b.Property<int?>("BankID");

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("CurrencyID");

                    b.Property<DateTime>("Date");

                    b.Property<string>("DepositedBy");

                    b.Property<string>("ExternalRef");

                    b.Property<string>("Particulars");

                    b.Property<int>("PartiesID");

                    b.HasKey("CashInBankID");

                    b.HasIndex("BankID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("CashInBank");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.CashPayment", b =>
                {
                    b.Property<int>("CashPaymentID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("AmountInWords");

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("CurrencyID");

                    b.Property<DateTime>("Date");

                    b.Property<string>("ExternalRef");

                    b.Property<string>("ImportExportID");

                    b.Property<string>("Particulars");

                    b.Property<int>("PartiesID");

                    b.Property<string>("Payee");

                    b.HasKey("CashPaymentID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("CashPayment");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.CashReceipt", b =>
                {
                    b.Property<int>("CashReceiptID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("AmountInWords");

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("CurrencyID");

                    b.Property<DateTime>("Date");

                    b.Property<string>("ExternalRef");

                    b.Property<string>("PaidBy");

                    b.Property<string>("Particulars");

                    b.Property<int>("PartiesID");

                    b.HasKey("CashReceiptID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("CashReceipt");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.ChequePayment", b =>
                {
                    b.Property<int>("ChequePaymentID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("AmountInWords");

                    b.Property<int>("BankID");

                    b.Property<string>("ChequeNumber");

                    b.Property<int>("ChequeStatus");

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("CurrencyID");

                    b.Property<string>("CurrentStatus");

                    b.Property<DateTime>("Date");

                    b.Property<DateTime>("DateOfDeposite");

                    b.Property<DateTime>("DateOfMature");

                    b.Property<string>("ExternalRef");

                    b.Property<string>("ImportExportID");

                    b.Property<string>("InNameOf");

                    b.Property<string>("Particulars");

                    b.Property<int>("PartiesID");

                    b.HasKey("ChequePaymentID");

                    b.HasIndex("BankID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("ChequePayment");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.ChequeReceipt", b =>
                {
                    b.Property<int>("ChequeReceiptID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("AmountInWords");

                    b.Property<int>("BankID");

                    b.Property<string>("ChequeNumber");

                    b.Property<int>("ChequeStatus");

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("CurrencyID");

                    b.Property<string>("CurrentStatus");

                    b.Property<DateTime>("Date");

                    b.Property<DateTime>("DateOfDeposite");

                    b.Property<DateTime>("DateOfMature");

                    b.Property<string>("ExternalRef");

                    b.Property<string>("InNameOf");

                    b.Property<string>("Particulars");

                    b.Property<int>("PartiesID");

                    b.HasKey("ChequeReceiptID");

                    b.HasIndex("BankID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("ChequeReceipt");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.Voucher", b =>
                {
                    b.Property<int>("VoucherID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("CurrencyID");

                    b.Property<DateTime>("Date");

                    b.Property<string>("ExternalRef");

                    b.Property<string>("ImportExportID");

                    b.Property<string>("Particulars");

                    b.HasKey("VoucherID");

                    b.HasIndex("CurrencyID");

                    b.ToTable("Voucher");
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.VoucherItems", b =>
                {
                    b.Property<int>("VoucherItemsID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<decimal>("Credit")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("Debit")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("MainAccountID");

                    b.Property<string>("Narration");

                    b.Property<int>("SubAccountID");

                    b.Property<int>("TransactionAccountID");

                    b.Property<int>("VoucherID");

                    b.HasKey("VoucherItemsID");

                    b.HasIndex("MainAccountID");

                    b.HasIndex("SubAccountID");

                    b.HasIndex("TransactionAccountID");

                    b.HasIndex("VoucherID");

                    b.ToTable("VoucherItems");
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.GRN", b =>
                {
                    b.Property<int>("GRNID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<string>("ContactNumber");

                    b.Property<string>("ContactPerson");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("GRNDate");

                    b.Property<int>("GRN_I_Extra");

                    b.Property<int>("PartiesID");

                    b.Property<string>("Remarks");

                    b.Property<int>("TotalQuantity");

                    b.HasKey("GRNID");

                    b.HasIndex("PartiesID");

                    b.ToTable("GRN");
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.GRNItem", b =>
                {
                    b.Property<int>("GRNItemID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<int>("GRNID");

                    b.Property<int>("ItemID");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("GRNItemID");

                    b.HasIndex("GRNID");

                    b.HasIndex("ItemID");

                    b.ToTable("GRNItem");
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.Item", b =>
                {
                    b.Property<int>("ItemID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("CrossRef");

                    b.Property<DateTime?>("Date");

                    b.Property<decimal?>("ItemExtra_Dec_1");

                    b.Property<decimal?>("ItemExtra_Dec_2");

                    b.Property<decimal?>("ItemExtra_Dec_3");

                    b.Property<decimal?>("ItemExtra_Dec_4");

                    b.Property<decimal?>("ItemExtra_Dec_5");

                    b.Property<int?>("ItemExtra_Int_1");

                    b.Property<int?>("ItemExtra_Int_2");

                    b.Property<int?>("ItemExtra_Int_3");

                    b.Property<int?>("ItemExtra_Int_4");

                    b.Property<int?>("ItemExtra_Int_5");

                    b.Property<string>("ItemExtra_String_1");

                    b.Property<string>("ItemExtra_String_2");

                    b.Property<string>("ItemExtra_String_3");

                    b.Property<string>("ItemExtra_String_4");

                    b.Property<string>("ItemExtra_String_5");

                    b.Property<string>("ItemMainCompany");

                    b.Property<string>("ItemName");

                    b.Property<string>("ItemType");

                    b.Property<string>("ItemType2");

                    b.Property<decimal>("LCPrice")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<bool?>("NotMappedBool");

                    b.Property<int?>("NotMappedInt");

                    b.Property<string>("NotMappedString_1");

                    b.Property<string>("NotMappedString_2");

                    b.Property<string>("OEMNo");

                    b.Property<byte[]>("PhotoData");

                    b.Property<string>("ProductDescription");

                    b.Property<int?>("ProductGroupID");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("Remark");

                    b.Property<decimal?>("SalePrice");

                    b.Property<string>("Size");

                    b.Property<decimal?>("Value");

                    b.HasKey("ItemID");

                    b.HasIndex("ProductGroupID");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.ProductGroup", b =>
                {
                    b.Property<int>("ProductGroupID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CheckCase");

                    b.Property<int>("CompanyID");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("ProductGroupName")
                        .IsRequired();

                    b.HasKey("ProductGroupID");

                    b.ToTable("ProductGroup");
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseBill", b =>
                {
                    b.Property<int>("PurchaseBillID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Advance")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<DateTime>("BillDate");

                    b.Property<decimal>("CashPaid")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("CompanyID");

                    b.Property<string>("ContactNumber");

                    b.Property<string>("ContactPerson");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("CreditDays");

                    b.Property<int>("CurrencyID");

                    b.Property<int>("ExchangeRate");

                    b.Property<string>("ExternalRef");

                    b.Property<decimal>("GrossTotal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("NetAmount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("PB_1");

                    b.Property<int>("PB_2");

                    b.Property<bool>("PB_B_1");

                    b.Property<decimal>("PB_D_1")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("PB_D_2")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("PB_S_1");

                    b.Property<string>("PB_S_2");

                    b.Property<int?>("PartiesID");

                    b.Property<int?>("PayTerms");

                    b.Property<string>("PurchaseBillNo")
                        .IsRequired();

                    b.Property<int>("PurchaseImport");

                    b.Property<DateTime>("RefDate");

                    b.Property<string>("Remarks");

                    b.Property<string>("TDiscount");

                    b.Property<decimal>("TLandingExpenses")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("TotalQuantity")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("PurchaseBillID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("PurchaseBill");
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseBillItem", b =>
                {
                    b.Property<int>("PurchaseBillItemID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<int>("ItemID");

                    b.Property<bool>("PurchaseBillExtraBool");

                    b.Property<decimal>("PurchaseBillExtraDecimal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("PurchaseBillExtraDecimal_1")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("PurchaseBillExtraInt");

                    b.Property<int>("PurchaseBillExtraInt_1");

                    b.Property<int>("PurchaseBillExtraInt_2");

                    b.Property<string>("PurchaseBillExtraString");

                    b.Property<string>("PurchaseBillExtraString_2");

                    b.Property<int>("PurchaseBillID");

                    b.Property<decimal>("PurchasePrice")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("PurchaseBillItemID");

                    b.HasIndex("ItemID");

                    b.HasIndex("PurchaseBillID");

                    b.ToTable("PurchaseBillItem");
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseReturn", b =>
                {
                    b.Property<int>("PurchaseReturnID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("AmountToReceive")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("CompanyID");

                    b.Property<string>("ContactNumber");

                    b.Property<string>("ContactPerson");

                    b.Property<string>("CreatedBy");

                    b.Property<int>("ItemID");

                    b.Property<decimal>("OldQuantity")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<bool>("PR_B_1");

                    b.Property<decimal>("PR_D_1")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("PR_D_2")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("PR_I_1");

                    b.Property<int>("PR_I_2");

                    b.Property<string>("PR_S_1");

                    b.Property<string>("PR_S_2");

                    b.Property<int?>("PartiesID");

                    b.Property<DateTime>("PurBillReturnDate");

                    b.Property<int>("PurchaseBillID");

                    b.Property<string>("Remarks");

                    b.Property<decimal>("ReturnQuantity")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("PurchaseReturnID");

                    b.HasIndex("ItemID");

                    b.HasIndex("PartiesID");

                    b.HasIndex("PurchaseBillID");

                    b.ToTable("PurchaseReturn");
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseReturnTransaction", b =>
                {
                    b.Property<int>("PurchaseReturnTransactionID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int?>("BankID");

                    b.Property<string>("Cheque");

                    b.Property<int>("CompanyID");

                    b.Property<DateTime>("Date");

                    b.Property<int?>("Mode");

                    b.Property<bool>("Paid");

                    b.Property<int>("PurchaseBillID");

                    b.Property<int>("PurchaseReturnID");

                    b.Property<bool>("PurchaseTransactionExtraBool");

                    b.Property<decimal>("PurchaseTransactionExtraDecimal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("PurchaseTransactionExtraInt");

                    b.Property<string>("PurchaseTransactionExtraString");

                    b.HasKey("PurchaseReturnTransactionID");

                    b.HasIndex("BankID");

                    b.HasIndex("PurchaseReturnID");

                    b.ToTable("PurchaseReturnTransaction");
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.Transaction", b =>
                {
                    b.Property<int>("TransactionID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int?>("BankID");

                    b.Property<string>("Cheque");

                    b.Property<int>("CompanyID");

                    b.Property<DateTime>("Date");

                    b.Property<int?>("Mode");

                    b.Property<bool>("Paid");

                    b.Property<int?>("PurchaseBillID");

                    b.Property<bool>("PurchaseTransactionExtraBool");

                    b.Property<decimal>("PurchaseTransactionExtraDecimal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("PurchaseTransactionExtraInt");

                    b.Property<string>("PurchaseTransactionExtraString");

                    b.HasKey("TransactionID");

                    b.HasIndex("BankID");

                    b.HasIndex("PurchaseBillID");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleBill", b =>
                {
                    b.Property<int>("SaleBillID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Advance")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<DateTime>("BillDate");

                    b.Property<decimal>("CashPaid")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("CompanyID");

                    b.Property<string>("ContactNumber");

                    b.Property<string>("ContactPerson");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("CreditDays");

                    b.Property<int>("CurrencyID");

                    b.Property<int>("ExchangeRate");

                    b.Property<string>("ExternalRef");

                    b.Property<decimal>("GrossTotal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("NetAmount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int?>("PartiesID");

                    b.Property<int?>("PayTerms");

                    b.Property<DateTime>("RefDate");

                    b.Property<string>("Remarks");

                    b.Property<int>("SB_1");

                    b.Property<int>("SB_2");

                    b.Property<bool>("SB_B_1");

                    b.Property<decimal>("SB_D_1")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("SB_D_2")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<string>("SB_S_1");

                    b.Property<string>("SB_S_2");

                    b.Property<string>("SaleBillNo")
                        .IsRequired();

                    b.Property<string>("TDiscount");

                    b.Property<decimal>("TotalQuantity")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("SaleBillID");

                    b.HasIndex("CurrencyID");

                    b.HasIndex("PartiesID");

                    b.ToTable("SaleBill");
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleBillItem", b =>
                {
                    b.Property<int>("SaleBillItemID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyID");

                    b.Property<int>("ItemID");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<bool>("SaleBillExtraBool");

                    b.Property<decimal>("SaleBillExtraDecimal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("SaleBillExtraDecimal_1")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("SaleBillExtraInt");

                    b.Property<int>("SaleBillExtraInt_1");

                    b.Property<int>("SaleBillExtraInt_2");

                    b.Property<string>("SaleBillExtraString");

                    b.Property<int>("SaleBillID");

                    b.Property<string>("SaleExtraString_2");

                    b.Property<decimal>("SalePrice")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("SaleBillItemID");

                    b.HasIndex("ItemID");

                    b.HasIndex("SaleBillID");

                    b.ToTable("SaleBillItem");
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleReturn", b =>
                {
                    b.Property<int>("SaleReturnID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("AmountToPay")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("CompanyID");

                    b.Property<string>("ContactNumber");

                    b.Property<string>("ContactPerson");

                    b.Property<string>("CreatedBy");

                    b.Property<int>("ItemID");

                    b.Property<decimal>("OldQuantity")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int?>("PartiesID");

                    b.Property<string>("Remarks");

                    b.Property<decimal>("ReturnQuantity")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<bool>("SR_B_1");

                    b.Property<decimal>("SR_D_1")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<decimal>("SR_D_2")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("SR_I_1");

                    b.Property<int>("SR_I_2");

                    b.Property<string>("SR_S_1");

                    b.Property<string>("SR_S_2");

                    b.Property<int>("SaleBillID");

                    b.Property<DateTime>("SaleBillReturnDate");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18, 6)");

                    b.HasKey("SaleReturnID");

                    b.HasIndex("ItemID");

                    b.HasIndex("PartiesID");

                    b.HasIndex("SaleBillID");

                    b.ToTable("SaleReturn");
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleReturnTransaction", b =>
                {
                    b.Property<int>("SaleTransactionID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int?>("BankID");

                    b.Property<string>("Cheque");

                    b.Property<int>("CompanyID");

                    b.Property<DateTime>("Date");

                    b.Property<int?>("Mode");

                    b.Property<bool>("Paid");

                    b.Property<int>("SaleBillID");

                    b.Property<int>("SaleReturnID");

                    b.Property<bool>("SaleTransactionExtraBool");

                    b.Property<decimal>("SaleTransactionExtraDecimal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("SaleTransactionExtraInt");

                    b.Property<string>("SaleTransactionExtraString");

                    b.HasKey("SaleTransactionID");

                    b.HasIndex("BankID");

                    b.HasIndex("SaleReturnID");

                    b.ToTable("SaleReturnTransaction");
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleTransaction", b =>
                {
                    b.Property<int>("SaleTransactionID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int?>("BankID");

                    b.Property<string>("Cheque");

                    b.Property<int>("CompanyID");

                    b.Property<DateTime>("Date");

                    b.Property<int?>("Mode");

                    b.Property<bool>("Paid");

                    b.Property<int>("SaleBillID");

                    b.Property<bool>("SaleTransactionExtraBool");

                    b.Property<decimal>("SaleTransactionExtraDecimal")
                        .HasColumnType("decimal(18, 6)");

                    b.Property<int>("SaleTransactionExtraInt");

                    b.Property<string>("SaleTransactionExtraString");

                    b.HasKey("SaleTransactionID");

                    b.HasIndex("BankID");

                    b.HasIndex("SaleBillID");

                    b.ToTable("SaleTransaction");
                });

            modelBuilder.Entity("Invento.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<bool>("AccountActive");

                    b.Property<int>("CompanyID");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.CompanyProfile", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Country", "Country")
                        .WithMany("CompanyProfile")
                        .HasForeignKey("CountryID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.CompanyAdmin.Models.Company.Parties", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Country", "Country")
                        .WithMany("Parties")
                        .HasForeignKey("CountryID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Finance.Models.TransactionAccount", "TransactionAccount")
                        .WithOne("Parties")
                        .HasForeignKey("Invento.Areas.CompanyAdmin.Models.Company.Parties", "TransactionAccountID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.CashFlow", b =>
                {
                    b.HasOne("Invento.Areas.Payment.Models.CashInBank", "CashInBank")
                        .WithMany("CashFlow")
                        .HasForeignKey("CashInBankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Payment.Models.CashPayment", "CashPayment")
                        .WithMany("CashFlow")
                        .HasForeignKey("CashPaymentID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Payment.Models.CashReceipt", "CashReceipt")
                        .WithMany("CashFlow")
                        .HasForeignKey("CashReceiptID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Payment.Models.ChequePayment", "ChequePayment")
                        .WithMany("CashFlow")
                        .HasForeignKey("ChequePaymentID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Payment.Models.ChequeReceipt", "ChequeReceipt")
                        .WithMany("CashFlow")
                        .HasForeignKey("ChequeReceiptID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Finance.Models.MainAccount", "MainAccount")
                        .WithMany("CashFlow")
                        .HasForeignKey("MainAccountID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("CashFlow")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Purchase.Models.PurchaseBill", "PurchaseBill")
                        .WithMany("CashFlow")
                        .HasForeignKey("PurchaseBillID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Purchase.Models.PurchaseReturn", "PurchaseReturn")
                        .WithMany("CashFlow")
                        .HasForeignKey("PurchaseReturnID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Sale.Models.SaleBill", "SaleBill")
                        .WithMany("CashFlow")
                        .HasForeignKey("SaleBillID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Sale.Models.SaleReturn", "SaleReturn")
                        .WithMany("CashFlow")
                        .HasForeignKey("SaleReturnID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Finance.Models.SubAccount", "SubAccount")
                        .WithMany("CashFlow")
                        .HasForeignKey("SubAccountID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Finance.Models.TransactionAccount", "TransactionAccount")
                        .WithMany("CashFlow")
                        .HasForeignKey("TransactionAccountID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Payment.Models.VoucherItems", "VoucherItems")
                        .WithMany("CashFlow")
                        .HasForeignKey("VoucherItemsID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.SubAccount", b =>
                {
                    b.HasOne("Invento.Areas.Finance.Models.MainAccount", "MainAccount")
                        .WithMany("SubAccount")
                        .HasForeignKey("MainAccountID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Finance.Models.TransactionAccount", b =>
                {
                    b.HasOne("Invento.Areas.Finance.Models.SubAccount", "SubAccount")
                        .WithMany("TransactionAccount")
                        .HasForeignKey("SubAccountID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.CashInBank", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany("CashInBank")
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("CashInBank")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("CashInBank")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.CashPayment", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("CashPayment")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("CashPayment")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.CashReceipt", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("CashReceipt")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("CashReceipt")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.ChequePayment", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany("ChequePayment")
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("ChequePayment")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("ChequePayment")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.ChequeReceipt", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany("ChequeReceipt")
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("ChequeReceipt")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("ChequeReceipt")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.Voucher", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Payment.Models.VoucherItems", b =>
                {
                    b.HasOne("Invento.Areas.Finance.Models.MainAccount", "MainAccount")
                        .WithMany()
                        .HasForeignKey("MainAccountID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Finance.Models.SubAccount", "SubAccount")
                        .WithMany()
                        .HasForeignKey("SubAccountID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Finance.Models.TransactionAccount", "TransactionAccount")
                        .WithMany("VoucherItems")
                        .HasForeignKey("TransactionAccountID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Payment.Models.Voucher", "Voucher")
                        .WithMany("VoucherItems")
                        .HasForeignKey("VoucherID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.GRN", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany()
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.GRNItem", b =>
                {
                    b.HasOne("Invento.Areas.Product.Models.GRN", "GRN")
                        .WithMany("GRNItem")
                        .HasForeignKey("GRNID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Product.Models.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Product.Models.Item", b =>
                {
                    b.HasOne("Invento.Areas.Product.Models.ProductGroup", "ProductGroup")
                        .WithMany("Item")
                        .HasForeignKey("ProductGroupID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseBill", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("PurchaseBill")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("PurchaseBill")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseBillItem", b =>
                {
                    b.HasOne("Invento.Areas.Product.Models.Item", "Item")
                        .WithMany("PurchaseBillItem")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Purchase.Models.PurchaseBill", "PurchaseBill")
                        .WithMany("PurchaseBillItem")
                        .HasForeignKey("PurchaseBillID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseReturn", b =>
                {
                    b.HasOne("Invento.Areas.Product.Models.Item", "Item")
                        .WithMany("PurchaseReturn")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("PurchaseReturn")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Purchase.Models.PurchaseBill", "PurchaseBill")
                        .WithMany("PurchaseReturn")
                        .HasForeignKey("PurchaseBillID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.PurchaseReturnTransaction", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany()
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Purchase.Models.PurchaseReturn", "PurchaseReturn")
                        .WithMany("PurchaseReturnTransaction")
                        .HasForeignKey("PurchaseReturnID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Purchase.Models.Transaction", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany("Transaction")
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Purchase.Models.PurchaseBill", "PurchaseBill")
                        .WithMany("Transaction")
                        .HasForeignKey("PurchaseBillID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleBill", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Currency", "Currency")
                        .WithMany("SaleBill")
                        .HasForeignKey("CurrencyID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("SaleBill")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleBillItem", b =>
                {
                    b.HasOne("Invento.Areas.Product.Models.Item", "Item")
                        .WithMany("SaleBillItem")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Sale.Models.SaleBill", "SaleBill")
                        .WithMany("SaleBillItem")
                        .HasForeignKey("SaleBillID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleReturn", b =>
                {
                    b.HasOne("Invento.Areas.Product.Models.Item", "Item")
                        .WithMany("SaleReturn")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Parties", "Parties")
                        .WithMany("SaleReturn")
                        .HasForeignKey("PartiesID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Sale.Models.SaleBill", "SaleBill")
                        .WithMany("SaleReturn")
                        .HasForeignKey("SaleBillID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleReturnTransaction", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany()
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Sale.Models.SaleReturn", "SaleReturn")
                        .WithMany("SaleReturnTransaction")
                        .HasForeignKey("SaleReturnID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Invento.Areas.Sale.Models.SaleTransaction", b =>
                {
                    b.HasOne("Invento.Areas.CompanyAdmin.Models.Company.Bank", "Bank")
                        .WithMany()
                        .HasForeignKey("BankID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Areas.Sale.Models.SaleBill", "SaleBill")
                        .WithMany("SaleTransaction")
                        .HasForeignKey("SaleBillID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Invento.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Invento.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Invento.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
