namespace RenewOne.Application.DTOs.Response.User
{
    public class WorkAllocationResponse
    {
        public Guid InsuranceTypeId { get; set; }
        public List<Guid> VehicleTypeIds { get; set; } = [];
    }
}
