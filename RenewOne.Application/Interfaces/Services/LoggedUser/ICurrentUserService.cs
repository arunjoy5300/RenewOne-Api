namespace RenewOne.Application.Interfaces.Services.LoggedUser
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }

        Guid AccountId { get; }

        string Role { get; }

        string? IpAddress { get; }
    }
}
