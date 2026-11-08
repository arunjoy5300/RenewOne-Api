using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Common.Models;
using RenewOne.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RenewOne.API.Controllers.Accounts
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountLogicService _accountLogicService;
        private readonly AppDbContext _db;
        public AccountController(IAccountLogicService accountLogicService, AppDbContext db)
        {
            _accountLogicService = accountLogicService;
            _db = db;
        }
        [HttpPost("register-agent")]
        public async Task<IActionResult> RegisterAgent([FromBody] AgentSignupRequest signupRequest)
        {
            var result = await _accountLogicService.RegisterAgentAsync(signupRequest);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpPost("approve-agent/{accountId}")]
        public async Task<IActionResult> ApproveAgent([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.ApproveAgentAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents([FromQuery] AgentFilterRequest filter,CancellationToken ct)
        {
            var result = await _accountLogicService.GetAgentsAsync(filter, ct);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("agent-lookup")]
        public async Task<IActionResult> GetAgentLookup([FromQuery] AgentLookupRequest request)
        {
            var result = await _accountLogicService.GetAgentLookupAsync(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("user-profile/{userId}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] Guid userId)
        {
            var result = await _accountLogicService.GetUserProfileAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("admin/agent-details/{accountId}")]
        public async Task<IActionResult> GetAgentDetails([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.GetAgentDetailsByAccountIdAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpPut("admin/agent-details/{accountId:guid}")]
        public async Task<IActionResult> UpdateAgentDetails([FromRoute] Guid accountId, [FromBody] UpdateAgentAdminDetailsRequest request, CancellationToken ct)
        {
            var result = await _accountLogicService.UpdateAgentAdminDetailsAsync(accountId, request, ct);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("admin/agent-dashboard-overview/{accountId}")] //inner page
        public async Task<IActionResult> GetAgentDashboardOverview([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.GetAgentDashboardOverviewAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "SuperAdmin,Supervisor")]
        [HttpGet("admin/agent-profitability/{accountId:guid}")]
        public async Task<IActionResult> GetAgentProfitability(Guid accountId, CancellationToken ct)
        {
            var accountExists = await _db.Accounts.AsNoTracking().AnyAsync(x => x.Id == accountId, ct);
            if (!accountExists) return NotFound(Result<object>.NotFound("Agent account not found."));

            var now = DateTime.UtcNow;
            var indiaOffset = TimeSpan.FromMinutes(330);
            var indiaNow = new DateTimeOffset(now, TimeSpan.Zero).ToOffset(indiaOffset);
            var indiaDayStart = new DateTimeOffset(indiaNow.Year, indiaNow.Month, indiaNow.Day, 0, 0, 0, indiaOffset);
            var indiaWeekStart = indiaDayStart.AddDays(-(((int)indiaDayStart.DayOfWeek + 6) % 7));
            var indiaYearStart = new DateTimeOffset(indiaNow.Year, 1, 1, 0, 0, 0, indiaOffset);
            var todayStart = indiaDayStart.UtcDateTime;
            var weekStart = indiaWeekStart.UtcDateTime;
            var yearStart = indiaYearStart.UtcDateTime;
            var priorYearStart = yearStart.AddYears(-1);

            var issuedPolicies = await _db.PolicyDetails.AsNoTracking()
                .Where(p => p.Enquiry.TenantId == accountId && p.Enquiry.Status == "PolicyIssued" &&
                    p.CreatedAt >= priorYearStart && p.CreatedAt <= now)
                .Select(p => new IssuedPolicyProfitRow
                {
                    IssuedAt = p.CreatedAt,
                    Premium = p.Enquiry.EnquiryQuotes.Where(q => q.IsSelected).Select(q => q.Premium).FirstOrDefault(),
                    AdminProfit = p.Enquiry.EnquiryQuotes.Where(q => q.IsSelected).Select(q => q.AdminProfit).FirstOrDefault(),
                    AgentEarnings = p.Enquiry.EnquiryQuotes.Where(q => q.IsSelected).Select(q => q.AgentPoints).FirstOrDefault()
                })
                .ToListAsync(ct);

            var elapsedToday = now - todayStart;
            var elapsedWeek = now - weekStart;
            var elapsedYear = now - yearStart;
            var response = new AgentProfitabilityResponse
            {
                Today = BuildProfitSnapshot(issuedPolicies, todayStart, now, todayStart.AddDays(-1), todayStart.AddDays(-1) + elapsedToday),
                ThisWeek = BuildProfitSnapshot(issuedPolicies, weekStart, now, weekStart.AddDays(-7), weekStart.AddDays(-7) + elapsedWeek),
                ThisYear = BuildProfitSnapshot(issuedPolicies, yearStart, now, priorYearStart, priorYearStart + elapsedYear)
            };

            return Ok(Result<AgentProfitabilityResponse>.Success(response, "Agent profitability retrieved."));
        }

        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("admin/agent-staff-performance/{accountId:guid}")]
        public async Task<IActionResult> GetAgentStaffPerformance(Guid accountId, CancellationToken ct)
        {
            var accountExists = await _db.Accounts.AsNoTracking().AnyAsync(x => x.Id == accountId, ct);
            if (!accountExists) return NotFound(Result<object>.NotFound("Agent account not found."));

            var cutoff = DateTime.UtcNow.AddDays(-30);
            var matureAccountEnquiries = await _db.Enquiries.AsNoTracking()
                .Where(e => e.TenantId == accountId && e.CreatedAt <= cutoff).CountAsync(ct);
            var issuedAccountEnquiries = await _db.Enquiries.AsNoTracking()
                .Where(e => e.TenantId == accountId && e.CreatedAt <= cutoff &&
                    _db.PolicyDetails.Any(p => p.EnquiryId == e.Id && p.CreatedAt <= e.CreatedAt.AddDays(30)))
                .CountAsync(ct);
            var staff = await _db.Users.AsNoTracking()
                .Where(x => x.AccountId == accountId && x.UserType == "Staff")
                .OrderBy(x => x.FullName)
                .Select(x => new
                {
                    staffId = x.Id,
                    x.FullName,
                    x.UserName,
                    x.IsActive,
                    x.CreatedAt,
                    assignedEnquiries = _db.Enquiries.Count(e => e.TenantId == accountId && e.AssignedEmployeeId == x.Id),
                    matureEnquiries = _db.Enquiries.Count(e => e.TenantId == accountId && e.AssignedEmployeeId == x.Id && e.CreatedAt <= cutoff),
                    issuedWithin30Days = _db.Enquiries.Count(e => e.TenantId == accountId && e.AssignedEmployeeId == x.Id && e.CreatedAt <= cutoff && _db.PolicyDetails.Any(p => p.EnquiryId == e.Id && p.CreatedAt <= e.CreatedAt.AddDays(30)))
                })
                .ToListAsync(ct);

            var response = new
            {
                totalStaff = staff.Count,
                activeStaff = staff.Count(x => x.IsActive),
                inactiveStaff = staff.Count(x => !x.IsActive),
                matureEnquiries = matureAccountEnquiries,
                issuedWithin30Days = issuedAccountEnquiries,
                conversionRate = matureAccountEnquiries == 0 ? 0m : Math.Round((decimal)issuedAccountEnquiries / matureAccountEnquiries * 100m, 1),
                cohortCutoffUtc = cutoff,
                staff = staff.Select(x => new
                {
                    x.staffId, x.FullName, x.UserName, x.IsActive, x.CreatedAt,
                    x.assignedEnquiries, x.matureEnquiries, x.issuedWithin30Days,
                    conversionRate = x.matureEnquiries == 0 ? 0m : Math.Round((decimal)x.issuedWithin30Days / x.matureEnquiries * 100m, 1)
                })
            };
            return Ok(Result<object>.Success(response, "Staff performance retrieved."));
        }

        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("admin/agent-finance/{accountId:guid}")]
        public async Task<IActionResult> GetAgentFinance(Guid accountId, CancellationToken ct)
        {
            var accountExists = await _db.Accounts.AsNoTracking().AnyAsync(x => x.Id == accountId, ct);
            if (!accountExists) return NotFound(Result<object>.NotFound("Agent account not found."));
            var wallet = await _db.Wallets.AsNoTracking().Where(x => x.AccountId == accountId)
                .Select(x => new { x.Id, x.Balance, x.UpdatedAt }).FirstOrDefaultAsync(ct);
            var manualWallet = await _db.Ledgers.AsNoTracking().Where(x => x.AccountId == accountId)
                .Select(x => new { x.Id, balance = x.AvailableBalance, x.UpdatedAt }).FirstOrDefaultAsync(ct);
            var walletTransactions = new List<object>();
            if (wallet != null)
            {
                var rows = await _db.WalletTransactions.AsNoTracking()
                    .Where(x => x.WalletId == wallet.Id).OrderByDescending(x => x.CreatedAt).Take(25)
                    .Select(x => new { x.Id, type = x.TransactionType, x.Amount, x.BalanceBefore, x.BalanceAfter, x.Remarks, x.ReferenceNo, x.Status, x.CreatedAt })
                    .ToListAsync(ct);
                walletTransactions = rows.Cast<object>().ToList();
            }
            var manualWalletTransactions = new List<object>();
            if (manualWallet != null)
            {
                var rows = await _db.LedgerTransactions.AsNoTracking()
                    .Where(x => x.LedgerId == manualWallet.Id).OrderByDescending(x => x.CreatedAt).Take(25)
                    .Select(x => new { x.Id, x.TransactionType, x.Amount, x.BalanceBefore, x.BalanceAfter, x.Remarks, x.ReferenceNo, x.CreatedAt })
                    .ToListAsync(ct);
                manualWalletTransactions = rows.Cast<object>().ToList();
            }
            return Ok(Result<object>.Success(new { wallet, manualWallet, walletTransactions, manualWalletTransactions }, "Agent finance retrieved."));
        }

        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("admin/agent-activity/{accountId:guid}")]
        public async Task<IActionResult> GetAgentActivity(Guid accountId, CancellationToken ct)
        {
            var accountExists = await _db.Accounts.AsNoTracking().AnyAsync(x => x.Id == accountId, ct);
            if (!accountExists) return NotFound(Result<object>.NotFound("Agent account not found."));
            var enquiries = await _db.Enquiries.AsNoTracking().Where(x => x.TenantId == accountId)
                .OrderByDescending(x => x.CreatedAt).Take(25)
                .Select(x => new { id = x.Id, type = "Enquiry", title = x.CustomerName, status = x.Status, occurredAt = x.CreatedAt, reference = x.EnquiryNumber })
                .ToListAsync(ct);
            var policies = await _db.PolicyDetails.AsNoTracking().Where(x => x.Enquiry.TenantId == accountId)
                .OrderByDescending(x => x.CreatedAt).Take(25)
                .Select(x => new { id = x.Id, type = "Policy", title = x.Enquiry.CustomerName, status = "PolicyIssued", occurredAt = x.CreatedAt, reference = x.PolicyNumber })
                .ToListAsync(ct);
            return Ok(Result<object>.Success(new { enquiries, policies }, "Agent activity retrieved."));
        }

        private static AgentProfitPeriodSnapshot BuildProfitSnapshot(
            List<IssuedPolicyProfitRow> rows,
            DateTime periodStart,
            DateTime periodEnd,
            DateTime previousStart,
            DateTime previousEnd)
        {
            var current = rows.Where(x => x.IssuedAt >= periodStart && x.IssuedAt <= periodEnd).ToList();
            var previous = rows.Where(x => x.IssuedAt >= previousStart && x.IssuedAt <= previousEnd).ToList();
            var adminProfit = current.Sum(x => x.AdminProfit);
            var agentEarnings = current.Sum(x => x.AgentEarnings);

            return new AgentProfitPeriodSnapshot
            {
                AdminProfit = adminProfit,
                AgentEarnings = agentEarnings,
                PremiumVolume = current.Sum(x => x.Premium),
                PoliciesIssued = current.Count,
                AdminProfitChangePercentage = CalculateChange(adminProfit, previous.Sum(x => x.AdminProfit)),
                AgentEarningsChangePercentage = CalculateChange(agentEarnings, previous.Sum(x => x.AgentEarnings))
            };
        }

        private static decimal? CalculateChange(decimal current, decimal previous)
        {
            if (previous == 0m) return current == 0m ? 0m : null;
            return Math.Round((current - previous) / Math.Abs(previous) * 100m, 1);
        }

        private sealed class IssuedPolicyProfitRow
        {
            public DateTime IssuedAt { get; init; }
            public decimal Premium { get; init; }
            public decimal AdminProfit { get; init; }
            public decimal AgentEarnings { get; init; }
        }
        #region Agent Staff Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetAgentDropdown(CancellationToken cancellationToken = default)
        {
            var result = await _accountLogicService.GetAgentDropdownAsync(cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
    }
}
