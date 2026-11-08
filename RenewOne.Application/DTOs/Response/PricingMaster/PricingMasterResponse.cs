namespace RenewOne.Application.DTOs.Response.PricingMaster
{
    public class PricingMasterResponse
    {
        public Guid Id { get; set; }
        public Guid InsuranceCompanyId { get; set; }
        public Guid AgentClassId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public string AgentClassName { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public string? FuelType { get; set; }
        public int? MinCC { get; set; }
        public int? MaxCC { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CompanyPayable { get; set; }
        public decimal AgentPoints { get; set; }
        public decimal AdminProfit { get; set; }
        public bool IsActive { get; set; }
    }
}
