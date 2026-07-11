using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Auth;
using RenewOne.Application.DTOs.Response.Auth;
using RenewOne.Application.Interfaces.LogicService.Auth;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Services.Jwt;

namespace RenewOne.Application.LogicService.Auth
{
    public class AuthLogicService : IAuthLogicService
    {   
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        public AuthLogicService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
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
    }
}
