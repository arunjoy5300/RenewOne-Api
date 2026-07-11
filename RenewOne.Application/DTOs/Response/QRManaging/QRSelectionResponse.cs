namespace RenewOne.Application.DTOs.Response.QRManaging
{
    public class QRSelectionResponse
    {
        public Guid QRMasterId { get; set; }

        public string QRName { get; set; } = string.Empty;

        public string QRImageUrl { get; set; } = string.Empty;

        public Guid BankAccountId { get; set; }

        public string BankName { get; set; } = string.Empty;

        public string BranchName { get; set; } = string.Empty;

        public string AccountHolderName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string IFSCCode { get; set; } = string.Empty;

        public string? UpiId { get; set; }
    }
}
