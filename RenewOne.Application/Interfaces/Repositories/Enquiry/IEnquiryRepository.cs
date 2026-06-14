using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;
namespace RenewOne.Application.Interfaces.Repositories.Enquiry
{
    public interface IEnquiryRepository
    {
        Task<EnquiryEntity?> GetByIdAsync(Guid enquiryId);
        public IQueryable<EnquiryEntity> GetEnquiries();
    }
}
