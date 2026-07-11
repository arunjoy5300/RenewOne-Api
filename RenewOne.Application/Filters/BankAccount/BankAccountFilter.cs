using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.BankAccount
{
    public class BankAccountFilter : PagedRequest
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
