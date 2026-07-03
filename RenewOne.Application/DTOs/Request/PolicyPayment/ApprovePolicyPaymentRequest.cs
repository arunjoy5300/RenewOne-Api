namespace RenewOne.Application.DTOs.Request.PolicyPayment
{
    public class ApprovePolicyPaymentRequest
    {
        public Guid PolicyPaymentId { get; set; }
        public string? Remarks { get; set; }
    }
}
