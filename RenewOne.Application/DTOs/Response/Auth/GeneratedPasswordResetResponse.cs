namespace RenewOne.Application.DTOs.Response.Auth
{
    public class GeneratedPasswordResetResponse
    {
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GeneratedPassword { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
    }
}
