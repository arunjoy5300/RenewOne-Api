using System.ComponentModel.DataAnnotations;

namespace RenewOne.Application.DTOs.Request.PricingMaster
{
    public class PricingMasterRequest
    {
        [Required]
        public Guid InsuranceCompanyId { get; set; }
        [Required]
        public Guid AgentClassId { get; set; }
        [Required]
        public Guid VehicleTypeId { get; set; }
        [MaxLength(50)]
        public string? FuelType { get; set; }
        [Range(0, int.MaxValue)]
        public int MinCC { get; set; }
        [Range(1, int.MaxValue)]
        public int MaxCC { get; set; }
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal TotalPremium { get; set; }
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal CompanyPayable { get; set; }
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal AdminProfit { get; set; }
        [Range(0, int.MaxValue)]
        public int AgentPoints { get; set; }
    }
}
