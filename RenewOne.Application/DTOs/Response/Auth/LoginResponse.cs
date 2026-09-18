namespace RenewOne.Application.DTOs.Response.Auth
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }
}
