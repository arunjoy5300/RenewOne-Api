namespace RenewOne.Application.DTOs.Request.QRMapping
{
    public class UpdateQRMappingRequest
    {
        public Guid Id { get; set; }
        public Guid QRMasterId { get; set; }
        public Guid InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public bool IsActive { get; set; }
    }
}
