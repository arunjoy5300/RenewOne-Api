namespace RenewOne.Application.DTOs.Response.RC
{
    public class VehicleRcResponse
    {
        public string RegnNumber { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string IssuedBy { get; set; } = string.Empty;
        public string RegnDate { get; set; } = string.Empty;
        public string RegnValidity { get; set; } = string.Empty;
        public int OwnerSerial { get; set; }
        public string ChassisNo { get; set; } = string.Empty;
        public string EngineNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string SonWifeDaughterOf { get; set; } = string.Empty;
        public string Ownership { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Fuel { get; set; } = string.Empty;
        public string EmissionNorms { get; set; } = string.Empty;
        public string CardIssueDate { get; set; } = string.Empty;
        public string MfgMonthYear { get; set; } = string.Empty;
        public int NoOfCylinders { get; set; }
        public string FormType { get; set; } = string.Empty;
        public string VehicleClass { get; set; } = string.Empty;
        public string MakerName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;
        public int SeatingCapacity { get; set; }
        public int UnladenWeight { get; set; }
        public string CubicCapacity { get; set; } = string.Empty;
        public string HorsePower { get; set; } = string.Empty;
        public string WheelBase { get; set; } = string.Empty;
        public string Financier { get; set; } = string.Empty;
        public string RegAuthority { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string AuthoritySignUrl { get; set; } = string.Empty;
    }
}
