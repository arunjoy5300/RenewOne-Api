using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.DTOs.Request.Employees
{
    public class EmployeeListRequest : PagedRequest
    {
        public bool? Status { get; set; }
        public string? Search { get; set; }
    }
}
