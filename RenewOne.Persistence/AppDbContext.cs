using Microsoft.EntityFrameworkCore;
using RenewOne.Domain.Common;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Identity & Accounts
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }

        // Master Data
        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<AgentClass> AgentClasses { get; set; }
        public DbSet<PricingMaster> PricingMasters { get; set; }

        // Core Business
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<EnquiryQuote> EnquiryQuotes { get; set; }
        public DbSet<VehicleEnquiryDetails> VehicleEnquiryDetails { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; }
        public DbSet<VehicleDetails> VehicleDetails { get; set; }
        public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PaymentOrder> PaymentOrders { get; set; }
        public DbSet<UserTaskAllocation> UserTaskAllocations { get; set; }
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<LedgerTransaction> LedgerTransactions { get; set; }
        public DbSet<PolicyPayment> PolicyPayments { get; set; }
        public DbSet<QRMapping> QRMappings { get; set; }
        public DbSet<QRMaster> QRMasters { get; set; }
        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<UserModulePermission> UserModulePermissions { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorTransaction> VendorTransaction { get; set; }
        public DbSet<PolicyDetails> PolicyDetails { get; set; }
        public DbSet<InsuranceProductDisplay> InsuranceProductDisplays { get; set; }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PricingMaster>(entity =>
            {
                entity.ToTable("PricingMasters");
                entity.Property(p => p.Id).IsRequired();
            });

            modelBuilder.Entity<Enquiry>()
                .HasOne(e => e.VehicleEnquiryDetails)
                .WithOne(v => v.Enquiry)
                .HasForeignKey<VehicleEnquiryDetails>(v => v.EnquiryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.AgentClass)
                .WithMany(ac => ac.Accounts)
                .HasForeignKey(a => a.AgentClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // AgentClass
            modelBuilder.Entity<AgentClass>()
                .Property(a => a.CommSharePercentage).HasPrecision(18, 2);

            // PricingMaster
            modelBuilder.Entity<PricingMaster>()
                .Property(p => p.TotalPremium).HasPrecision(18, 2);
            modelBuilder.Entity<PricingMaster>()
                .Property(p => p.CompanyPayable).HasPrecision(18, 2);
            modelBuilder.Entity<PricingMaster>()
                .Property(p => p.AdminProfit).HasPrecision(18, 2);

            modelBuilder.Entity<VehicleEnquiryDetails>()
                .HasOne(v => v.VehicleDetails)
                .WithMany(vd => vd.EnquiryDetails)
                .HasForeignKey(v => v.VehicleDetailsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enquiry>()
                .HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedEnquiries)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enquiry>()
                .HasOne(e => e.AssignedEmployee)
                .WithMany(u => u.AssignedEnquiries)
                .HasForeignKey(e => e.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserTaskAllocation>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserTaskAllocations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTaskAllocation>()
                .HasOne(x => x.InsuranceType)
                .WithMany(x => x.UserTaskAllocations)
                .HasForeignKey(x => x.InsuranceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserTaskAllocation>()
                .HasOne(x => x.VehicleType)
                .WithMany(x => x.UserTaskAllocations)
                .HasForeignKey(x => x.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);


            modelBuilder.Entity<AssignmentHistory>(entity =>
            {
                entity.HasOne(d => d.Enquiry)
                    .WithMany(p => p.AssignmentHistories)
                    .HasForeignKey(d => d.EnquiryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ChangedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.FromEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.FromEmployeeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.ToEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.ToEmployeeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Account>()
                .HasOne(a => a.AgentClass)
                .WithMany(ac => ac.Accounts)
                .HasForeignKey(a => a.AgentClassId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PolicyPayment>()
                .HasOne(x => x.Enquiry)
                .WithMany(x => x.PolicyPayments)
                .HasForeignKey(x => x.EnquiryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PolicyPayment>()
                .HasOne(x => x.EnquiryQuote)
                .WithMany(x => x.PolicyPayments)
                .HasForeignKey(x => x.EnquiryQuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PolicyPayment>()
                .HasOne(x => x.Account)
                .WithMany(x => x.PolicyPayments)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PolicyPayment>()
                .HasOne(x => x.SubmittedByUser)
                .WithMany(x => x.SubmittedPolicyPayments)
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PolicyPayment>()
                .HasOne(x => x.VerifiedByUser)
                .WithMany(x => x.VerifiedPolicyPayments)
                .HasForeignKey(x => x.VerifiedbyUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QRMapping>()
                .HasOne(x => x.QRMaster)
                .WithMany(x => x.QRMappings)
                .HasForeignKey(x => x.QRMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QRMapping>()
                .HasOne(x => x.InsuranceType)
                .WithMany(x => x.QRMappings)
                .HasForeignKey(x => x.InsuranceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QRMapping>()
                .HasOne(x => x.VehicleType)
                .WithMany(x => x.QRMappings)
                .HasForeignKey(x => x.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PricingMaster>()
                .HasOne(p => p.InsuranceCompany)
                .WithMany(i => i.PricingMasters)
                .HasForeignKey(p => p.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserModulePermission>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserModulePermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            var seedDate = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc);


            modelBuilder.Entity<AgentClass>().HasData(
                new AgentClass
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    ClassName = "Premium",
                    CommSharePercentage = 15,
                    CreatedAt = seedDate
                },
                new AgentClass
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    ClassName = "Standard",
                    CommSharePercentage = 10,
                    CreatedAt = seedDate
                },
                new AgentClass
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    ClassName = "Basic",
                    CommSharePercentage = 5,
                    CreatedAt = seedDate
                }
            );


            modelBuilder.Entity<Enquiry>()
                .HasIndex(e => new { e.AssignedEmployeeId, e.CreatedAt });
        }
    }
}
