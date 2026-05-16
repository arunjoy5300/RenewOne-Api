using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.User;
using RenewOne.Application.DTOs.Response.User;
using RenewOne.Application.Interfaces.LogicService.User;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.Users
{
    public class UserLogicService : IUserLogicService
    {
        private readonly IUserRepository _userRepository;
        public UserLogicService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Result<bool>> CreateStaffAsync(CreateStaffRequest request, Guid accountId)
        {
            if (await _userRepository.GetByUserNameAsync(request.UserName) != null)
                return Result<bool>.Conflict("Username already exists");

            if (await _userRepository.GetByLoginIdentifierAsync(request.Email) != null)
                return Result<bool>.Conflict("Email already exists");

            var newStaff = new User
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserType = "Staff",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddUserAsync(newStaff);
            return Result<bool>.Success(true, "Staff Created Successfully", 201);
        }

        public async Task<Result<IEnumerable<StaffResponse>>> GetMyStaffAsync(Guid accountId)
        {
            var staffList = await _userRepository.GetAgentStaffByAccountAsync(accountId);

            var response = staffList.Select(s => new StaffResponse
            {
                Id = s.Id,
                UserName = s.UserName,
                FullName = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            });

            return Result<IEnumerable<StaffResponse>>.Success(response);
        }

        public async Task<Result<bool>> ToggleStaffStatusAsync(Guid staffId, Guid accountId)
        {
            var staff = await _userRepository.GetByIdAsync(staffId);

            if (staff == null || staff.AccountId != accountId)
                return Result<bool>.NotFound("Staff not exists");

            staff.IsActive = !staff.IsActive;
            await _userRepository.UpdateUserAsync(staff);

            string statusMessage = staff.IsActive ? "Staff account activated." : "Staff account deactivated.";

            return Result<bool>.Success(true, statusMessage);
        }
    }
}
