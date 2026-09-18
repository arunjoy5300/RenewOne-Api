namespace RenewOne.Application.DTOs.Response.Dashboard
{
    public record DashboardMetricsDto(
        int TotalEnquiriesCount,
        int TotalUnassignedCount,
        int ConvertedPolicyCount,
        decimal TotalProfit,
        int PendingAgentsCount,
        int UpcomingRenewalCount
    );
}
