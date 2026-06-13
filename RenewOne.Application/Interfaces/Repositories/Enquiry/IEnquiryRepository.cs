using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;
namespace RenewOne.Application.Interfaces.Repositories.Enquiry
{
    public interface IEnquiryRepository
    {
        public IQueryable<EnquiryEntity> GetEnquiries();
    }
}
