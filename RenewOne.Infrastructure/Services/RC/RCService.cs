using RenewOne.Application.Interfaces.Services.RC;
using System.Text.Json;

namespace RenewOne.Infrastructure.Services.RC
{
    public class RCService : IRCService
    {
        public async Task<string> FetchVehicleDetailsAsync(string formattedNumber)
        {
            await Task.Delay(1000);

            var isCar = formattedNumber.Contains("C", StringComparison.OrdinalIgnoreCase);

            var dummyData = new
            {
                ChassisNumber = "MD2A" + new Random().Next(100000, 999999),
                EngineNumber = "DGL" + new Random().Next(100000, 999999),
                MakerModel = isCar ? "MARUTI SWIFT DZIRE" : "HONDA ACTIVA 6G",
                FuelType = "PETROL",
                CubicCapacity = isCar ? "1197" : "109"
            };

            return JsonSerializer.Serialize(dummyData);
        }
    }
}