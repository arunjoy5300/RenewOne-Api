namespace RenewOne.Application.DTOs.Request.QRMapping
{
    public class CreateQRMasterRequest
    {
        public string UpiId { get; set; }
        public string UpiName { get; set; }
        public Guid BankAccountId { get; set; }
    }
}
