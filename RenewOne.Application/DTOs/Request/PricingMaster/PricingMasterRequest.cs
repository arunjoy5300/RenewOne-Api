namespace RenewOne.Application.DTOs.Request.PricingMaster
{
    public class PricingMasterRequest
    {
        public Guid InsuranceCompanyId { get; set; }
        public Guid AgentClassId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public int MinCC { get; set; }
        public int MaxCC { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CompanyPayable { get; set; }
        public decimal AdminProfit { get; set; }
        public int AgentPoints { get; set; }
    }
}
