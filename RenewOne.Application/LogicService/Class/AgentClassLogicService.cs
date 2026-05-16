using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Class;
using RenewOne.Application.Interfaces.LogicService.Class;
using RenewOne.Application.Interfaces.Repositories.Class;

namespace RenewOne.Application.LogicService.Class
{
    public class AgentClassLogicService : IAgentClassLogicService
    {
        private readonly IAgentClassRepository _agentClassRepository;
        public AgentClassLogicService(IAgentClassRepository agentClassRepository)
        {
            _agentClassRepository = agentClassRepository;
        }
        public async Task<Result<IEnumerable<ViewAgentClass>>> GetAllAgentClasses()
        {
            var result = await _agentClassRepository.GetAllAsync();
            if (result.Any())
            {
                var agentClasses = result.Select(ac => new ViewAgentClass
                {
                    Id = ac.Id,
                    ClassName = ac.ClassName,
                    CommSharePercentage = ac.CommSharePercentage,
                    IsActive = ac.IsActive
                }).ToList();

                return Result<IEnumerable<ViewAgentClass>>.Success(agentClasses, "Agent classes retrieved successfully");
            }
            return Result<IEnumerable<ViewAgentClass>>.NotFound("No agent classes found");
        }
        public async Task<Result<ViewAgentClass>> GetAgentClassById(Guid id)
        {
            var result = await _agentClassRepository.GetByIdAsync(id);
            if (result != null)
            {
                var agentClass = new ViewAgentClass
                {
                    Id = result.Id,
                    ClassName = result.ClassName,
                    CommSharePercentage = result.CommSharePercentage,
                    IsActive = result.IsActive
                };
                return Result<ViewAgentClass>.Success(agentClass, "Agent class retrieved successfully");
            }
            return Result<ViewAgentClass>.NotFound("Agent class not found");
        }
    }
}