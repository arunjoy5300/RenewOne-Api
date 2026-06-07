using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class VehicleDetails : BaseEntity
    {
        // Basic Vehicle
        public string VehicleNumber { get; set; } = string.Empty;

        public Guid VehicleTypeId { get; set; }
        public virtual VehicleType? VehicleType { get; set; }

        public string VehicleClass { get; set; } = string.Empty;

        public string ChassisNumber { get; set; } = string.Empty;

        public string EngineNumber { get; set; } = string.Empty;

        public string ManufacturerName { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string MakerModel { get; set; } = string.Empty;

        public string VehicleColor { get; set; } = string.Empty;

        public string FuelType { get; set; } = string.Empty;

        public string NormsType { get; set; } = string.Empty;

        public string BodyType { get; set; } = string.Empty;

        public string CubicCapacity { get; set; } = string.Empty;

        public string VehicleCategory { get; set; } = string.Empty;

        public string GrossVehicleWeight { get; set; } = string.Empty;

        public string UnladenWeight { get; set; } = string.Empty;

        public string Wheelbase { get; set; } = string.Empty;

        public string SeatCapacity { get; set; } = string.Empty;

        public string CylinderCount { get; set; } = string.Empty;

        public bool IsCommercial { get; set; }

        public bool IsFinanced { get; set; }

        // Owner Details
        public string OwnerName { get; set; } = string.Empty;

        public string OwnerFatherName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string OwnerCount { get; set; } = string.Empty;

        // Registration Details
        public string RegistrationAuthority { get; set; } = string.Empty;

        public string RegistrationDate { get; set; } = string.Empty;

        public string RcExpiryDate { get; set; } = string.Empty;

        public string ManufacturingMonthYear { get; set; } = string.Empty;

        public string VehicleTaxUpto { get; set; } = string.Empty;

        public string RtoCode { get; set; } = string.Empty;

        // Insurance Details
        public string InsuranceCompanyName { get; set; } = string.Empty;

        public string InsurancePolicyNumber { get; set; } = string.Empty;

        public string InsuranceValidUpto { get; set; } = string.Empty;

        // Finance
        public string FinancerName { get; set; } = string.Empty;

        // Pollution
        public string PuccNumber { get; set; } = string.Empty;

        public string PuccUpto { get; set; } = string.Empty;

        // Present Address
        public string PresentAddress { get; set; } = string.Empty;

        public string PresentDistrict { get; set; } = string.Empty;

        public string PresentState { get; set; } = string.Empty;

        public string PresentCity { get; set; } = string.Empty;

        public string PresentPincode { get; set; } = string.Empty;

        // Permanent Address
        public string PermanentAddress { get; set; } = string.Empty;

        public string PermanentDistrict { get; set; } = string.Empty;

        public string PermanentState { get; set; } = string.Empty;

        public string PermanentCity { get; set; } = string.Empty;

        public string PermanentPincode { get; set; } = string.Empty;

        // Raw External Response
        public string? RawApiResponse { get; set; }

        // Navigation
        public virtual ICollection<VehicleEnquiryDetails> EnquiryDetails { get; set; }
            = new HashSet<VehicleEnquiryDetails>();
    }
}
