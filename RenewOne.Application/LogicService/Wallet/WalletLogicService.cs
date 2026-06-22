using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Service;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Application.DTOs.Response.Service;
using RenewOne.Application.DTOs.Response.Wallet;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Wallet;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Application.Interfaces.Services.External.Cashfree;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;
using System.Text.Json;

namespace RenewOne.Application.LogicService.Wallet
{
    public class WalletLogicService : IWalletlogicService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WalletLogicService> _logger;
        private readonly ICashfreeService _cashfreeService;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentOrderRepository _paymentOrderRepository;
        public WalletLogicService(
            IWalletRepository walletRepository,
            ICurrentUserService currentUser,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork,
            ILogger<WalletLogicService> logger,
            ICashfreeService cashfreeService,
            IUserRepository userRepository,
            IPaymentOrderRepository paymentOrderRepository)
        {
            _walletRepository = walletRepository;
            _currentUser = currentUser;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _cashfreeService = cashfreeService;
            _logger = logger;
            _userRepository = userRepository;
            _paymentOrderRepository = paymentOrderRepository;
        }

        public async Task<Result<bool>> AdjustWalletAsync(WalletAdjustmentRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Amount <= 0)
                    return Result<bool>.Failure("Amount should be greater than zero.");

                _logger.LogInformation("Fetching wallet details");

                var wallet = await _walletRepository.GetByAccountIdAsync(request.AccountId);

                if (wallet == null)
                    return Result<bool>.NotFound("Wallet not found.");

                if (request.TransactionType == WalletTransactionType.Debit &&
                    wallet.Balance < request.Amount)
                {
                    _logger.LogError("Insufficient balance");
                    return Result<bool>.Failure("Insufficient wallet balance.");
                }

                _logger.LogInformation("Current Balance:{bal}", wallet.Balance);
                var balanceBefore = wallet.Balance;

                if (request.TransactionType == WalletTransactionType.Credit)
                    wallet.Balance += request.Amount;
                else
                    wallet.Balance -= request.Amount;

                wallet.UpdatedAt = DateTime.UtcNow;

                var transaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,

                    TransactionType = request.TransactionType.ToString(),

                    Amount = request.Amount,

                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,

                    Remarks = request.Remarks,

                    Status = "Success",

                    CreatedByUserId = _currentUser.UserId,

                    CreatedByIpAddress = _currentUser.IpAddress,

                    TransactionDate = DateTime.UtcNow,

                    ReferenceNo = $"WT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Updating Wallet");
                await _walletRepository.Update(wallet);

                _logger.LogInformation("Adding Wallet Transaction");
                await _walletTransactionRepository.AddAsync(transaction);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(
                    true,
                    request.TransactionType == WalletTransactionType.Credit
                        ? "Wallet credited successfully."
                        : "Wallet debited successfully.");
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<bool>.Failure("Error adjusting wallet: " + ex.Message, 500);
            }
        }

        public async Task<Result<PagedResponse<WalletTransactionResponse>>> GetTransactionsAsync(WalletTransactionFilter filter)
        {
            var accountId = _currentUser.AccountId;

            var wallet = await _walletRepository.GetByAccountIdAsync(accountId);

            if (wallet == null)
                return Result<PagedResponse<WalletTransactionResponse>>
                    .NotFound("Wallet not found.");

            var query = _walletTransactionRepository
                .GetQueryable()
                .Where(x => x.WalletId == wallet.Id);

            // Transaction Type Filter
            if (filter.Type.HasValue)
            {
                query = query.Where(x => x.TransactionType == filter.Type.Value.ToString());
            }

            // Search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
            }

            // Date Filter
            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(x => x.CreatedAt >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(x => x.CreatedAt <= to);
            }

            var totalRecords = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WalletTransactionResponse
                {
                    Id = x.Id,
                    Type = Enum.Parse<WalletTransactionType>(x.TransactionType),
                    Amount = x.Amount,
                    BalanceAfter = x.BalanceAfter,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var response = new PagedResponse<WalletTransactionResponse>
            {
                Items = transactions,
                TotalRecords = totalRecords,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return Result<PagedResponse<WalletTransactionResponse>>
                .Success(response);
        }

        public async Task<Result<WalletResponse>> GetWalletAsync()
        {
            try
            {
                var accountId = _currentUser.AccountId;

                var wallet = await _walletRepository.GetByAccountIdAsync(accountId);

                if (wallet == null)
                    return Result<WalletResponse>.NotFound("Wallet not found.");

                var response = new WalletResponse
                {
                    WalletId = wallet.Id,
                    Balance = wallet.Balance,
                    UpdatedAt = wallet.UpdatedAt
                };

                return Result<WalletResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<WalletResponse>.Failure("Error fetching wallet: " + ex.Message,500);
            }
        }

        public async Task<Result<bool>> ProcessWebhookAsync(string body, IHeaderDictionary headers)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Processing Cashfree Webhook");
                if (!headers.TryGetValue("x-webhook-signature", out var signature) ||
                    !headers.TryGetValue("x-webhook-timestamp", out var timestamp))
                {
                    return Result<bool>.Failure("Webhook headers missing.", 400);
                }

                var isValid = _cashfreeService.VerifyWebhookSignature(
                    body,
                    timestamp,
                    signature);

                if (!isValid)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Invalid webhook signature.", 401);
                }

                // Deserialize
                var webhook = JsonSerializer.Deserialize<CashfreeWebhookRequest>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                _logger.LogInformation("Serialized");
                if (webhook == null)
                {
                    _logger.LogInformation("Webhook null");
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Invalid webhook payload.");
                }

                _logger.LogInformation("OrderId: {id}", webhook.Data.Order.OrderId);
                // Find Payment Order
                var paymentOrder = await _paymentOrderRepository
                    .GetByMerchantOrderIdAsync(webhook.Data.Order.OrderId);

                if (paymentOrder == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.NotFound("Payment order not found.");
                }

                if (paymentOrder.Amount != webhook.Data.Payment.PaymentAmount)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Payment amount mismatch.");
                }

                // Already processed
                if (paymentOrder.Status == "SUCCESS")
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return Result<bool>.Success(true, "Webhook already processed.");
                }

                // Update PaymentOrder
                paymentOrder.Status = webhook.Data.Payment.PaymentStatus;
                paymentOrder.CashfreePaymentId = webhook.Data.Payment.CfPaymentId;
                paymentOrder.CompletedAt = webhook.Data.Payment.PaymentTime;

                paymentOrder.Amount =
                    webhook.Data.Payment.PaymentAmount;

                paymentOrder.CompletedAt =
                    webhook.Data.Payment.PaymentTime;
                paymentOrder.UpdatedAt = DateTime.UtcNow;

                _paymentOrderRepository.Update(paymentOrder);

                // If payment failed / dropped
                if (webhook.Data.Payment.PaymentStatus != "SUCCESS")
                {
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<bool>.Success(
                        true,
                        $"Payment status updated : {webhook.Data.Payment.PaymentStatus}");
                }

                switch (paymentOrder.Module)
                {
                    case "WalletRecharge":
                        await HandleWalletRechargeAsync(paymentOrder, webhook);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unknown payment module '{paymentOrder.Module}'.");
                }

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(
                    true,
                    "Webhook processed successfully.");

            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing Cashfree webhook");
                return Result<bool>.Failure("Error processing webhook: " + ex.Message, 500);
            }
        }

        public async Task<Result<WalletRechargeResponse>> RechargeWalletAsync(WalletRechargeRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.Amount <= 0)
                    return Result<WalletRechargeResponse>.Failure("Amount should be greater than zero.");

                var accountId = _currentUser.AccountId;

                var wallet = await _walletRepository.GetByAccountIdAsync(accountId);

                if (wallet == null)
                    return Result<WalletRechargeResponse>.NotFound("Wallet not found.");

                var user = await _userRepository.GetByIdAsync(_currentUser.UserId);

                if (user == null)
                    return Result<WalletRechargeResponse>.NotFound("User not found.");

                var merchantOrderId =
                    $"WR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

                var cashfreeResponse = await _cashfreeService.CreateOrderAsync(
                    new CashfreeCreateOrderRequest
                    {
                        OrderId = merchantOrderId,
                        OrderAmount = request.Amount,

                        CustomerId = user.Id.ToString(),
                        CustomerName = user.FullName,
                        CustomerEmail = user.Email,
                        CustomerPhone = user.PhoneNumber,
                        OrderNote = "Wallet Recharge"
                    });

                var paymentOrder = new PaymentOrder
                {
                    Id = Guid.NewGuid(),

                    AccountId = accountId,

                    Module = "WalletRecharge",

                    Amount = cashfreeResponse.Order_Amount,

                    Currency = cashfreeResponse.Order_Currency,

                    MerchantOrderId = cashfreeResponse.Order_Id,

                    CashfreeOrderId = cashfreeResponse.Cf_Order_Id,

                    CashfreePaymentSessionId = cashfreeResponse.Payment_Session_Id,

                    Status = cashfreeResponse.Order_Status,

                    CustomerId = cashfreeResponse.Customer_Details.Customer_Id,

                    CustomerName = cashfreeResponse.Customer_Details.Customer_Name,

                    CustomerEmail = cashfreeResponse.Customer_Details.Customer_Email,

                    CustomerPhone = cashfreeResponse.Customer_Details.Customer_Phone,

                    Notes = cashfreeResponse.Order_Note,

                    ExpiresAt = cashfreeResponse.Order_Expiry_Time,

                    CreatedByUserId = _currentUser.UserId,

                    CreatedByIpAddress = _currentUser.IpAddress,

                    RawResponse = JsonSerializer.Serialize(cashfreeResponse),

                    CreatedAt = DateTime.UtcNow,

                    UpdatedAt = DateTime.UtcNow
                };

                await _paymentOrderRepository.AddAsync(paymentOrder);

                await _unitOfWork.CommitTransactionAsync();

                return Result<WalletRechargeResponse>.Success(
                    new WalletRechargeResponse
                    {
                        MerchantOrderId = paymentOrder.MerchantOrderId,
                        PaymentSessionId = paymentOrder.CashfreePaymentSessionId,
                        Amount = paymentOrder.Amount
                    },
                    "Order created successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return Result<WalletRechargeResponse>.Failure(
                    $"Unable to create payment order. {ex.Message}",
                    500);
            }
        }

        private async Task HandleWalletRechargeAsync(PaymentOrder paymentOrder,CashfreeWebhookRequest webhook)
        {
            var wallet = await _walletRepository.GetByAccountIdAsync(paymentOrder.AccountId);

            if (wallet == null)
                throw new KeyNotFoundException("Wallet not found.");

            var balanceBefore = wallet.Balance;

            wallet.Balance += webhook.Data.Payment.PaymentAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            _walletRepository.Update(wallet);

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                TransactionType = "Credit",
                Amount = webhook.Data.Payment.PaymentAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                Status = "Success",
                TransactionDate = DateTime.UtcNow,
                Remarks = "Wallet Recharge via Cashfree",
                ReferenceNo = webhook.Data.Payment.CfPaymentId,
                CreatedByUserId = paymentOrder.CreatedByUserId,
                CreatedByIpAddress = paymentOrder.CreatedByIpAddress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _walletTransactionRepository.AddAsync(transaction);
        }
    }
}
