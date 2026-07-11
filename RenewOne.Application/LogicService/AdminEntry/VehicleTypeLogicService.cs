using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.AdminEntry;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;

namespace RenewOne.Application.LogicService.AdminEntry
{
    public class VehicleTypeLogicService : IVehicleTypeLogicService
    {
        private readonly IVehicleTypeRepository _vehicleTypeRepository;
        private readonly ILogger<VehicleTypeLogicService> _logger;
        public VehicleTypeLogicService(IVehicleTypeRepository vehicleTypeRepository, ILogger<VehicleTypeLogicService> logger)
        {
            _vehicleTypeRepository = vehicleTypeRepository;
            _logger = logger;
        }
        public async Task<Result<List<VehicleTypeResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching Vehicle Types.");

                var vehicleTypes = await _vehicleTypeRepository
                    .GetQueryable()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.TypeName)
                    .Select(x => new VehicleTypeResponse
                    {
                        Id = x.Id,
                        TypeName = x.TypeName,
                        Description = x.Description,
                        IsActive = x.IsActive
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "{Count} Vehicle Types fetched successfully.",
                    vehicleTypes.Count);

                return Result<List<VehicleTypeResponse>>.Success(
                    vehicleTypes,
                    "Vehicle Types fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching Vehicle Types.");

                return Result<List<VehicleTypeResponse>>.Failure(
                    "An error occurred while fetching Vehicle Types.",
                    500);
            }
        }

        public async Task<Result<VehicleTypeResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching Vehicle Type. Id: {Id}",
                    id);

                var vehicleType = await _vehicleTypeRepository.GetByIdAsync(id);

                if (vehicleType == null)
                {
                    _logger.LogWarning(
                        "Vehicle Type not found. Id: {Id}",
                        id);

                    return Result<VehicleTypeResponse>.NotFound(
                        "Vehicle Type not found.");
                }

                var response = new VehicleTypeResponse
                {
                    Id = vehicleType.Id,
                    TypeName = vehicleType.TypeName,
                    Description = vehicleType.Description,
                    IsActive = vehicleType.IsActive
                };

                return Result<VehicleTypeResponse>.Success(
                    response,
                    "Vehicle Type fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching Vehicle Type. Id: {Id}",
                    id);

                return Result<VehicleTypeResponse>.Failure(
                    "An error occurred while fetching Vehicle Type.",
                    500);
            }
        }
    }
}
