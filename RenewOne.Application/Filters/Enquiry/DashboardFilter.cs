using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public class DashboardFilter
    {
        public DateFilterType DateFilter { get; set; } = DateFilterType.Today;

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }
    }
}
