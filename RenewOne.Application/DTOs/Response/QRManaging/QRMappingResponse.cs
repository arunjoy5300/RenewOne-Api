namespace RenewOne.Application.DTOs.Response.QRMapping
{
    public class QRMappingResponse
    {
        public Guid Id { get; set; }
        public Guid QRMasterId { get; set; }
        public string QRName { get; set; } = string.Empty;
        public Guid InsuranceTypeId { get; set; }
        public string InsuranceType { get; set; } = string.Empty;
        public Guid? VehicleTypeId { get; set; }
        public string? VehicleType { get; set; }
        public string BankName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
