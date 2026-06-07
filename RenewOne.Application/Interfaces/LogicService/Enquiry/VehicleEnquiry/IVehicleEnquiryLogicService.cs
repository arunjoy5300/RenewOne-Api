using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry
{
    public interface IVehicleEnquiryLogicService
    {
        Task<Result<bool>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId);
    }
}
