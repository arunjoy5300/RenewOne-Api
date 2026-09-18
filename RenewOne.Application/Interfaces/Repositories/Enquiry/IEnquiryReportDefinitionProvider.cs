using RenewOne.Domain.Reports.Enquiries.Models;

namespace RenewOne.Application.Interfaces.Repositories.Enquiry
{
    public interface IEnquiryReportDefinitionProvider
    {
        IReadOnlyList<EnquiryReportDefinition> GetDefinitions();
    }
}
