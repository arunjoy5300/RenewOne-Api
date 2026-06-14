using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Services.External.RC
{
    public interface IRCIDSService
    {
        Task<RcApiDto> FetchVehicleDetailsAsync(string vehicleNumber, CancellationToken cancellationToken = default);
    }
}
