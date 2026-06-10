namespace RenewOne.Application.Interfaces.Services.RC
{
    public interface IRCService
    {
        Task<string> FetchVehicleDetailsAsync(string formattedNumber);
    }
}
