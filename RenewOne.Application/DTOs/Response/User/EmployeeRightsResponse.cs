namespace RenewOne.Application.DTOs.Response.User
{
    public class EmployeeRightsResponse
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string UserType { get; set; } = string.Empty;

        public List<WorkAllocationResponse> WorkAllocations { get; set; } = [];

        public List<string> ModulePermissions { get; set; } = [];
    }
}
