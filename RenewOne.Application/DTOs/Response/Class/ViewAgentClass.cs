namespace RenewOne.Application.DTOs.Response.Class
{
    public class ViewAgentClass
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public decimal CommSharePercentage { get; set; }
        public bool IsActive { get; set; } = true;
        public int AgentCount { get; set; }
        public List<AgentClassAgent> Agents { get; set; } = [];
    }

    public class AgentClassAgent
    {
        public Guid Id { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string? AgentCode { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
