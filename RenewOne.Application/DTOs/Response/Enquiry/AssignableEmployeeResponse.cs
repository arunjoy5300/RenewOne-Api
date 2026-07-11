namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class AssignableEmployeeResponse
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsCurrentlyAssigned { get; set; }
    }
}
