using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.BankAccount;
using RenewOne.Application.DTOs.Response.BankAccount;
using RenewOne.Application.Filters.BankAccount;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.BankAccounts;
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.BankAccounts
{
    public class BankAccountLogicService : IBankAccountLogicService
    {
        #region DI
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ILogger<BankAccountLogicService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBankTransactionRepository _transactionRepository;
        #endregion

        #region Constructor
        public BankAccountLogicService(
            IBankAccountRepository bankAccountRepository, 
            ILogger<BankAccountLogicService> logger, 
            IUnitOfWork unitOfWork, 
            IBankTransactionRepository transactionRepository)
        {
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _transactionRepository = transactionRepository;
        }
        #endregion

        #region Create Bank Account
        public async Task<Result<Guid>> CreateAsync(CreateBankAccountRequest request)
        {
            _logger.LogInformation("Creating new bank account.");

            try
            {
                var exists = await _bankAccountRepository
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.AccountNumber == request.AccountNumber &&
                        x.IFSCCode == request.IFSCCode);

                if (exists)
                {
                    _logger.LogWarning(
                        "Bank account already exists. AccountNumber: {AccountNumber}",
                        request.AccountNumber);

                    return Result<Guid>.Failure(
                        "A bank account with the same account number already exists.");
                }

                if (request.IsPrimary)
                {
                    var primaryAccounts = _bankAccountRepository
                        .GetQueryable()
                        .Where(x => x.IsPrimary);

                    await primaryAccounts.ExecuteUpdateAsync(x =>
                        x.SetProperty(p => p.IsPrimary, false));
                }

                var bankAccount = new BankAccount
                {
                    Id = Guid.NewGuid(),
                    AccountName = request.AccountName,
                    BankName = request.BankName,
                    BranchName = request.BranchName,
                    AccountHolderName = request.AccountHolderName,
                    AccountNumber = request.AccountNumber,
                    IFSCCode = request.IFSCCode,
                    UpiId = request.UpiId,
                    OpeningBalance = request.OpeningBalance,
                    CurrentBalance = request.OpeningBalance,
                    IsPrimary = request.IsPrimary,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _bankAccountRepository.AddAsync(bankAccount);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Bank account created successfully. Id: {Id}",
                    bankAccount.Id);

                return Result<Guid>.Success(
                    bankAccount.Id,
                    "Bank account created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating bank account.");

                return Result<Guid>.Failure(
                    "An error occurred while creating the bank account.",
                    500);
            }
        }
        #endregion

        public async Task<Result<PagedResponse<BankAccountResponse>>> GetAllAsync(BankAccountFilter filter)
        {
            try
            {
                _logger.LogInformation("Fetching bank accounts.");

                var query = _bankAccountRepository.GetQueryable();

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                        x.AccountName.Contains(filter.Search) ||
                        x.BankName.Contains(filter.Search) ||
                        x.BranchName.Contains(filter.Search) ||
                        x.AccountHolderName.Contains(filter.Search) ||
                        x.AccountNumber.Contains(filter.Search));
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == filter.IsActive.Value);
                }

                query = filter.SortDescending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);

                var response = query.Select(x => new BankAccountResponse
                {
                    Id = x.Id,
                    AccountName = x.AccountName,
                    BankName = x.BankName,
                    BranchName = x.BranchName,
                    AccountHolderName = x.AccountHolderName,
                    AccountNumber = x.AccountNumber,
                    IFSCCode = x.IFSCCode,
                    UpiId = x.UpiId,
                    OpeningBalance = x.OpeningBalance,
                    CurrentBalance = x.CurrentBalance,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive
                });

                var pagedResult = await response.ToPagedResponseAsync(
                    filter.PageNumber,
                    filter.PageSize);

                return Result<PagedResponse<BankAccountResponse>>.Success(
                    pagedResult,
                    "Bank accounts fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bank accounts.");

                return Result<PagedResponse<BankAccountResponse>>.Failure(
                    "An error occurred while fetching bank accounts.",
                    500);
            }
        }

        public async Task<Result<BankAccountResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching bank account. Id: {Id}",
                    id);

                var bankAccount = await _bankAccountRepository.GetByIdAsync(id);

                if (bankAccount == null)
                {
                    _logger.LogWarning(
                        "Bank account not found. Id: {Id}",
                        id);

                    return Result<BankAccountResponse>.NotFound(
                        "Bank account not found.");
                }

                var response = new BankAccountResponse
                {
                    Id = bankAccount.Id,
                    AccountName = bankAccount.AccountName,
                    BankName = bankAccount.BankName,
                    BranchName = bankAccount.BranchName,
                    AccountHolderName = bankAccount.AccountHolderName,
                    AccountNumber = bankAccount.AccountNumber,
                    IFSCCode = bankAccount.IFSCCode,
                    UpiId = bankAccount.UpiId,
                    OpeningBalance = bankAccount.OpeningBalance,
                    CurrentBalance = bankAccount.CurrentBalance,
                    IsPrimary = bankAccount.IsPrimary,
                    IsActive = bankAccount.IsActive
                };

                return Result<BankAccountResponse>.Success(
                    response,
                    "Bank account fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching bank account. Id: {Id}",
                    id);

                return Result<BankAccountResponse>.Failure(
                    "An error occurred while fetching the bank account.",
                    500);
            }
        }

        public async Task<Result<string>> UpdateAsync(UpdateBankAccountRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Updating bank account. Id: {Id}",
                    request.Id);

                var bankAccount = await _bankAccountRepository.GetByIdAsync(request.Id);

                if (bankAccount == null)
                {
                    return Result<string>.NotFound("Bank account not found.");
                }

                var duplicateExists = await _bankAccountRepository
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.Id != request.Id &&
                        x.AccountNumber == request.AccountNumber &&
                        x.IFSCCode == request.IFSCCode);

                if (duplicateExists)
                {
                    return Result<string>.Failure(
                        "Another bank account with the same account number already exists.");
                }

                if (request.IsPrimary && !bankAccount.IsPrimary)
                {
                    await _bankAccountRepository
                        .GetQueryable()
                        .Where(x => x.IsPrimary)
                        .ExecuteUpdateAsync(x =>
                            x.SetProperty(p => p.IsPrimary, false));
                }

                bankAccount.AccountName = request.AccountName;
                bankAccount.BankName = request.BankName;
                bankAccount.BranchName = request.BranchName;
                bankAccount.AccountHolderName = request.AccountHolderName;
                bankAccount.AccountNumber = request.AccountNumber;
                bankAccount.IFSCCode = request.IFSCCode;
                bankAccount.UpiId = request.UpiId;
                bankAccount.IsPrimary = request.IsPrimary;
                bankAccount.IsActive = request.IsActive;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                _bankAccountRepository.Update(bankAccount);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Bank account updated successfully. Id: {Id}",
                    request.Id);

                return Result<string>.Success(
                    request.Id.ToString(),
                    "Bank account updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating bank account. Id: {Id}",
                    request.Id);

                return Result<string>.Failure(
                    "An error occurred while updating bank account.",
                    500);
            }
        }

        public async Task<Result<string>> ChangeStatusAsync(Guid id, bool isActive)
        {
            try
            {
                _logger.LogInformation(
                    "Changing bank account status. Id: {Id}",
                    id);

                var bankAccount = await _bankAccountRepository.GetByIdAsync(id);

                if (bankAccount == null)
                {
                    return Result<string>.NotFound(
                        "Bank account not found.");
                }

                bankAccount.IsActive = isActive;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                _bankAccountRepository.Update(bankAccount);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Bank account status updated successfully. Id: {Id}",
                    id);

                return Result<string>.Success(
                    id.ToString(),
                    $"Bank account {(isActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error changing bank account status. Id: {Id}",
                    id);

                return Result<string>.Failure(
                    "An error occurred while updating bank account status.",
                    500);
            }
        }

        public async Task<Result<BankTransactionResponse>> GetBankTransactionsAsync(BankTransactionFilterRequest? filter,CancellationToken cancellationToken = default)
        {
            filter ??= new BankTransactionFilterRequest();

            // 1. Pagination bounds clamping
            const int defaultPageSize = 10;
            const int maxPageSize = 100;
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? defaultPageSize : Math.Min(filter.PageSize, maxPageSize);

            // 2. Base Query with AsNoTracking
            var query = _transactionRepository
                .GetQueryable()
                .AsNoTracking();

            // 3. Bank Account Filter
            if (filter.BankAccountId.HasValue && filter.BankAccountId.Value != Guid.Empty)
            {
                query = query.Where(x => x.BankAccountId == filter.BankAccountId.Value);
            }

            // 4. Linked Entity Filters
            if (filter.UserId.HasValue && filter.UserId.Value != Guid.Empty)
            {
                query = query.Where(x => x.UserId == filter.UserId.Value);
            }

            if (filter.VendorId.HasValue && filter.VendorId.Value != Guid.Empty)
            {
                query = query.Where(x => x.VendorId == filter.VendorId.Value);
            }

            if (filter.PolicyPaymentId.HasValue && filter.PolicyPaymentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PolicyPaymentId == filter.PolicyPaymentId.Value);
            }

            // 5. Transaction Type Filter (Credit / Debit)
            if (!string.IsNullOrWhiteSpace(filter.TransactionType))
            {
                var type = filter.TransactionType.Trim().ToLowerInvariant();
                query = query.Where(x => x.TransactionType.ToLower() == type);
            }

            // 6. Category Filter
            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                var category = filter.Category.Trim().ToLowerInvariant();
                query = query.Where(x => x.Category.ToLower() == category);
            }

            // 7. Search Text (Description, Category, or Bank Account Name/Number)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Description, $"%{search}%") ||
                    EF.Functions.Like(x.Category, $"%{search}%") ||
                    (x.BankAccount != null && (
                        EF.Functions.Like(x.BankAccount.BankName, $"%{search}%") ||
                        EF.Functions.Like(x.BankAccount.AccountNumber, $"%{search}%")
                    )));
            }

            // 8. Date Range Filter
            if (filter.FromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.TransactionDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(x => x.TransactionDate <= to);
            }

            // 9. Sorting
            var isDescending = !string.Equals(filter.SortOrder?.Trim(), "asc", StringComparison.OrdinalIgnoreCase);

            query = filter.SortBy?.Trim().ToLowerInvariant() switch
            {
                "amount" => isDescending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
                _ => isDescending ? query.OrderByDescending(x => x.TransactionDate) : query.OrderBy(x => x.TransactionDate)
            };

            // 10. Financial Aggregations across filtered set
            var totalCredited = await query
                .Where(x => x.TransactionType == "Credit")
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            var totalDebited = await query
                .Where(x => x.TransactionType == "Debit")
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            var totalRecords = await query.CountAsync(cancellationToken);

            // 11. Pagination & Secure Projection
            var items = totalRecords > 0
                ? await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new BankTransactionItemDto
                    {
                        Id = x.Id,
                        BankAccountId = x.BankAccountId,
                        BankName = x.BankAccount != null ? x.BankAccount.BankName : "N/A",
                        AccountNumberMasked = x.BankAccount != null && !string.IsNullOrEmpty(x.BankAccount.AccountNumber) && x.BankAccount.AccountNumber.Length >= 4
                            ? $"•••• {x.BankAccount.AccountNumber.Substring(x.BankAccount.AccountNumber.Length - 4)}"
                            : "••••",
                        TransactionType = x.TransactionType,
                        Amount = x.Amount,
                        Category = x.Category,
                        Description = x.Description,
                        PolicyPaymentId = x.PolicyPaymentId,
                        SalaryId = x.SalaryId,
                        VendorId = x.VendorId,
                        UserId = x.UserId,
                        TransactionDate = x.TransactionDate,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(cancellationToken)
                : [];

            var totalPages = totalRecords > 0
                ? (int)Math.Ceiling((double)totalRecords / pageSize)
                : 0;

            var response = new BankTransactionResponse
            {
                Summary = new BankTransactionSummaryDto
                {
                    TotalCredited = totalCredited,
                    TotalDebited = totalDebited,
                    TotalTransactions = totalRecords
                },
                Transactions = new PagedResponse<BankTransactionItemDto>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                }
            };

            return Result<BankTransactionResponse>.Success(response, "Bank transactions fetched successfully.");
        }
    }
}
