using RenewOne.Application.Common.Pagination;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public class WalletTransactionFilter : PagedRequest
    {
        public WalletTransactionType? Type { get; set; }

        public DateFilterType? DateFilter { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public string? Search { get; set; }
    }
}
