namespace RenewOne.Application.DTOs.Response.AdminEntry
{
    public class VehicleTypeResponse
    {
        public Guid Id { get; set; }

        public string TypeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
