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
                return Result<GeneratedPasswordResetResponse>.NotFound("Agent user not found for this account.");
            }

            var generatedPassword = GenerateTemporaryPassword();

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
                UserType = user.UserType,
                GeneratedPassword = generatedPassword,
                ResetAt = DateTime.UtcNow
            };

            return Result<GeneratedPasswordResetResponse>.Success(
                response,
                "Password has been reset successfully.");
        }

        public async Task<Result<GeneratedPasswordResetResponse>> ResetStaffPasswordWithGeneratedAsync(Guid accountId, Guid staffUserId, CancellationToken cancellationToken = default)
        {
            var staff = await _userRepository.GetAllUsers()
                .FirstOrDefaultAsync(x => x.Id == staffUserId && x.AccountId == accountId && x.UserType == "Staff", cancellationToken);
            if (staff is null)
                return Result<GeneratedPasswordResetResponse>.NotFound("Staff user was not found under this agent account.");

            var generatedPassword = GenerateTemporaryPassword();
            staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword, workFactor: 11);
            staff.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(staff);
            await _unitOfWork.SaveChangesAsync();

            return Result<GeneratedPasswordResetResponse>.Success(new GeneratedPasswordResetResponse
            {
                AccountId = accountId,
                UserId = staff.Id,
                UserName = staff.UserName,
                UserType = staff.UserType,
                GeneratedPassword = generatedPassword,
                ResetAt = DateTime.UtcNow
            }, "Staff password has been reset successfully.");
        }

        private static string GenerateTemporaryPassword()
        {
            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowercase = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%&*?-_";
            var alphabet = uppercase + lowercase + digits + symbols;
            var characters = new char[20];
            characters[0] = Pick(uppercase);
            characters[1] = Pick(lowercase);
            characters[2] = Pick(digits);
            characters[3] = Pick(symbols);
            for (var index = 4; index < characters.Length; index++)
                characters[index] = Pick(alphabet);
            for (var index = characters.Length - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (characters[index], characters[swapIndex]) = (characters[swapIndex], characters[index]);
            }
            return new string(characters);
        }

        private static char Pick(string characters) => characters[RandomNumberGenerator.GetInt32(characters.Length)];
    }
}
