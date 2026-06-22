using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RenewOne.Application.DTOs.Request.Service
{
    public class CashfreeWebhookRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("event_time")]
        public DateTime EventTime { get; set; }

        [JsonPropertyName("data")]
        public CashfreeWebhookData Data { get; set; } = new();
    }
    public class CashfreeWebhookData
    {
        [JsonPropertyName("order")]
        public CashfreeOrder Order { get; set; } = new();

        [JsonPropertyName("payment")]
        public CashfreePayment Payment { get; set; } = new();
    }
    public class CashfreeOrder
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("order_amount")]
        public decimal OrderAmount { get; set; }

        [JsonPropertyName("order_currency")]
        public string OrderCurrency { get; set; } = string.Empty;
    }
    public class CashfreePayment
    {
        [JsonPropertyName("cf_payment_id")]
        public string CfPaymentId { get; set; } = string.Empty;

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; } = string.Empty;

        [JsonPropertyName("payment_amount")]
        public decimal PaymentAmount { get; set; }

        [JsonPropertyName("payment_currency")]
        public string PaymentCurrency { get; set; } = string.Empty;

        [JsonPropertyName("payment_time")]
        public DateTime PaymentTime { get; set; }

        [JsonPropertyName("bank_reference")]
        public string BankReference { get; set; } = string.Empty;
    }
}
