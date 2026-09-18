using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Vendor;

namespace RenewOne.Application.Interfaces.LogicService.Vendors
{
    public interface IVendorTransactionLogicService
    {
        Task<Result<List<VendorTransactionResponseDto>>> GetVendorTransactionsAsync(Guid vendorId,CancellationToken ct);
        Task<Result<decimal>> GetVendorBalanceAsync(Guid vendorId,CancellationToken ct);
        Task<Result<VendorTransactionResponseDto>> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
