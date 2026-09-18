using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Dashboard;
using RenewOne.Application.Filters.Enquiry;

namespace RenewOne.Application.Interfaces.LogicService.Dashboard
{
    public interface IDashboardLogicService
    {
        Task<Result<DashboardMetricsDto>> GetDashboardMetricsAsync(DashboardFilter dashboardFilter, CancellationToken ct);
        Task<Result<List<InsuranceProductDto>>> GetActiveDashboardProductsAsync(CancellationToken ct = default);
    }
}
