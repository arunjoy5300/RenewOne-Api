using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;

namespace RenewOne.Persistence.Repositories.Enquiry
{
    public sealed class EnquiryReportRepository : IEnquiryReportRepository
    {
        private readonly AppDbContext _dbContext;
        public EnquiryReportRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<EnquiryEntity> GetEnquiries()
        {
            return _dbContext.Enquiries
                .AsNoTracking()
                .Include(x => x.AssignedEmployee)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Tenant)
                .Include(x => x.InsuranceType)
                .Include(x => x.PolicyDetails)
                .Include(x => x.Documents)
                .Include(x => x.EnquiryQuotes)
                    .ThenInclude(q => q.InsuranceCompany)
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleType);
        }
    }
}
