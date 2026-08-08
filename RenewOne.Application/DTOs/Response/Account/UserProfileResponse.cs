namespace RenewOne.Application.DTOs.Response.Account
{
    public class UserProfileResponse
    {
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
        public string ProfileUrl { get; set; }
        public string FullName { get; set; }
        public string AgencyName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
    }
}
