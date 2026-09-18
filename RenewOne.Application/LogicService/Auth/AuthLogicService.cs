using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Auth;
using RenewOne.Application.DTOs.Response.Auth;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Auth;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Services.Jwt;
using System.Security.Cryptography;

namespace RenewOne.Application.LogicService.Auth
{
    public class AuthLogicService : IAuthLogicService
    {   
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        public AuthLogicService(IUserRepository userRepository, IJwtService jwtService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetByLoginIdentifierAsync(request.UserName);

                if (user == null) return Result<LoginResponse>.NotFound("User not found");

                if (user.IsActive == false) return Result<LoginResponse>.NotFound("Account is inactive");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Result<LoginResponse>.Unauthorized("Invalid password");

                var token = _jwtService.GenerateToken(user);

                var response = new LoginResponse
                {
                    UserId = user.Id,
                    AccountId = user?.AccountId,
                    Token = token,
                    FullName = user.FullName,
                    UserType = user.UserType
                };

                return Result<LoginResponse>.Success(response, "Login Success");
            }
            catch(Exception ex)
            {
                return Result<LoginResponse>.Failure($"An error occurred while processing the request: {ex}",500);
            }
        }

        public async Task<Result<GeneratedPasswordResetResponse>> ResetAgentPasswordWithGeneratedAsync(Guid accountId,CancellationToken cancellationToken = default)
        {

            // Find the primary agent user linked to this account
            var user = await _userRepository
                .GetAllUsers()
                .FirstOrDefaultAsync(u => u.AccountId == accountId && u.UserType == "Agent", cancellationToken);

            if (user is null)
            {
                return Result<GeneratedPasswordResetResponse>.Failure("Agent user not found for this account.");
            }

            // Generate cryptographically secure 6-digit number (100000 to 999999)
            var randomNumber = RandomNumberGenerator.GetInt32(100000, 1000000);
            var generatedPassword = $"Renew#{randomNumber}";

            // Hash and update credentials
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword, workFactor: 11);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var response = new GeneratedPasswordResetResponse
            {
                AccountId = accountId,
                UserId = user.Id,
                UserName = user.UserName,
                GeneratedPassword = generatedPassword,
                ResetAt = DateTime.UtcNow
            };

            return Result<GeneratedPasswordResetResponse>.Success(
                response,
                "Password has been reset successfully.");
        }
    }
}
