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
        public DbSet<EnquiryPricing> EnquiryPricings { get; set; }
        public DbSet<VehicleEnquiryDetails> VehicleEnquiryDetails { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; }

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
                    entity.CreatedAt = DateTime.Now;
                }
                else
                {
                    entity.UpdatedAt = DateTime.Now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // EnquiryPricing
            modelBuilder.Entity<EnquiryPricing>()
                .Property(ep => ep.TotalPremium).HasPrecision(18, 2);
            modelBuilder.Entity<EnquiryPricing>()
                .Property(ep => ep.CompanyPayable).HasPrecision(18, 2);
            modelBuilder.Entity<EnquiryPricing>()
                .Property(ep => ep.AdminProfit).HasPrecision(18, 2);

            modelBuilder.Entity<AssignmentHistory>(entity =>
            {
                entity.HasOne(d => d.Enquiry)
                    .WithMany(p => p.AssignmentHistories)
                    .HasForeignKey(d => d.EnquiryId)
                    .OnDelete(DeleteBehavior.Cascade); // എൻക്വയറി ഡിലീറ്റ് ആയാൽ ഹിസ്റ്ററി പോകണം

                entity.HasOne(d => d.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ChangedByUserId)
                    .OnDelete(DeleteBehavior.NoAction); // ഇവിടെ NoAction നൽകുക

                entity.HasOne(d => d.FromEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.FromEmployeeId)
                    .OnDelete(DeleteBehavior.NoAction); // ഇവിടെ NoAction നൽകുക

                entity.HasOne(d => d.ToEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.ToEmployeeId)
                    .OnDelete(DeleteBehavior.NoAction); // ഇവിടെ NoAction നൽകുക
            });

            // Accounts ടേബിളിലും കാസ്കേഡ് പ്രശ്നം വരാതിരിക്കാൻ ഇത് നൽകാം
            modelBuilder.Entity<Account>()
                .HasOne(a => a.AgentClass)
                .WithMany(ac => ac.Accounts)
                .HasForeignKey(a => a.AgentClassId)
                .OnDelete(DeleteBehavior.NoAction);

            // ഒരു ഫിക്സഡ് ഡേറ്റ് ക്രിയേറ്റ് ചെയ്യുക
            var seedDate = new DateTime(2026, 05, 12);

            modelBuilder.Entity<AgentClass>().HasData(
                new AgentClass
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    ClassName = "Premium",
                    CommSharePercentage = 15,
                    CreatedAt = seedDate // DateTime.Now മാറ്റുക
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
        }
    }
}
