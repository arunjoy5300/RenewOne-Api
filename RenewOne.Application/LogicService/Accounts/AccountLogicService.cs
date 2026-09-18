using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.Class;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.PolicyDetails;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.Accounts
{
    public class AccountLogicService : IAccountLogicService
    {
        #region DI
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IAgentClassRepository _agentClassRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILedgerRepository _ledgerRepository;
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IPolicyDetailsRepository _policyRepository;
        private readonly ICurrentUserService _currentUserService;
        #endregion

        #region Constructor
        public AccountLogicService(
            IUserRepository userRepository, 
            IAccountRepository accountRepository, 
            IAgentClassRepository agentClassRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            ILedgerRepository ledgerRepository,
            IEnquiryRepository enquiryRepository,
            IPolicyDetailsRepository policyRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _agentClassRepository = agentClassRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _ledgerRepository = ledgerRepository;
            _enquiryRepository = enquiryRepository;
            _policyRepository = policyRepository;
            _currentUserService = currentUserService;
        }
        #endregion

        #region ApproveAgentAsync
        public async Task<Result<bool>> ApproveAgentAsync(Guid accountId)
        {
            try
            {
                await _accountRepository.ApproveAgentAsync(accountId);

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true, "Agent approved successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return Result<bool>.NotFound(ex.Message);
            }
            catch (Exception ex)
            { 
                return Result<bool>.Failure("Error approving agent",500);
            }
        }
        #endregion

        #region GetAgentsAsync
        public async Task<Result<AgentDashboardResponse>> GetAgentsAsync(AgentFilterRequest? filter,CancellationToken cancellationToken = default)
        {
            // 1. Sanitize & clamp pagination inputs to prevent DoS / negative offset
            filter ??= new AgentFilterRequest();

            const int defaultPageSize = 10;
            const int maxPageSize = 100;

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? defaultPageSize : Math.Min(filter.PageSize, maxPageSize);

            // 2. Base query with AsNoTracking for read-only query performance
            const string agentUserType = "Agent";
            var baseAgentQuery = _accountRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.Users.Any(u => u.UserType == agentUserType));

            var filteredQuery = baseAgentQuery;

            // 3. Search filter using EF.Functions for database-side case-insensitive search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                filteredQuery = filteredQuery.Where(x =>
                    EF.Functions.Like(x.AgentName, $"%{search}%") ||
                    EF.Functions.Like(x.Email, $"%{search}%") ||
                    x.ContactNumber.Contains(search) ||
                    x.Code.Contains(search));
            }

            // 4. Status Filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                filteredQuery = filter.Status.Trim().ToLowerInvariant() switch
                {
                    "pendingapproval" or "pending" => filteredQuery.Where(x => !x.IsActive && x.Users.Any(u => !u.IsActive)),
                    "active" => filteredQuery.Where(x => x.IsActive),
                    "inactive" => filteredQuery.Where(x => !x.IsActive),
                    _ => filteredQuery
                };
            }

            // 5. Sorting
            var isDescending = string.Equals(filter.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);

            filteredQuery = filter.SortBy?.Trim().ToLowerInvariant() switch
            {
                "name" => isDescending
                    ? filteredQuery.OrderByDescending(x => x.AgentName)
                    : filteredQuery.OrderBy(x => x.AgentName),

                "joined" or "date" => isDescending
                    ? filteredQuery.OrderByDescending(x => x.CreatedAt)
                    : filteredQuery.OrderBy(x => x.CreatedAt),

                _ => filteredQuery.OrderByDescending(x => x.CreatedAt)
            };

            // 6. Summary metrics calculated from base query
            var summary = new AgentDashboardSummary
            {
                ActiveAgents = await baseAgentQuery
                    .CountAsync(x => x.IsActive, cancellationToken),

                PendingApprovals = await baseAgentQuery
                    .CountAsync(x => !x.IsActive && x.Users.Any(u => !u.IsActive), cancellationToken)
            };

            // 7. Pagination and projected fetch
            var totalRecords = await filteredQuery.CountAsync(cancellationToken);

            var items = totalRecords > 0
                ? await filteredQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new AgentListItem
                    {
                        AccountId = x.Id,
                        AgentId = x.Code,
                        AgentName = x.AgentName,
                        AgencyName = x.AgentName,
                        Email = x.Email,
                        PhoneNumber = x.ContactNumber,
                        Address = x.Address,
                        Pincode = x.Pincode,
                        JoinedOn = x.CreatedAt,
                        Status = !x.IsActive && x.Users.Any(u => !u.IsActive)
                            ? "Pending"
                            : x.IsActive ? "Active" : "Inactive"
                    })
                    .ToListAsync(cancellationToken)
                : [];

            var totalPages = totalRecords > 0
                ? (int)Math.Ceiling((double)totalRecords / pageSize)
                : 0;

            var response = new AgentDashboardResponse
            {
                Summary = summary,
                Agents = new PagedResponse<AgentListItem>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                }
            };

            return Result<AgentDashboardResponse>.Success(
                response,
                "Agents fetched successfully.");
        }
        #endregion

        #region RegisterAgentAsync
        public async Task<Result<bool>> RegisterAgentAsync(AgentSignupRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingUser = await _userRepository.GetByUserNameAsync(request.UserName);
                if (existingUser != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Conflict("Username already exists");
                }

                var basicClass = _agentClassRepository.GetQueryable()
                    .Where(a => a.ClassName == "Basic")
                    .FirstOrDefault();

                if (basicClass == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Basic agent class configuration missing", 500);
                }

                var isAdminCreated = _currentUserService.Role is "SuperAdmin"
                        or "Supervisor"
                        or "AdminEmployee";

                var newAccount = new Account
                {
                    Id = Guid.NewGuid(),
                    AgentName = request.AgentName,
                    Code = await GenerateUniqueAgentCodeAsync(),
                    AgentClassId = basicClass.Id,
                    Email = request.Email,
                    ContactNumber = request.ContactNumber,
                    Pincode = request.Pincode,
                    Address = request.Address,
                    IsActive = isAdminCreated
                };


                var newAgent = new User
                {
                    Id = Guid.NewGuid(),
                    AccountId = newAccount.Id,
                    UserName = request.UserName,
                    FullName = request.AgentName,
                    Email = request.Email,
                    PhoneNumber = request.ContactNumber,
                    UserType = "Agent",
                    IsActive = isAdminCreated,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };

                var wallet = new Domain.Entities.Wallet
                {
                    Id = Guid.NewGuid(),
                    AccountId = newAccount.Id,
                    Balance = 0,
                    IsActive = true
                };

                var ledger = new Domain.Entities.Ledger
                {
                    Id= Guid.NewGuid(),
                    AccountId = newAccount.Id,
                    CreditLimit = 0,
                    AvailableBalance = 0,
                    IsActive = true
                };

                await _accountRepository.AddAccountAsync(newAccount);
                await _userRepository.AddUserAsync(newAgent);
                await _walletRepository.AddAsync(wallet);
                await _ledgerRepository.AddAsync(ledger);

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, "Agent registered successfully", 201);
            }catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<bool>.Failure("An error occurred while registering the agent: " + ex.Message, 500);

            }
        }
        #endregion

        #region GenerateUniqueAgentCodeAsync
        private async Task<string> GenerateUniqueAgentCodeAsync()
        {
            var lastAccount = _accountRepository.GetQueryable()
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastAccount != null && !string.IsNullOrEmpty(lastAccount.Code))
            {
                var parts = lastAccount.Code.Split('-');
                if (parts.Length > 0 && int.TryParse(parts.Last(), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string year = DateTime.UtcNow.Year.ToString();
            return $"AGENT-{year}-{nextNumber:D4}";
        }
        #endregion

        #region Agent Lookup
        public async Task<Result<PagedResponse<AgentLookupResponse>>> GetAgentLookupAsync(AgentLookupRequest request)
        {
            var query = _accountRepository
                .GetQueryable()
                .Where(x =>
                    x.IsActive &&
                    x.Users.Any(u => u.UserType == "Agent"));

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.AgentName.ToLower().Contains(search) ||
                    x.Code.ToLower().Contains(search) ||
                    x.ContactNumber.Contains(search));
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.AgentName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AgentLookupResponse
                {
                    AccountId = x.Id,
                    AgentCode = x.Code,
                    AgentName = x.AgentName,
                    AgencyName = x.AgentName,
                    MobileNumber = x.ContactNumber,
                    AgentClass = x.AgentClass.ClassName,
                    CommissionPercentage = x.AgentClass.CommSharePercentage,
                    WalletBalance = x.Wallet != null
                        ? x.Wallet.Balance
                        : 0,
                    LedgerBalance = x.Ledger != null
                        ? x.Ledger.AvailableBalance
                        : 0,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Result<PagedResponse<AgentLookupResponse>>.Success(
                new PagedResponse<AgentLookupResponse>
                {
                    Items = items,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
                },
                "Agents fetched successfully");
        }
        #endregion

        #region User Profile
        public async Task<Result<UserProfileResponse>> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetAllUsers()
                .Include(u => u.Account)
                .ThenInclude(a => a.AgentClass)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Result<UserProfileResponse>.NotFound("User not found");
            }
            var response = new UserProfileResponse
            {
                UserId = user.Id,
                AccountId = user.AccountId,
                ProfileUrl = null,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.Account?.Address,
                Pincode = user.Account?.Pincode,
                AgencyName = user.Account?.AgentName
            };
            return Result<UserProfileResponse>.Success(response, "User profile fetched successfully");
        }
        #endregion

        #region GetAgentDetailsByAccountIdAsync
        public async Task<Result<AgentAdminDetailsResponse>> GetAgentDetailsByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            // Direct projection with AsNoTracking to prevent memory overhead & change tracking
            var agentDetails = await _accountRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.Id == accountId && x.Users.Any(u => u.UserType == "Agent"))
                .Select(x => new AgentAdminDetailsResponse
                {
                    AccountId = x.Id,
                    AgentCode = x.Code,
                    AgentName = x.AgentName,
                    AgencyName = x.AgentName,
                    PhoneNumber = x.ContactNumber,
                    Email = x.Email,
                    JoinedDate = x.CreatedAt,
                    Address = x.Address,
                    IsActive = x.IsActive,
                    AgentClassName = x.AgentClass != null ? x.AgentClass.ClassName : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (agentDetails is null)
            {
                return Result<AgentAdminDetailsResponse>.Failure("Agent account not found.");
            }

            return Result<AgentAdminDetailsResponse>.Success(
                agentDetails,
                "Agent details fetched successfully.");
        }
        #endregion

        #region GetAgentDashboardOverviewAsync
        public async Task<Result<AgentDashboardOverviewResponse>> GetAgentDashboardOverviewAsync(Guid accountId,CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);

            // 1. Validate Agent Existence
            var agentExists = await _accountRepository
                .GetQueryable()
                .AsNoTracking()
                .AnyAsync(x => x.Id == accountId, cancellationToken);

            if (!agentExists)
            {
                return Result<AgentDashboardOverviewResponse>.Failure("Agent account not found.");
            }

            // 2. Business Summary (Current Month & MoM Growth)
            var currentMonthEnquiriesCount = await _enquiryRepository
                .GetQueryable()
                .AsNoTracking()
                .CountAsync(x => x.TenantId == accountId && x.CreatedAt >= startOfCurrentMonth, cancellationToken);

            var lastMonthEnquiriesCount = await _enquiryRepository
                .GetQueryable()
                .AsNoTracking()
                .CountAsync(x => x.TenantId == accountId && x.CreatedAt >= startOfLastMonth && x.CreatedAt < startOfCurrentMonth, cancellationToken);

            double growthPercent = lastMonthEnquiriesCount == 0
                ? (currentMonthEnquiriesCount > 0 ? 100.0 : 0.0)
                : Math.Round(((double)(currentMonthEnquiriesCount - lastMonthEnquiriesCount) / lastMonthEnquiriesCount) * 100, 1);

            var policiesCompleted = await _policyRepository
                .GetQueryable()
                .AsNoTracking()
                .CountAsync(x => x.Enquiry.TenantId == accountId && x.Enquiry.Status == "PolicyIssued" && x.CreatedAt >= startOfCurrentMonth, cancellationToken);

            var quoteInProgress = await _enquiryRepository
                .GetQueryable()
                .AsNoTracking()
                .CountAsync(x => x.TenantId == accountId && (x.Status == "QuoteInProgress") && x.CreatedAt >= startOfCurrentMonth, cancellationToken);

            var policyInProgress = await _enquiryRepository
                .GetQueryable()
                .AsNoTracking()
                .CountAsync(x => x.TenantId == accountId && (x.Status == "PolicyInProgress") && x.CreatedAt >= startOfCurrentMonth, cancellationToken);

            // 3. Revenue Summary (GWP & Commissions)
            var monthlySales = await _policyRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.Enquiry.TenantId == accountId && x.CreatedAt >= startOfCurrentMonth)
                .SumAsync(x => (decimal?)x.Enquiry.PolicyPayments.Sum(p => p.Amount), cancellationToken) ?? 0m;

            var lifetimeSales = await _policyRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.Enquiry.TenantId == accountId)
                .SumAsync(x => (decimal?)x.Enquiry.PolicyPayments.Sum(p => p.Amount), cancellationToken) ?? 0m;

            //var monthlyProfit = await _enquiryRepository
            //    .GetQueryable()
            //    .AsNoTracking()
            //    .Where(x => x.TenantId == accountId && x.CreatedAt >= startOfCurrentMonth)
            //    .SumAsync(x => (decimal?)x.EnquiryQuotes, cancellationToken) ?? 0m;

            //var lifetimeProfit = await _commissionRepository
            //    .GetQueryable()
            //    .AsNoTracking()
            //    .Where(x => x.AccountId == accountId)
            //    .SumAsync(x => (decimal?)x.NetAmount, cancellationToken) ?? 0m;

            //var companyPayable = await _ledgerRepository
            //    .GetQueryable()
            //    .AsNoTracking()
            //    .Where(x => x.AccountId == accountId && x.Type == "CompanyPayable" && !x.IsSettled)
            //    .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            //var commissionEarned = await _commissionRepository
            //    .GetQueryable()
            //    .AsNoTracking()
            //    .Where(x => x.AccountId == accountId && x.Status == "Earned" && x.CreatedAt >= startOfCurrentMonth)
            //    .SumAsync(x => (decimal?)x.CommissionAmount, cancellationToken) ?? 0m;

            // 4. Financial & Wallet Overview
            //var wallet = await _walletRepository
            //    .()
            //    .AsNoTracking()
            //    .Where(x => x.tena == accountId)
            //    .Select(w => new
            //    {
            //        w.Balance,
            //        w.CreditLimit,
            //        w.OutstandingAmount,
            //        w.LastRechargeAmount,
            //        w.LastRechargeDate
            //    })
            //    .FirstOrDefaultAsync(cancellationToken);

            // 5. Recent Enquiries (Latest 5)
            var recentEnquiries = await _enquiryRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.TenantId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new RecentEnquiryDto
                {
                    Id = x.Id,
                    CustomerName = x.CustomerName,
                    InsuranceType = x.InsuranceType.Name,
                    Status = x.Status,
                    //Premium = x.EnquiryQuotes.Any() ? x.EnquiryQuotes.Sum(eq => eq.) : 0m
                })
                .ToListAsync(cancellationToken);

            //6.Recent Policies(Latest 5)
            var recentPolicies = await _policyRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.Enquiry.TenantId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new RecentPolicyDto
                {
                    PolicyId = x.Id,
                    PolicyNo = x.PolicyNumber,
                    CustomerName = x.Enquiry.CustomerName,
                    //InsurerName = x.Enquiry.PolicyDetails.InsurerName,
                    Premium = x.Enquiry.EnquiryQuotes.Where(p => p.IsSelected == true).Sum(p => p.Premium)
                })
                .ToListAsync(cancellationToken);

            var response = new AgentDashboardOverviewResponse
            {
                BusinessSummary = new BusinessSummaryDto
                {
                    TotalEnquiries = currentMonthEnquiriesCount,
                    GrowthPercentage = growthPercent,
                    PoliciesCompleted = policiesCompleted,
                    QuoteInProgress = quoteInProgress,
                    PolicyInProgress = policyInProgress
                },
                RevenueSummary = new RevenueSummaryDto
                {
                    MonthlySales = monthlySales,
                    LifetimeSales = lifetimeSales,
                    //MonthlyProfit = monthlyProfit,
                    //LifetimeProfit = lifetimeProfit,
                    //CompanyPayable = companyPayable,
                    //CommissionEarned = commissionEarned
                },
                //FinancialOverview = new FinancialOverviewDto
                //{
                //    WalletBalance = wallet?.Balance ?? 0m,
                //    CreditLimit = wallet?.CreditLimit ?? 0m,
                //    LastRechargeAmount = wallet?.LastRechargeAmount ?? 0m,
                //    LastRechargeDate = wallet?.LastRechargeDate,
                //    OutstandingAmount = wallet?.OutstandingAmount ?? 0m
                //},
                RecentEnquiries = recentEnquiries,
                RecentPolicies = recentPolicies
            };

            return Result<AgentDashboardOverviewResponse>.Success(response, "Agent dashboard overview retrieved successfully.");
        }
        #endregion

        #region AgentStaff Dropdown
        public async Task<Result<List<AgentDropdownDto>>> GetAgentDropdownAsync(CancellationToken cancellationToken = default)
        {
            var accountId = _currentUserService.AccountId;

            var agents = await _userRepository
                .GetAllUsers()
                .AsNoTracking()
                .Where(x =>
                    x.AccountId == accountId &&
                    x.UserType == "Staff" &&
                    x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new AgentDropdownDto(
                    x.Id,
                    x.FullName))
                .ToListAsync(cancellationToken);

            return Result<List<AgentDropdownDto>>.Success(
                agents,
                "Agents fetched successfully.");
        }
        #endregion
    }
}
