namespace RenewOne.Application.DTOs.Response.Account
{
    public class AgentListItem
    {
        public Guid AccountId { get; set; }

        public string AgentId { get; set; }

        public string AgentName { get; set; }

        public string AgencyName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Pincode { get; set; }
        public string IsApproved { get; set; }

        public string Status { get; set; }

        public DateTime JoinedOn { get; set; }
    }
}
