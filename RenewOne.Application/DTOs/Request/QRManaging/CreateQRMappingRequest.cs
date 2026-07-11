namespace RenewOne.Application.DTOs.Request.QRMapping
{
    public class CreateQRMappingRequest
    {
        public Guid QRMasterId { get; set; }
        public Guid InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
    }
}
