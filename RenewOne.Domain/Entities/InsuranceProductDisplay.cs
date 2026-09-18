namespace RenewOne.Domain.Entities
{
    public class InsuranceProductDisplay
    {
        public Guid Id { get; set; }
        public Guid InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }  // Vehicle products only

        public string Name { get; set; } = string.Empty;
        public string Subtitle { get; set; } = "Insurance";
        public string Code { get; set; } = string.Empty;
        public string RoutePath { get; set; } = string.Empty;

        public string? BadgeText { get; set; }
        public string? BadgeColor { get; set; }
        public string IconKey { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public virtual InsuranceType InsuranceType { get; set; } = null!;
    }
}
