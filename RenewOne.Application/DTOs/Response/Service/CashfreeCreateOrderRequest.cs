using System;
using System.Collections.Generic;
using System.Text;

namespace RenewOne.Application.DTOs.Response.Service
{
    public class CashfreeCreateOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string OrderNote { get; set; } = string.Empty;
    }
}
