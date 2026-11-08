using System.ComponentModel.DataAnnotations;

namespace RenewOne.Application.DTOs.Request.Account
{
    public class UpdateAgentAdminDetailsRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string AgentName { get; set; } = string.Empty;

        [Required, StringLength(254), EmailAddress, RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$")]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [RegularExpression(@"^$|^[0-9]{6}$", ErrorMessage = "Pincode must be empty or exactly 6 digits.")]
        public string? Pincode { get; set; }
    }
}
