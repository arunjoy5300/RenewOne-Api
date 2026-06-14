using System;
using System.Collections.Generic;
using System.Text;

namespace RenewOne.Application.DTOs.Response.PricingMaster
{
    public class PricingMasterResponse
    {
        public Guid Id { get; set; }

        public decimal TotalPremium { get; set; }

        public string? AgentClassName { get; set; }

        public string? VehicleTypeName { get; set; }

        public string? InsuranceCompanyName { get; set; }
    }
}
