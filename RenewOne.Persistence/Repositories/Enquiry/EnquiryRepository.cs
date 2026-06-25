using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Domain.Enums;
using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;

namespace RenewOne.Persistence.Repositories.Enquiry
{
    public class EnquiryRepository : IEnquiryRepository
    {
        #region DI
        private readonly AppDbContext _dbContext;
        #endregion

        #region Constructor
        public EnquiryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        #region GetEnquiryById
        public async Task<EnquiryEntity?> GetByIdAsync(Guid enquiryId)
        {
            return await _dbContext.Enquiries
                .Include(x => x.InsuranceType)
                .Include(x => x.Documents)
                .Include(x => x.AssignedEmployee)
                .Include(x => x.CreatedByUser)
                .Include(x => x.EnquiryQuotes)
                    .ThenInclude(x => x.InsuranceCompany)
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleDetails)
                .FirstOrDefaultAsync(x => x.Id == enquiryId);
        }
        #endregion

        #region Queryable1
        public IQueryable<EnquiryEntity> GetEnquiries()
        {
            return _dbContext.Enquiries
                .Include(e => e.VehicleEnquiryDetails)
                    .ThenInclude(v => v.VehicleDetails)
                .Include(e => e.InsuranceType)
                .Include(e => e.Documents);
        }
        #endregion
        public IQueryable<EnquiryEntity> GetEnquiryQueryable()
        {
            return _dbContext.Enquiries
                .AsNoTracking();
        }

        public IQueryable<EnquiryEntity> GetCommonQueue()
        {
            return _dbContext.Enquiries
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleType)
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleDetails)
                .Include(x => x.CreatedByUser)
                .Where(x =>
                    x.Status == EnquiryStatus.Submitted.ToString() ||
                    x.Status == EnquiryStatus.StartQuote.ToString());
        }

        public IQueryable<EnquiryEntity> GetQueryable()
        {
            return _dbContext.Enquiries
                .Include(x => x.CreatedByUser)
                .Include(x => x.InsuranceType)
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleDetails)
                .Include(x => x.VehicleEnquiryDetails)
                    .ThenInclude(x => x.VehicleType)
                .Include(x => x.Documents)
                .Include(x => x.EnquiryQuotes);
        }
    }
}
