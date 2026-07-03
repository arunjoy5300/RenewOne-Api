using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class EnquiryQuote : BaseEntity
    {
        public Guid EnquiryId { get; set;  }
        public Guid InsuranceCompanyId { get; set; }
        public decimal Premium { get; set; }
        public decimal CompanyPayable { get; set; }
        public decimal AgentPoints { get; set; }
        public decimal AdminProfit { get; set; }
        public decimal? Idv { get; set; }
        public bool IsManualQuote { get; set; }
        public string? Remarks { get; set; }
        public bool IsSelected { get; set; }
        public Guid? SelectedByUserId { get; set; }
        public DateTime? SelectedAt { get; set; }
        public virtual Enquiry? Enquiry { get; set; }
        public virtual InsuranceCompany? InsuranceCompany { get; set; }
        public virtual ICollection<PolicyPayment> PolicyPayments { get; set; }
    = new HashSet<PolicyPayment>();
        public virtual User? User { get; set; } 
    }
}
