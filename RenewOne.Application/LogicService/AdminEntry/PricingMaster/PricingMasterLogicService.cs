using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.DTOs.Response.PricingMaster;
using RenewOne.Application.Filters.Admin;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.AdminEntry.PricingMaster
{
    public class PricingMasterLogicService : IPricingMasterLogicService
    {
        private readonly IPricingMasterRepository _repository;

        public PricingMasterLogicService(IPricingMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> CreatePricingAsync(PricingMasterRequest request)
        {
            var validation = Validate(request);
            if (validation != null) return Result<bool>.Failure(validation, 400);

            try
            {
                var pricing = new Domain.Entities.PricingMaster
                {
                    Id = Guid.NewGuid(),
                    InsuranceCompanyId = request.InsuranceCompanyId,
                    AgentClassId = request.AgentClassId,
                    VehicleTypeId = request.VehicleTypeId,
                    MinCC = request.MinCC,
                    MaxCC = request.MaxCC,
                    FuelType = NormalizeFuel(request.FuelType),
                    TotalPremium = request.TotalPremium,
                    CompanyPayable = request.CompanyPayable,
                    AdminProfit = request.AdminProfit,
                    AgentPoints = request.AgentPoints,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _repository.AddPricingMaster(pricing);
                return success
                    ? Result<bool>.Success(true, "Pricing added successfully", 201)
                    : Result<bool>.Failure("Pricing could not be saved.", 500);
            }
            catch
            {
                return Result<bool>.Failure("Error creating pricing record.", 500);
            }
        }

        public async Task<Result<PagedResponse<PricingMasterResponse>>> GetAllPricingAsync(PricingMasterFilterRequest request)
        {
            try
            {
                var query = _repository.GetPricingMastersQuery();
                if (request.InsuranceCompanyId.HasValue) query = query.Where(x => x.InsuranceCompanyId == request.InsuranceCompanyId);
                if (request.AgentClassId.HasValue) query = query.Where(x => x.AgentClassId == request.AgentClassId);
                if (request.VehicleTypeId.HasValue) query = query.Where(x => x.VehicleTypeId == request.VehicleTypeId);
                if (!string.IsNullOrWhiteSpace(request.FuelType)) query = query.Where(x => x.FuelType == request.FuelType);
                if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var search = request.Search.Trim();
                    query = query.Where(x =>
                        x.InsuranceCompany!.CompanyName.Contains(search) ||
                        x.AgentClass!.ClassName.Contains(search) ||
                        x.VehicleType!.TypeName.Contains(search) ||
                        (x.FuelType != null && x.FuelType.Contains(search)));
                }

                var totalCount = await query.CountAsync();
                var pricing = await query
                    .OrderBy(x => x.InsuranceCompany!.CompanyName)
                    .ThenBy(x => x.AgentClass!.ClassName)
                    .ThenBy(x => x.VehicleType!.TypeName)
                    .ThenBy(x => x.MinCC)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var response = pricing.Select(ToResponse).ToList();
                return Result<PagedResponse<PricingMasterResponse>>.Success(
                    new PagedResponse<PricingMasterResponse>
                    {
                        Items = response,
                        TotalRecords = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                    },
                    "Pricing records fetched successfully");
            }
            catch
            {
                return Result<PagedResponse<PricingMasterResponse>>.Failure("Error fetching pricing records.", 500);
            }
        }

        public async Task<Result<PricingMasterResponse>> GetPricingByIdAsync(Guid id)
        {
            try
            {
                var pricing = await _repository.GetPricingMasterByIdAsync(id);
                return pricing == null
                    ? Result<PricingMasterResponse>.NotFound("Pricing record not found.")
                    : Result<PricingMasterResponse>.Success(ToResponse(pricing), "Pricing record fetched successfully.");
            }
            catch
            {
                return Result<PricingMasterResponse>.Failure("Error fetching pricing record.", 500);
            }
        }

        public async Task<Result<bool>> UpdatePricingAsync(Guid id, PricingMasterRequest request)
        {
            var validation = Validate(request);
            if (validation != null) return Result<bool>.Failure(validation, 400);

            try
            {
                var pricing = await _repository.GetPricingMasterByIdAsync(id);
                if (pricing == null) return Result<bool>.NotFound("Pricing record not found.");

                pricing.InsuranceCompanyId = request.InsuranceCompanyId;
                pricing.AgentClassId = request.AgentClassId;
                pricing.VehicleTypeId = request.VehicleTypeId;
                pricing.FuelType = NormalizeFuel(request.FuelType);
                pricing.MinCC = request.MinCC;
                pricing.MaxCC = request.MaxCC;
                pricing.TotalPremium = request.TotalPremium;
                pricing.CompanyPayable = request.CompanyPayable;
                pricing.AgentPoints = request.AgentPoints;
                pricing.AdminProfit = request.AdminProfit;
                pricing.UpdatedAt = DateTime.UtcNow;

                var success = await _repository.UpdatePricingMaster(pricing);
                return success
                    ? Result<bool>.Success(true, "Pricing updated successfully.")
                    : Result<bool>.Failure("Pricing could not be updated.", 500);
            }
            catch
            {
                return Result<bool>.Failure("Error updating pricing record.", 500);
            }
        }

        private static string? Validate(PricingMasterRequest request)
        {
            if (request.InsuranceCompanyId == Guid.Empty || request.AgentClassId == Guid.Empty || request.VehicleTypeId == Guid.Empty)
                return "Insurance company, agent class, and vehicle type are required.";
            if (request.MinCC < 0 || request.MaxCC <= request.MinCC)
                return "Minimum CC must be non-negative and less than maximum CC.";
            if (request.TotalPremium < 0 || request.CompanyPayable < 0 || request.AdminProfit < 0 || request.AgentPoints < 0)
                return "Premium and commission values cannot be negative.";
            if (request.FuelType?.Length > 50)
                return "Fuel type cannot exceed 50 characters.";
            return null;
        }

        private static string? NormalizeFuel(string? fuelType) => string.IsNullOrWhiteSpace(fuelType) ? null : fuelType.Trim();

        private static PricingMasterResponse ToResponse(Domain.Entities.PricingMaster pricing) => new()
        {
            Id = pricing.Id,
            InsuranceCompanyId = pricing.InsuranceCompanyId,
            AgentClassId = pricing.AgentClassId,
            VehicleTypeId = pricing.VehicleTypeId,
            InsuranceCompanyName = pricing.InsuranceCompany?.CompanyName ?? string.Empty,
            AgentClassName = pricing.AgentClass?.ClassName ?? string.Empty,
            VehicleTypeName = pricing.VehicleType?.TypeName ?? string.Empty,
            FuelType = pricing.FuelType,
            MinCC = pricing.MinCC,
            MaxCC = pricing.MaxCC,
            TotalPremium = pricing.TotalPremium,
            CompanyPayable = pricing.CompanyPayable,
            AgentPoints = pricing.AgentPoints,
            AdminProfit = pricing.AdminProfit,
            IsActive = pricing.IsActive
        };
    }
}
