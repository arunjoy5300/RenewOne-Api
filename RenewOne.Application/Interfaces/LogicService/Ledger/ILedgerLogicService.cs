using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Wallet;

namespace RenewOne.Application.Interfaces.LogicService.Ledger
{
    public interface ILedgerLogicService
    {
        Task<Result<bool>> AdjustManualWalletAsync(WalletAdjustmentRequest request);
    }
}
