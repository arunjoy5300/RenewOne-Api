namespace RenewOne.Application.DTOs.Response.Account
{
    public class AgentProfitabilityResponse
    {
        public AgentProfitPeriodSnapshot Today { get; set; } = new();
        public AgentProfitPeriodSnapshot ThisWeek { get; set; } = new();
        public AgentProfitPeriodSnapshot ThisYear { get; set; } = new();
    }

    public class AgentProfitPeriodSnapshot
    {
        public decimal AdminProfit { get; set; }
        public decimal AgentEarnings { get; set; }
        public decimal PremiumVolume { get; set; }
        public int PoliciesIssued { get; set; }
        public decimal? AdminProfitChangePercentage { get; set; }
        public decimal? AgentEarningsChangePercentage { get; set; }
    }
}
