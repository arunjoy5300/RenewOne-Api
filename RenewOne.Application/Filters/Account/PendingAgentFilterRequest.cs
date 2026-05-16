namespace RenewOne.Application.Filters.Account
{
    public class PendingAgentFilterRequest
    {
        public string FilterType { get; set; } = "all";
        public DateTime? CustomDate { get; set; }
    }
}
