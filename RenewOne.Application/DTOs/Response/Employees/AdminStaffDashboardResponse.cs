namespace RenewOne.Application.DTOs.Response.Employees
{
    public class AdminStaffDashboardResponse
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalActiveTask { get; set; }
        public int TotalCompletedTask { get; set; }
    }
}

