using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.InsuranceCompany;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry
{
    public interface IInsuranceCompanyLogicService
    {
        Task<Result<List<InsuranceCompanyDto>>> GetAllInsuranceCompaniesAsync(CancellationToken cancellationToken = default);
    }
}
