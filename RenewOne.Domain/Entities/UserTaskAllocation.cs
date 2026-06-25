using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class UserTaskAllocation : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid InsuranceTypeId { get; set; } 
        public Guid? VehicleTypeId { get; set; }
        public bool CanAssign { get; set; } = true;
        public virtual User User { get; set; }
        public virtual InsuranceType InsuranceType { get; set; }
        public virtual VehicleType? VehicleType { get; set; }
    }
}
