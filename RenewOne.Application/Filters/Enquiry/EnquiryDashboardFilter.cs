using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public class EnquiryDashboardFilter
    {
        public DateFilterType dateFilter {  get; set; }
        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public Guid? AccountId { get; set; }

        public Guid? InsuranceTypeId { get; set; }
    }
}
