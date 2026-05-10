using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class LoginHistory : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public string? IpAddress { get; set; }
        public string? DeviceDetails { get; set; }
        public bool Status { get; set; }
        public virtual User? User { get; set; }
    }
}
