namespace RenewOne.Application.DTOs.Request.User
{
    public class WorkAllocationRequest
    {
        public Guid InsuranceTypeId { get; set; }
        public List<Guid> VehicleTypeIds { get; set; } = [];
    }
}
