using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.AdminEntry;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;

namespace RenewOne.Application.LogicService.AdminEntry
{
    public class InsuranceTypeLogicService : IInsuranceTypeLogicService
    {
        private readonly IInsuranceTypeRepository _insuranceTypeRepository;
        private readonly ILogger<InsuranceTypeLogicService> _logger;
        public InsuranceTypeLogicService(IInsuranceTypeRepository insuranceTypeRepository, ILogger<InsuranceTypeLogicService> logger)
        {
            _insuranceTypeRepository = insuranceTypeRepository;
            _logger = logger;
        }
        public async Task<Result<List<InsuranceTypeResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching Insurance Types.");

                var insuranceTypes = await _insuranceTypeRepository
                    .GetQueryable()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new InsuranceTypeResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DisplayOrder = x.DisplayOrder,
                        IsActive = x.IsActive
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "{Count} Insurance Types fetched successfully.",
                    insuranceTypes.Count);

                return Result<List<InsuranceTypeResponse>>.Success(
                    insuranceTypes,
                    "Insurance Types fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching Insurance Types.");

                return Result<List<InsuranceTypeResponse>>.Failure(
                    "An error occurred while fetching Insurance Types.",
                    500);
            }
        }

        public async Task<Result<InsuranceTypeResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching Insurance Type. Id: {Id}",
                    id);

                var insuranceType = await _insuranceTypeRepository
                    .GetByIdAsync(id);

                if (insuranceType == null)
                {
                    _logger.LogWarning(
                        "Insurance Type not found. Id: {Id}",
                        id);

                    return Result<InsuranceTypeResponse>.NotFound(
                        "Insurance Type not found.");
                }

                var response = new InsuranceTypeResponse
                {
                    Id = insuranceType.Id,
                    Name = insuranceType.Name,
                    DisplayOrder = insuranceType.DisplayOrder,
                    IsActive = insuranceType.IsActive
                };

                return Result<InsuranceTypeResponse>.Success(
                    response,
                    "Insurance Type fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching Insurance Type. Id: {Id}",
                    id);

                return Result<InsuranceTypeResponse>.Failure(
                    "An error occurred while fetching Insurance Type.",
                    500);
            }
        }
    }
}
