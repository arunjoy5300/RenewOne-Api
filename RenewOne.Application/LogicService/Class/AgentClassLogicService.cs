using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Class;
using RenewOne.Application.DTOs.Request.Account;
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
                var agentClasses = result.Select(ToView).ToList();

                return Result<IEnumerable<ViewAgentClass>>.Success(agentClasses, "Agent classes retrieved successfully");
            }
            return Result<IEnumerable<ViewAgentClass>>.NotFound("No agent classes found");
        }
        public async Task<Result<ViewAgentClass>> GetAgentClassById(Guid id)
        {
            var result = await _agentClassRepository.GetByIdAsync(id);
            if (result != null)
            {
                var agentClass = ToView(result);
                return Result<ViewAgentClass>.Success(agentClass, "Agent class retrieved successfully");
            }
            return Result<ViewAgentClass>.NotFound("Agent class not found");
        }

        public async Task<Result<ViewAgentClass>> CreateAgentClass(AgentClassRequest request)
        {
            var validation = Validate(request);
            if (validation != null) return Result<ViewAgentClass>.Failure(validation);
            var className = request.ClassName.Trim();
            if (await _agentClassRepository.ExistsByNameAsync(className))
                return Result<ViewAgentClass>.Conflict("An agent class with this name already exists.");

            var entity = new Domain.Entities.AgentClass
            {
                ClassName = className,
                CommSharePercentage = request.CommSharePercentage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            return await _agentClassRepository.AddAsync(entity)
                ? Result<ViewAgentClass>.Success(ToView(entity), "Agent class created successfully", 201)
                : Result<ViewAgentClass>.Failure("Agent class could not be saved.", 500);
        }

        public async Task<Result<ViewAgentClass>> UpdateAgentClass(Guid id, AgentClassRequest request)
        {
            var validation = Validate(request);
            if (validation != null) return Result<ViewAgentClass>.Failure(validation);
            var entity = await _agentClassRepository.GetByIdAsync(id);
            if (entity == null) return Result<ViewAgentClass>.NotFound("Agent class not found.");
            var className = request.ClassName.Trim();
            if (await _agentClassRepository.ExistsByNameAsync(className, id))
                return Result<ViewAgentClass>.Conflict("An agent class with this name already exists.");
            entity.ClassName = className;
            entity.CommSharePercentage = request.CommSharePercentage;
            entity.UpdatedAt = DateTime.UtcNow;
            return await _agentClassRepository.UpdateAsync(entity)
                ? Result<ViewAgentClass>.Success(ToView(entity), "Agent class updated successfully")
                : Result<ViewAgentClass>.Failure("Agent class could not be updated.", 500);
        }

        public async Task<Result<bool>> ToggleAgentClassStatus(Guid id)
        {
            var entity = await _agentClassRepository.GetByIdAsync(id);
            if (entity == null) return Result<bool>.NotFound("Agent class not found.");
            var saved = await _agentClassRepository.ToggleStatusAsync(entity);
            return saved
                ? Result<bool>.Success(true, $"Agent class {(entity.IsActive ? "activated" : "deactivated")} successfully")
                : Result<bool>.Failure("Agent class status could not be changed.", 500);
        }

        private static string? Validate(AgentClassRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClassName)) return "Class name is required.";
            if (request.ClassName.Trim().Length > 100) return "Class name cannot exceed 100 characters.";
            if (request.CommSharePercentage < 0 || request.CommSharePercentage > 100)
                return "Commission share must be between 0 and 100 percent.";
            return null;
        }

        private static ViewAgentClass ToView(Domain.Entities.AgentClass entity)
        {
            var agents = entity.Accounts
                .Where(account => account.Users.Any(user => user.UserType == "Agent"))
                .Select(account => new AgentClassAgent
                {
                    Id = account.Id,
                    AgentName = account.AgentName,
                    AgentCode = account.Code,
                    ContactNumber = account.ContactNumber,
                    Email = account.Email,
                    IsActive = account.IsActive
                }).ToList();
            return new ViewAgentClass
            {
                Id = entity.Id,
                ClassName = entity.ClassName,
                CommSharePercentage = entity.CommSharePercentage,
                IsActive = entity.IsActive,
                AgentCount = agents.Count,
                Agents = agents
            };
        }
    }
}
