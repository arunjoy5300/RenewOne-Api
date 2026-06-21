namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class EnquiryDashboardResponse
    {
        public int TotalEnquiries { get; set; }
        public int Created { get; set; }
        public int Submitted { get; set; }
        public int PendingAssignment { get; set; }
        public int Quoted { get; set; }
        public int PolicyIssued { get; set; }
    }
}
