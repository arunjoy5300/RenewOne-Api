namespace RenewOne.Application.DTOs.Response.Service
{
    public class CashfreeCreateOrderResponse
    {
        public string Cf_Order_Id { get; set; }

        public string Order_Id { get; set; }

        public decimal Order_Amount { get; set; }

        public string Order_Currency { get; set; }

        public string Payment_Session_Id { get; set; }

        public string Order_Status { get; set; }

        public DateTime Order_Expiry_Time { get; set; }

        public CashfreeCustomerDetails Customer_Details { get; set; }

        public CashfreeOrderMeta Order_Meta { get; set; }

        public string Order_Note { get; set; }
    }

    public class CashfreeCustomerDetails
    {
        public string Customer_Id { get; set; }
        public string Customer_Name { get; set; }
        public string Customer_Email { get; set; }
        public string Customer_Phone { get; set; }
    }

    public class CashfreeOrderMeta
    {
        public string Return_Url { get; set; }
        public string Notify_Url { get; set; }
    }
}
