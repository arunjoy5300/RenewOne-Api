namespace RenewOne.Application.DTOs.Request.Account
{
    public sealed class AgentClassRequest
    {
        public string ClassName { get; set; } = string.Empty;
        public decimal CommSharePercentage { get; set; }
    }
}
