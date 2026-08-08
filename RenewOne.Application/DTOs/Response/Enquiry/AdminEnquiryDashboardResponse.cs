using System;
using System.Collections.Generic;
using System.Text;

namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class AdminEnquiryDashboardResponse
    {
        public int TotalCount { get; set; }

        public int DraftCount { get; set; }

        public int CommonQueueCount { get; set; }

        public int QuoteCount { get; set; }

        public int QuotedCount { get; set; }

        public int PolicyCount { get; set; }

        public int IssuedPolicyCount { get; set; }

        public int ConvertedPolicyCount { get; set; }

        public int MyTaskCount { get; set; }

        public int UnassignedCount { get; set; }

        public int AgentAssistedCount { get; set; }

        public int AdminCreatedCount { get; set; }
    }
}
