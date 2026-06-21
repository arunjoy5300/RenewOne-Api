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
                entity.ToTable("PricingMasters"); // ടേബിൾ നെയിം ഉറപ്പിക്കുക
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

            // PricingMaster-ലേക്ക് പുതിയ InsuranceCompany ബന്ധം ചേർക്കുക
            modelBuilder.Entity<PricingMaster>()
                .HasOne(p => p.InsuranceCompany)
                .WithMany(i => i.PricingMasters)
                .HasForeignKey(p => p.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ഒരു ഫിക്സഡ് ഡേറ്റ് ക്രിയേറ്റ് ചെയ്യുക
            //var seedDate = new DateTime(2026, 05, 12);
            var seedDate = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc);


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


            modelBuilder.Entity<VehicleType>().HasData(
                // 1. ഇരുചക്ര വാഹനങ്ങൾ
                new VehicleType { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), TypeName = "Two Wheeler", Description = "Motorcycles and Scooters", IsActive = true, CreatedAt = seedDate },

                // 2. കാറുകൾ
                new VehicleType { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), TypeName = "Car - Private", Description = "Private Four Wheeler", IsActive = true, CreatedAt = seedDate },
                new VehicleType { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), TypeName = "Car - Taxi", Description = "Commercial Four Wheeler (Taxi)", IsActive = true, CreatedAt = seedDate },

                // 3. ഓട്ടോറിക്ഷകൾ (ത്രീ വീലർ)
                new VehicleType { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), TypeName = "Auto - Private", Description = "Private Three Wheeler", IsActive = true, CreatedAt = seedDate },
                new VehicleType { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), TypeName = "Auto - Taxi", Description = "Commercial Three Wheeler (Passenger)", IsActive = true, CreatedAt = seedDate },
                new VehicleType { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), TypeName = "Auto - Goods", Description = "Commercial Three Wheeler (Goods)", IsActive = true, CreatedAt = seedDate },

                // 4. വലിയ കൊമേഴ്സ്യൽ വാഹനങ്ങൾ (ഭാവിയിലേക്ക്)
                new VehicleType { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), TypeName = "Commercial Heavy", Description = "Buses and Lorries", IsActive = true, CreatedAt = seedDate }
            );

            modelBuilder.Entity<InsuranceType>().HasData(
                // 1. ഫുൾ കവർ (Comprehensive)
                new InsuranceType
                {
                    Id = Guid.Parse("A1111111-1111-1111-1111-111111111111"),
                    Name = "Vehicle Insurance",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                // 2. തേർഡ് പാർട്ടി മാത്രം (Third Party)
                new InsuranceType
                {
                    Id = Guid.Parse("A2222222-2222-2222-2222-222222222222"),
                    Name = "Health Insurance",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                // 3. സ്വന്തം വണ്ടിക്ക് മാത്രം (Standalone Own Damage - SAOD)
                new InsuranceType
                {
                    Id = Guid.Parse("A3333333-3333-3333-3333-333333333333"),
                    Name = "Life Insurance",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
        }
    }
}
