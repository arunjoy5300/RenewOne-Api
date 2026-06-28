using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class QRMapping : BaseEntity
    {
        public Guid QRMasterId { get; set; }
        public Guid InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public bool IsActive { get; set; }
        public virtual QRMaster QRMaster { get; set; } = null!;
        public virtual InsuranceType InsuranceType { get; set; } = null!;
        public virtual VehicleType? VehicleType { get; set; }
    }
}
