using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry
{
    public interface IVehicleEnquiryLogicService
    {
        Task<Result<VehicleEnquiryResponse>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId);
        Task<Result<Guid>> UploadDocumentAsync(UploadVehicleDocumentRequest request,CancellationToken cancellationToken = default);
        Task<Result<EnquirySubmitResponse>> SubmitVehicleEnquiryAsync(SubmitVehicleEnquiry request, Guid AccountId, CancellationToken cancellationToken = default);
        Task<Result<string>> SubmitPolicyRequestAsync(SubmitPolicyRequest request, CancellationToken cancellationToken = default);
    }
}
