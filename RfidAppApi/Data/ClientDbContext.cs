using Microsoft.EntityFrameworkCore;
using RfidAppApi.Models;

namespace RfidAppApi.Data
{
    public class ClientDbContext : DbContext
    {
        private readonly string _clientCode;

        public ClientDbContext(DbContextOptions<ClientDbContext> options, string clientCode) : base(options)
        {
            _clientCode = clientCode;
        }

        // Client Database - All Product and RFID Tables
        public DbSet<CategoryMaster> CategoryMasters { get; set; }
        public DbSet<ProductMaster> ProductMasters { get; set; }
        public DbSet<DesignMaster> DesignMasters { get; set; }
        public DbSet<PurityMaster> PurityMasters { get; set; }
        public DbSet<BranchMaster> BranchMasters { get; set; }
        public DbSet<CounterMaster> CounterMasters { get; set; }
        public DbSet<Rfid> Rfids { get; set; }
        public DbSet<ProductDetails> ProductDetails { get; set; }
        public DbSet<ProductRfidAssignment> ProductRfidAssignments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<DailyStockBalance> DailyStockBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names
            modelBuilder.Entity<CategoryMaster>().ToTable("tblCategoryMaster");
            modelBuilder.Entity<ProductMaster>().ToTable("tblProductMaster");
            modelBuilder.Entity<DesignMaster>().ToTable("tblDesignMaster");
            modelBuilder.Entity<PurityMaster>().ToTable("tblPurityMaster");
            modelBuilder.Entity<BranchMaster>().ToTable("tblBranchMaster");
            modelBuilder.Entity<CounterMaster>().ToTable("tblCounterMaster");
            modelBuilder.Entity<Rfid>().ToTable("tblRFID");
            modelBuilder.Entity<ProductDetails>().ToTable("tblProductDetails");
            modelBuilder.Entity<ProductRfidAssignment>().ToTable("tblProductRFIDAssignment");
            modelBuilder.Entity<Invoice>().ToTable("tblInvoice");
            modelBuilder.Entity<ProductImage>().ToTable("tblProductImage");
            modelBuilder.Entity<StockMovement>().ToTable("tblStockMovement");
            modelBuilder.Entity<DailyStockBalance>().ToTable("tblDailyStockBalance");

            // Configure relationships
            modelBuilder.Entity<CounterMaster>()
                .HasOne(c => c.Branch)
                .WithMany()
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetails>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetails>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetails>()
                .HasOne(p => p.Design)
                .WithMany()
                .HasForeignKey(p => p.DesignId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetails>()
                .HasOne(p => p.Purity)
                .WithMany()
                .HasForeignKey(p => p.PurityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetails>()
                .HasOne(p => p.Branch)
                .WithMany()
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetails>()
                .HasOne(p => p.Counter)
                .WithMany()
                .HasForeignKey(p => p.CounterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductRfidAssignment>()
                .HasOne(pr => pr.Product)
                .WithMany()
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductRfidAssignment>()
                .HasOne(pr => pr.Rfid)
                .WithMany()
                .HasForeignKey(pr => pr.RFIDCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany()
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Branch)
                .WithMany()
                .HasForeignKey(sm => sm.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Counter)
                .WithMany()
                .HasForeignKey(sm => sm.CounterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Category)
                .WithMany()
                .HasForeignKey(sm => sm.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyStockBalance>()
                .HasOne(dsb => dsb.Product)
                .WithMany()
                .HasForeignKey(dsb => dsb.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyStockBalance>()
                .HasOne(dsb => dsb.Branch)
                .WithMany()
                .HasForeignKey(dsb => dsb.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyStockBalance>()
                .HasOne(dsb => dsb.Counter)
                .WithMany()
                .HasForeignKey(dsb => dsb.CounterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyStockBalance>()
                .HasOne(dsb => dsb.Category)
                .WithMany()
                .HasForeignKey(dsb => dsb.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global Query Filter for Client Code
            modelBuilder.Entity<Rfid>().HasQueryFilter(e => e.ClientCode == _clientCode);
            modelBuilder.Entity<ProductDetails>().HasQueryFilter(e => e.ClientCode == _clientCode);
            modelBuilder.Entity<ProductRfidAssignment>().HasQueryFilter(e => e.Product.ClientCode == _clientCode);
            modelBuilder.Entity<Invoice>().HasQueryFilter(e => e.ClientCode == _clientCode);
            modelBuilder.Entity<ProductImage>().HasQueryFilter(e => e.ClientCode == _clientCode);
            modelBuilder.Entity<StockMovement>().HasQueryFilter(e => e.ClientCode == _clientCode);
            modelBuilder.Entity<DailyStockBalance>().HasQueryFilter(e => e.ClientCode == _clientCode);

            // HIGH PERFORMANCE INDEXES FOR LAKHS OF RECORDS
            // Master Data Indexes
            modelBuilder.Entity<CategoryMaster>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            modelBuilder.Entity<ProductMaster>()
                .HasIndex(p => p.ProductName)
                .IsUnique();

            modelBuilder.Entity<DesignMaster>()
                .HasIndex(d => d.DesignName)
                .IsUnique();

            modelBuilder.Entity<PurityMaster>()
                .HasIndex(p => p.PurityName)
                .IsUnique();

            modelBuilder.Entity<BranchMaster>()
                .HasIndex(b => b.BranchName)
                .IsUnique();

            // RFID Table - High Performance Indexes
            modelBuilder.Entity<Rfid>()
                .HasIndex(r => r.RFIDCode)
                .IsUnique();

            modelBuilder.Entity<Rfid>()
                .HasIndex(r => r.IsActive);

            modelBuilder.Entity<Rfid>()
                .HasIndex(r => r.CreatedOn);

            modelBuilder.Entity<Rfid>()
                .HasIndex(r => new { r.RFIDCode, r.IsActive });

            // Product Details - High Performance Indexes
            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => p.ItemCode)
                .IsUnique();

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => p.ProductId);

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => p.BranchId);

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => p.CounterId);

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => p.CreatedOn);

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => new { p.CategoryId, p.BranchId });

            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => new { p.ItemCode, p.Status });

            // Product RFID Assignment - High Performance Indexes
            modelBuilder.Entity<ProductRfidAssignment>()
                .HasIndex(pr => pr.ProductId);

            modelBuilder.Entity<ProductRfidAssignment>()
                .HasIndex(pr => pr.RFIDCode);

            modelBuilder.Entity<ProductRfidAssignment>()
                .HasIndex(pr => pr.AssignedOn);

            modelBuilder.Entity<ProductRfidAssignment>()
                .HasIndex(pr => new { pr.ProductId, pr.RFIDCode })
                .IsUnique();

            // Composite Indexes for Complex Queries
            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => new { p.CategoryId, p.BranchId, p.Status });

            modelBuilder.Entity<Rfid>()
                .HasIndex(r => new { r.IsActive, r.CreatedOn });

            // Include Indexes for Covering Queries
            modelBuilder.Entity<ProductDetails>()
                .HasIndex(p => new { p.CategoryId, p.ItemCode, p.Status, p.CreatedOn });

            modelBuilder.Entity<Rfid>()
                .HasIndex(r => new { r.IsActive, r.RFIDCode, r.CreatedOn });

            // Invoice Table - High Performance Indexes
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.ProductId);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.SoldOn);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.CreatedOn);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceType);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.RfidCode);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.ClientCode, i.SoldOn });

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.InvoiceType, i.SoldOn });

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.ProductId, i.SoldOn });

            // Product Image Table - High Performance Indexes
            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ProductId);

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ImageType);

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.IsActive);

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.CreatedOn);

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.IsActive });

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.ImageType });

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.DisplayOrder });

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.IsActive, pi.CreatedOn });

            // Stock Movement Table - High Performance Indexes
            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.ProductId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.MovementType);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.MovementDate);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.CreatedOn);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.BranchId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.CounterId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.CategoryId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.RfidCode);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.ReferenceNumber);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.ProductId, sm.MovementDate });

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.MovementType, sm.MovementDate });

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.BranchId, sm.MovementDate });

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.CounterId, sm.MovementDate });

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.CategoryId, sm.MovementDate });

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.ClientCode, sm.MovementDate });

            // Daily Stock Balance Table - High Performance Indexes
            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.ProductId);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.BalanceDate);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.CreatedOn);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.BranchId);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.CounterId);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.CategoryId);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => dsb.RfidCode);

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.ProductId, dsb.BalanceDate })
                .IsUnique();

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.BranchId, dsb.BalanceDate });

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.CounterId, dsb.BalanceDate });

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.CategoryId, dsb.BalanceDate });

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.ClientCode, dsb.BalanceDate });

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.ProductId, dsb.BranchId, dsb.BalanceDate });

            modelBuilder.Entity<DailyStockBalance>()
                .HasIndex(dsb => new { dsb.ProductId, dsb.CounterId, dsb.BalanceDate });
        }
    }
} 