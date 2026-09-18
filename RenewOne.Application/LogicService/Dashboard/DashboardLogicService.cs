using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Dashboard;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Dashboard;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Dashboard
{
    public class DashboardLogicService : IDashboardLogicService
    {
        #region DI
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IEnquiryQuoteRepository _enquiryQuoteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInsuranceProductRepository _insuranceProductRepository;
        #endregion

        #region Constructor
        public DashboardLogicService(
            IEnquiryRepository enquiryRepository, 
            IEnquiryQuoteRepository enquiryQuoteRepository,
            IUserRepository userRepository,
            IInsuranceProductRepository insuranceProductRepository)
        {
            _enquiryRepository = enquiryRepository;
            _enquiryQuoteRepository = enquiryQuoteRepository;
            _userRepository = userRepository;
            _insuranceProductRepository = insuranceProductRepository;
        }
        #endregion

        #region GetDashboardMetricsAsync
        public async Task<Result<DashboardMetricsDto>> GetDashboardMetricsAsync(DashboardFilter dashboardFilter,CancellationToken ct)
        {
            var enquiryQuery = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking();

            var quoteQuery = _enquiryQuoteRepository
                .GetEnquiryQuoteQuery()
                .AsNoTracking();

            var userQuery = _userRepository
                .GetAllUsers()
                .AsNoTracking();

            var today = DateTime.Today;

            switch (dashboardFilter.DateFilter)
            {
                case DateFilterType.Today:
                    {
                        var from = today;
                        var to = today.AddDays(1);

                        enquiryQuery = enquiryQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        quoteQuery = quoteQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        userQuery = userQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        break;
                    }

                case DateFilterType.Yesterday:
                    {
                        var from = today.AddDays(-1);
                        var to = today;

                        enquiryQuery = enquiryQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        quoteQuery = quoteQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        userQuery = userQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        break;
                    }

                case DateFilterType.ThisWeek:
                    {
                        var from = today.AddDays(
                            -(int)today.DayOfWeek +
                            (int)DayOfWeek.Monday);

                        if (from > today)
                        {
                            from = from.AddDays(-7);
                        }

                        var to = from.AddDays(7);

                        enquiryQuery = enquiryQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        quoteQuery = quoteQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        userQuery = userQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        break;
                    }

                case DateFilterType.ThisMonth:
                    {
                        var from = new DateTime(
                            today.Year,
                            today.Month,
                            1);

                        var to = from.AddMonths(1);

                        enquiryQuery = enquiryQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        quoteQuery = quoteQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        userQuery = userQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        break;
                    }

                case DateFilterType.ThisYear:
                    {
                        var from = new DateTime(today.Year, 1, 1);
                        var to = from.AddYears(1);

                        enquiryQuery = enquiryQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        quoteQuery = quoteQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        userQuery = userQuery.Where(x =>
                            x.CreatedAt >= from &&
                            x.CreatedAt < to);

                        break;
                    }

                case DateFilterType.Custom:
                    {
                        if (dashboardFilter.FromDate.HasValue)
                        {
                            var from = dashboardFilter.FromDate.Value
                                .ToDateTime(TimeOnly.MinValue);

                            enquiryQuery = enquiryQuery.Where(x =>
                                x.CreatedAt >= from);

                            quoteQuery = quoteQuery.Where(x =>
                                x.CreatedAt >= from);

                            userQuery = userQuery.Where(x =>
                                x.CreatedAt >= from);
                        }

                        if (dashboardFilter.ToDate.HasValue)
                        {
                            var to = dashboardFilter.ToDate.Value
                                .ToDateTime(TimeOnly.MinValue)
                                .AddDays(1);

                            enquiryQuery = enquiryQuery.Where(x =>
                                x.CreatedAt < to);

                            quoteQuery = quoteQuery.Where(x =>
                                x.CreatedAt < to);

                            userQuery = userQuery.Where(x =>
                                x.CreatedAt < to);
                        }

                        break;
                    }
            }

            var totalEnquiries = await enquiryQuery
                .CountAsync(ct);

            var totalUnassigned = await enquiryQuery
                .CountAsync(
                    x => x.AssignedEmployeeId == null,
                    ct);

            var convertedPolicies = await enquiryQuery
                .CountAsync(
                    x => x.Status == EnquiryStatus.PolicyIssued.ToString(),
                    ct);

            var totalProfit = await quoteQuery
                .Where(x => x.IsSelected)
                .SumAsync(
                    x => (decimal?)x.AdminProfit,
                    ct) ?? 0;

            var pendingAgents = await userQuery
                .CountAsync(
                    x => x.UserType == "Agent" &&
                         !x.IsActive,
                    ct);

            var response = new DashboardMetricsDto(
                totalEnquiries,
                totalUnassigned,
                convertedPolicies,
                totalProfit,
                pendingAgents,
                0
            );

            return Result<DashboardMetricsDto>.Success(
                response,
                "Dashboard metrics fetched successfully.");
        }
        #endregion

        #region Product Listing
        public async Task<Result<List<InsuranceProductDto>>> GetActiveDashboardProductsAsync(CancellationToken ct = default)
        {
            try
            {
                var products = await _insuranceProductRepository
                    .GetInsuranceProducts()
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new InsuranceProductDto(
                        x.Id,
                        x.InsuranceTypeId,
                        x.InsuranceType.Name,
                        x.VehicleTypeId,
                        x.Name,
                        x.Subtitle,
                        x.Code,
                        x.RoutePath,
                        x.BadgeText,
                        x.BadgeColor,
                        x.IconKey,
                        x.DisplayOrder
                    ))
                    .ToListAsync(ct);

                return Result<List<InsuranceProductDto>>.Success(
                    products,
                    "Dashboard products loaded successfully.");
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Failed to load dashboard products.");

                return Result<List<InsuranceProductDto>>.Failure(
                    "Failed to load dashboard products.");
            }
        }
        #endregion
    }
}
