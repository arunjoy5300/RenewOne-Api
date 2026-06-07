using RenewOne.Application.Common.Models;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Services.External.RC
{
    public interface IRCIDSService
    {
        Task<Result<VehicleDetails>> GetVehicleDetailsAsync(string regNumber);
    }
}
