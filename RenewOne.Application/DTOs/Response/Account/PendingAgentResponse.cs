namespace RenewOne.Application.DTOs.Response.Account
{
    public class PendingAgentResponse
    {
        public Guid AccountId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string RequestedClass { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
