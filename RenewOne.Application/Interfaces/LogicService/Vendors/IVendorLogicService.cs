using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Vendor;
using RenewOne.Application.DTOs.Response.Vendor;

namespace RenewOne.Application.Interfaces.LogicService.Vendors
{
    public interface IVendorLogicService
    {
        Task<Result<VendorResponse>> GetVendorAsync(Guid id,CancellationToken ct);
        Task<Result<VendorResponse>> CreateVendorAsync(CreateVendorRequest request, CancellationToken ct);
        Task<Result<List<VendorResponse>>> GetVendorsAsync(CancellationToken ct);
        Task<Result<VendorResponse>> UpdateVendorAsync(Guid id,UpdateVendorRequest request,CancellationToken ct);
        Task<Result<bool>> ToggleVendorStatusAsync(Guid id, CancellationToken ct);
    }
}
