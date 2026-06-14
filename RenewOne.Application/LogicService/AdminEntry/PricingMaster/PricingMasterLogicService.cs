using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.DTOs.Response.PricingMaster;
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
                    TotalPremium = request.TotalPremium,
                    CompanyPayable = request.CompanyPayable,
                    AdminProfit = request.AdminProfit,
                    AgentPoints = request.AgentPoints,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _repository.AddPricingMaster(pricing);
                return success ? Result<bool>.Success(true, "Pricing added", 201)
                               : Result<bool>.Failure("Failed to add pricing", 500);
            }
            catch(Exception ex)
            {
                return Result<bool>.Failure("Error creating records");
            }
        }

        public async Task<Result<List<PricingMasterResponse>>> GetAllPricingAsync()
        {
            try
            {
                var result = await _repository.GetAllPricingMaster();
                if(result == null)
                {
                    return Result<List<PricingMasterResponse>>.NotFound("No records found");
                }

                var response = result.Select(p => new PricingMasterResponse
                {
                    Id = p.Id,
                    TotalPremium = p.TotalPremium,
                    AgentClassName = p.AgentClass.ClassName,
                    VehicleTypeName = p.VehicleType.TypeName,
                    InsuranceCompanyName = p.InsuranceCompany.CompanyName
                }).ToList();

                return Result<List<PricingMasterResponse>>.Success(response, "Records fetched successfully");
            }
            catch(Exception ex)
            {
                return Result<List<PricingMasterResponse>>.Failure("Error fetching records");
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
