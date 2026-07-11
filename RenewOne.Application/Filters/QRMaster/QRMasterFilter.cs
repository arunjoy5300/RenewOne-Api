using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.QRMaster
{
    public class QRMasterFilter : PagedRequest
    {
        public string? Search { get; set; }
        public Guid? BankAccountId { get; set; }
        public bool? IsActive { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
