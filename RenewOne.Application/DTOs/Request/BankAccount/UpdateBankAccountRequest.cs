namespace RenewOne.Application.DTOs.Request.BankAccount
{
    public class UpdateBankAccountRequest
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string? UpiId { get; set; }
        public bool IsPrimary { get; set; } 
        public bool IsActive { get; set; }
    }
}
