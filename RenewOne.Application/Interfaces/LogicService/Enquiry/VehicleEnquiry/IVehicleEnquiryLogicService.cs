using Microsoft.AspNetCore.Http;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Document;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry
{
    public interface IVehicleEnquiryLogicService
    {
        Task<Result<VehicleEnquiryResponse>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId);
        Task<Result<Guid>> UploadDocumentAsync(UploadVehicleDocumentRequest request,CancellationToken cancellationToken = default);
        Task<Result<EnquirySubmitResponse>> SubmitVehicleEnquiryAsync(SubmitVehicleEnquiry request, Guid AccountId, CancellationToken cancellationToken = default);
        Task<Result<string>> SubmitPolicyRequestAsync(SubmitPolicyRequest request, CancellationToken cancellationToken = default);
        Task<Result<PaymentVerificationResponse>> GetPaymentDetails(Guid enquiryId);
        Task<Result<IEnumerable<DocumentListDto>>> ListUploadedDocuments(Guid enquiryId, CancellationToken ct);
        Task<Result<bool>> DeleteUploadedDocument(Guid documentId, CancellationToken ct);
        Task<Result<bool>> ReplaceFile(ReplaceFileDto replaceFileDto, CancellationToken ct);
        Task<Result<PagedResponse<QuoteDto>>> ListVehicleQuotes(
        ListVehicleQuotesRequest request,
        CancellationToken cancellationToken = default);
        Task<Result<bool>> SubmitVehicleQuoteByAdmin(
            Guid enquiryId,
            Guid quoteId,
            CancellationToken cancellationToken = default);
        Task<Result<bool>> AddVehicleQuote(AddVehicleQuoteRequest request, CancellationToken cancellationToken = default);
    }
}
