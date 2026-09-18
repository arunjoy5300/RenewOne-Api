using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Vendor;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Vendors;
using RenewOne.Application.Interfaces.Repositories.Vendor;

namespace RenewOne.Application.LogicService.Vendors
{
    public class VendorTransactionLogicService : IVendorTransactionLogicService
    {
        #region DI
        private readonly IVendorTransactionRepository _vendorTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVendorRepository _vendorRepository;
        #endregion

        #region Constructor
        public VendorTransactionLogicService(
            IVendorTransactionRepository vendorTransactionRepository,
            IUnitOfWork unitOfWork,
            IVendorRepository vendorRepository)
        {
            _vendorTransactionRepository = vendorTransactionRepository;
            _unitOfWork = unitOfWork;
            _vendorRepository = vendorRepository;
        }
        #endregion

        #region Get Vendor Transactions
        public async Task<Result<List<VendorTransactionResponseDto>>> GetVendorTransactionsAsync(Guid vendorId, CancellationToken ct)
        {
            var vendorExists = await _vendorRepository
                .GetQueryable()
                .AnyAsync(x => x.Id == vendorId, ct);

            if (!vendorExists)
            {
                return Result<List<VendorTransactionResponseDto>>.NotFound(
                    "Vendor not found.");
            }

            var transactions = await _vendorTransactionRepository
                .GetQueryable()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.TransactionDate)
                .Select(x => new VendorTransactionResponseDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    EnquiryId = $"ENQ-{x.Id.ToString().Substring(x.EnquiryId.ToString().Length - 4, 4).ToUpper()}",
                    TransactionType = x.TransactionType.ToString(),
                    Amount = x.Amount,
                    BalanceAfter = x.BalanceAfter,
                    ReferenceNumber = x.ReferenceNumber,
                    PaymentMode = x.PaymentMode,
                    Remarks = x.Remarks,
                    TransactionDate = x.TransactionDate
                })
                .ToListAsync(ct);

            return Result<List<VendorTransactionResponseDto>>.Success(
                transactions,
                "Vendor transactions fetched successfully.");
        }
        #endregion

        #region Get Vendor Balance
        public async Task<Result<decimal>> GetVendorBalanceAsync(Guid vendorId,CancellationToken ct)
        {
            var vendorExists = await _vendorRepository
                .GetQueryable()
                .AnyAsync(x => x.Id == vendorId, ct);

            if (!vendorExists)
            {
                return Result<decimal>.NotFound(
                    "Vendor not found.");
            }

            var balance = await _vendorTransactionRepository
                .GetQueryable()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.TransactionDate)
                .Select(x => (decimal?)x.BalanceAfter)
                .FirstOrDefaultAsync(ct);

            return Result<decimal>.Success(
                balance ?? 0,
                "Vendor balance fetched successfully.");
        }
        #endregion

        #region Get Vendor Transaction By Id
        public async Task<Result<VendorTransactionResponseDto>> GetByIdAsync(Guid id,CancellationToken ct)
        {
            var transaction = await _vendorTransactionRepository
                .GetQueryable()
                .Where(x => x.Id == id)
                .Select(x => new VendorTransactionResponseDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    EnquiryId = $"ENQ-{x.Id.ToString().Substring(x.EnquiryId.ToString().Length - 4, 4).ToUpper()}",
                    TransactionType = x.TransactionType.ToString(),
                    Amount = x.Amount,
                    BalanceAfter = x.BalanceAfter,
                    ReferenceNumber = x.ReferenceNumber,
                    PaymentMode = x.PaymentMode,
                    Remarks = x.Remarks,
                    TransactionDate = x.TransactionDate
                })
                .FirstOrDefaultAsync(ct);

            if (transaction == null)
            {
                return Result<VendorTransactionResponseDto>.NotFound(
                    "Vendor transaction not found.");
            }

            return Result<VendorTransactionResponseDto>.Success(
                transaction,
                "Vendor transaction fetched successfully.");
        }
        #endregion
    }
}
