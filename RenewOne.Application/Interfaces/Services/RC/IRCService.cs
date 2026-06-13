using RenewOne.Application.DTOs.Response.RC;

namespace RenewOne.Application.Interfaces.Services.RC
{
    public interface IRCService
    {
        Task<RcApiDto> FetchVehicleDetailsAsync(string vehicleNumber, CancellationToken cancellationToken = default);
    }
}
