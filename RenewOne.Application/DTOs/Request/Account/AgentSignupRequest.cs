namespace RenewOne.Application.DTOs.Request.Account
{
    public class AgentSignupRequest
    {
        public string AgentName { get; set; }
        public string UserName { get; set; }
        //public string FullName { get; set; }
        //public string? Code { get; set; }
        //public Guid AgentClassId { get; set; }
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
