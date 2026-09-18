using EnquiryEntity = RenewOne.Domain.Entities.Enquiry;

namespace RenewOne.Application.Interfaces.Repositories.Enquiry
{
    public interface IEnquiryReportRepository
    {
        IQueryable<EnquiryEntity> GetEnquiries();
    }
}
