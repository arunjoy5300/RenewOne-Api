namespace RenewOne.Application.DTOs.Request.User
{
    public class UpdateEmployeeRightsRequest
    {
        public Guid UserId { get; set; }

        public List<WorkAllocationRequest> WorkAllocations { get; set; } = [];

        public List<string> ModulePermissions { get; set; } = [];
    }
}
