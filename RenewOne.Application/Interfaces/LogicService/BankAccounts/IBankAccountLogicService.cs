using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.BankAccount;
using RenewOne.Application.DTOs.Response.BankAccount;
using RenewOne.Application.Filters.BankAccount;

namespace RenewOne.Application.Interfaces.LogicService.BankAccounts
{
    public interface IBankAccountLogicService
    {
        Task<Result<Guid>> CreateAsync(CreateBankAccountRequest request);

        Task<Result<string>> UpdateAsync(UpdateBankAccountRequest request);

        Task<Result<string>> ChangeStatusAsync(Guid id, bool isActive);

        Task<Result<BankAccountResponse>> GetByIdAsync(Guid id);

        Task<Result<PagedResponse<BankAccountResponse>>> GetAllAsync(BankAccountFilter filter);
        Task<Result<BankTransactionResponse>> GetBankTransactionsAsync(BankTransactionFilterRequest? filter, CancellationToken cancellationToken = default);
    }
}
