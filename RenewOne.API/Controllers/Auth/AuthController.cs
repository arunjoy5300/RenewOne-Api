using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.Auth;
using LoginRequest = RenewOne.Application.DTOs.Request.Auth.LoginRequest;

namespace RenewOne.API.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthLogicService _authLogicService;
        public AuthController(IAuthLogicService authLogicService)
        {
            _authLogicService = authLogicService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _authLogicService.LoginAsync(loginRequest);
            return StatusCode(result.StatusCode, result);
        }
    }
}
