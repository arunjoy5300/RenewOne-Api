using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
