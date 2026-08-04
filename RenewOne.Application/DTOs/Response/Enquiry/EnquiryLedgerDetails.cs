using System;
using System.Collections.Generic;
using System.Text;

namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class EnquiryLedgerDetails
    {
        public decimal LedgerBalance { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal BalanceAfterCut { get; set; }
    }
}
