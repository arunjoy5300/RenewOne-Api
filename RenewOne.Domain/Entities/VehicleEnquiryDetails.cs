using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class VehicleEnquiryDetails : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string? IdvValue { get; set; }
        public virtual Enquiry? Enquiry { get; set; }
        public virtual VehicleType? VehicleType { get; set; }
    }
}
