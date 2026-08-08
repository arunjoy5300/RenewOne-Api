using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;
namespace RenewOne.Application.Interfaces.Repositories.Enquiry
{
    public interface IEnquiryRepository
    {
        Task<EnquiryEntity?> GetByIdAsync(Guid enquiryId);
        public IQueryable<EnquiryEntity> GetEnquiries();
        IQueryable<EnquiryEntity> GetEnquiryQueryable();
        IQueryable<EnquiryEntity> GetCommonQueue();
        IQueryable<EnquiryEntity> GetQueryable();
        void Update(EnquiryEntity enquiry);
        IQueryable<EnquiryEntity> GetQuoteQueue();
    }
}
