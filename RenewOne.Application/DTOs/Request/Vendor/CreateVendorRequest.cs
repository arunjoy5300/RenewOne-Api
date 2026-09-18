namespace RenewOne.Application.DTOs.Request.Vendor
{
    public class CreateVendorRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid BankAccountId { get; set; }
    }
}
