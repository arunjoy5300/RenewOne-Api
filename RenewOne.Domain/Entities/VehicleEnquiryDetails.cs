using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class VehicleEnquiryDetails : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public Guid VehicleDetailsId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string? IdvValue { get; set; }

        // Optional fields for RC details
        public string? MIPolicyType { get; set; }
        public bool IsRcOwnerChanged { get; set; }
        public bool HasPreviousClaim { get; set; }
        public int NcbPercentage { get; set; } = 0;
        public bool IsApiFetchedData { get; set; }
        public virtual Enquiry? Enquiry { get; set; }
        public virtual VehicleDetails? VehicleDetails { get; set; }
        public virtual VehicleType VehicleType { get; set; }
    }
}
