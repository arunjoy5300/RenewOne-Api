using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.DTOs.Response.PricingMaster;
using RenewOne.Application.Filters.Admin;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;

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
            if (request.MinCC >= request.MaxCC)
                return Result<bool>.Failure("Minimum CC should be lesser than Maximum CC", 400);

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
                    FuelType = request.FuelType,
                    TotalPremium = request.TotalPremium,
                    CompanyPayable = request.CompanyPayable,
                    AdminProfit = request.AdminProfit,
                    AgentPoints = request.AgentPoints,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _repository.AddPricingMaster(pricing);
                if (!success) 
                {
                    return Result<bool>.Failure("Pricing added successfully");
                }
                return Result<bool>.Success(true, "Pricing added successfully",201);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure("Error creating records",500);
            }
        }

        public async Task<Result<PagedResponse<PricingMasterResponse>>> GetAllPricingAsync(PricingMasterFilterRequest request)
        {
            try
            {
                var query = _repository.GetPricingMastersQuery();

                // Filters
                if (request.InsuranceCompanyId.HasValue)
                    query = query.Where(x => x.InsuranceCompanyId == request.InsuranceCompanyId);

                if (request.AgentClassId.HasValue)
                    query = query.Where(x => x.AgentClassId == request.AgentClassId);

                if (request.VehicleTypeId.HasValue)
                    query = query.Where(x => x.VehicleTypeId == request.VehicleTypeId);

                if (!string.IsNullOrWhiteSpace(request.FuelType))
                    query = query.Where(x => x.FuelType == request.FuelType);

                if (request.IsActive.HasValue)
                    query = query.Where(x => x.IsActive == request.IsActive);

                // Total count
                var totalCount = await query.CountAsync();
                Console.WriteLine($"Total Count: {totalCount}");

                // Pagination
                var pricing = await query
                    .OrderBy(x => x.Id)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var response = pricing.Select(x => new PricingMasterResponse
                {
                    Id = x.Id,
                    InsuranceCompanyName = x.InsuranceCompany.CompanyName,
                    AgentClassName = x.AgentClass.ClassName,
                    VehicleTypeName = x.VehicleType.TypeName,
                    FuelType = x.FuelType,
                    MinCC = x.MinCC,
                    MaxCC = x.MaxCC,
                    TotalPremium = x.TotalPremium,
                    CompanyPayable = x.CompanyPayable,
                    AgentPoints = x.AgentPoints,
                    AdminProfit = x.AdminProfit,
                    IsActive = x.IsActive
                }).ToList();

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
            catch(Exception ex)
            {
                return Result<PagedResponse<PricingMasterResponse>>.Failure($"Error fetching records: {ex.Message}");
            }
        }

        public async Task<Result<PricingMasterResponse>> GetPricingByIdAsync(Guid id)
        {
            try
            {
                var result = await _repository.GetPricingMasterByIdAsync(id);
                if (result != null)
                {
                    var response = new PricingMasterResponse
                    {
                        Id = result.Id,
                        TotalPremium = result.TotalPremium,
                        AgentClassName = result.AgentClass.ClassName,
                        VehicleTypeName = result.VehicleType.TypeName,
                        InsuranceCompanyName = result.InsuranceCompany.CompanyName
                    };
                    return Result<PricingMasterResponse>.Success(response, "Record fetched successfully");
                }
                return Result<PricingMasterResponse>.NotFound("No records found");
            }
            catch(Exception ex)
            {
                return Result<PricingMasterResponse>.Failure("Error fetching records");
            }
        }

        public async Task<Result<bool>> UpdatePricingAsync(Guid id, PricingMasterRequest request)
        {
            if (request == null)
            {
                return Result<bool>.Failure("Payload cannot be null");
            }
            try
            {
                var pricing = await _repository.GetPricingMasterByIdAsync(id);
                if(pricing == null)
                {
                    return Result<bool>.Failure("Pricing not found", 404);
                }
                pricing.InsuranceCompanyId = request.InsuranceCompanyId;
                pricing.TotalPremium = request.TotalPremium;
                pricing.CompanyPayable = request.CompanyPayable;
                pricing.AdminProfit = request.AdminProfit;
                pricing.UpdatedAt = DateTime.UtcNow;


                await _repository.UpdatePricingMaster(pricing);
                return Result<bool>.Success(true, "Updated successfully", 200);
            }
            catch(Exception ex)
            {
                return Result<bool>.Failure("Error updating records");
            }
            

        }
    }
}
