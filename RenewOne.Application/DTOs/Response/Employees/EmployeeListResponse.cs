namespace RenewOne.Application.DTOs.Response.Employees
{
    public class EmployeeListResponse
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool Status { get; set; }
        public int ActiveTask { get; set; }
        public int CompletedTask { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
