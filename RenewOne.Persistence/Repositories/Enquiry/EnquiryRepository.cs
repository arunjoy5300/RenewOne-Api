using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;

namespace RenewOne.Persistence.Repositories.Enquiry
{
    public class EnquiryRepository : IEnquiryRepository
    {
        private readonly AppDbContext _dbContext;
        public EnquiryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<EnquiryEntity?> GetByIdAsync(Guid enquiryId)
        {
            return await _dbContext.Enquiries
                .Include(x => x.InsuranceType)
                .Include(x => x.Documents)
                .Include(x => x.EnquiryQuotes)
                    .ThenInclude(x => x.InsuranceCompany)
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleDetails)
                .FirstOrDefaultAsync(x => x.Id == enquiryId);
        }
        public IQueryable<EnquiryEntity> GetEnquiries()
        {
            return _dbContext.Enquiries
                .Include(e => e.VehicleEnquiryDetails)
                    .ThenInclude(v => v.VehicleDetails)
                .Include(e => e.InsuranceType)
                .Include(e => e.Documents);
        }
    }
}
