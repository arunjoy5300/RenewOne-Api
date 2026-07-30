using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.InsuranceCompany;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;

namespace RenewOne.Application.LogicService.AdminEntry
{
    public class InsuranceCompanyLogicService : IInsuranceCompanyLogicService
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        public InsuranceCompanyLogicService(IInsuranceCompanyRepository insuranceCompanyRepository)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
        }
        public async Task<Result<List<InsuranceCompanyDto>>> GetAllInsuranceCompaniesAsync(CancellationToken cancellationToken = default)
        {
            var insuranceCompanies = _insuranceCompanyRepository.GetQueryable()
                .Where(x => x.IsActive);

            var insuranceCompanyDtos = await insuranceCompanies.Select(x => new InsuranceCompanyDto 
            { 
                Id = x.Id, 
                CompanyName = x.CompanyName 
            }).ToListAsync();
            return Result<List<InsuranceCompanyDto>>.Success(insuranceCompanyDtos, "Insurance companies retrieved successfully");
        }
    }
}
