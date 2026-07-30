using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class UserModulePermission : BaseEntity
    {
        public Guid UserId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

    }
}
