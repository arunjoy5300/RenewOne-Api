namespace RenewOne.Application.DTOs.Request.PolicyPayment
{
    public class RejectPolicyPaymentRequest
    {
        public Guid PolicyPaymentId { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
