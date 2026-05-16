using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Class;

namespace RenewOne.Application.Interfaces.LogicService.Class
{
    public interface IAgentClassLogicService
    {
        Task<Result<IEnumerable<ViewAgentClass>>> GetAllAgentClasses();
        Task<Result<ViewAgentClass>> GetAgentClassById(Guid id);
    }
}
