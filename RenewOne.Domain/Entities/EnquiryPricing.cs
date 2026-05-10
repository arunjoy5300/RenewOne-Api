using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class EnquiryPricing : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CompanyPayable { get; set; }
        public int AgentPoint { get; set; }
        public decimal AdminProfit { get; set; }
        public Guid UpdatedBy { get; set; }
        public virtual Enquiry? Enquiry { get; set; }
    }
}
