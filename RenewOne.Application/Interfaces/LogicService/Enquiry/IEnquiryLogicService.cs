using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Enquiry;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry
{
    public interface IEnquiryLogicService
    {
        Task<Result<EnquiryDetailsResponse>> GetEnquiryAsync(Guid enquiryId);
    }
}
