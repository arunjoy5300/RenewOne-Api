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
        public IQueryable<EnquiryEntity> GetEnquiries()
        {
            return _dbContext.Enquiries;
        }
    }
}
