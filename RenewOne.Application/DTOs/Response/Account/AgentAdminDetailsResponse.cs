namespace RenewOne.Application.DTOs.Response.Account
{
    public class AgentAdminDetailsResponse
    {
        public Guid AccountId { get; set; }
        public string AgentCode { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? AgentClassName { get; set; }
    }
}
