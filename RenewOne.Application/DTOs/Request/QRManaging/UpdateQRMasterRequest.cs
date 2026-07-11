namespace RenewOne.Application.DTOs.Request.QRMapping
{
    public class UpdateQRMasterRequest
    {
        public Guid Id { get; set; }
        public string UpiId { get; set; }
        public string UpiName { get; set; }
        public Guid BankAccountId { get; set; }
        public bool IsActive { get; set; }
    }
}
