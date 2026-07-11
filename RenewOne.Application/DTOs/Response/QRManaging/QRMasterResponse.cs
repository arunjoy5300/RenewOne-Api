namespace RenewOne.Application.DTOs.Response.QRMapping
{
    public class QRMasterResponse
    {
        public Guid Id { get; set; }
        public Guid BankAccountId { get; set; }
        public string BankAccount { get; set; } = string.Empty;
        public string UpiId { get; set; } = string.Empty;
        public string UpiName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

    }
}
