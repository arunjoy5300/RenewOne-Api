using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Application.DTOs.Response.Wallet;
using RenewOne.Application.Filters.Enquiry;

namespace RenewOne.Application.Interfaces.LogicService.Wallet
{
    public interface IWalletlogicService
    {
        Task<Result<PagedResponse<WalletTransactionResponse>>> GetTransactionsAsync(WalletTransactionFilter filter);
        Task<Result<bool>> AdjustWalletAsync(WalletAdjustmentRequest request);
        Task<Result<WalletResponse>> GetWalletAsync();
    }
}
