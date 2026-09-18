using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.BankAccount;
using RenewOne.Application.Filters.BankAccount;
using RenewOne.Application.Interfaces.LogicService.BankAccounts;

namespace RenewOne.API.Controllers.AdminEntry.BankAccounts
{
    [ApiController]
    [Route("api/bank-accounts")]
    [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountLogicService _logicService;

        public BankAccountController(
            IBankAccountLogicService logicService)
        {
            _logicService = logicService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBankAccountRequest request)
        {
            var result = await _logicService.CreateAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] BankAccountFilter filter)
        {
            var result = await _logicService.GetAllAsync(filter);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _logicService.GetByIdAsync(id);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBankAccountRequest request)
        {
            request.Id = id;

            var result = await _logicService.UpdateAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ChangeStatus(
            Guid id,
            [FromQuery] bool isActive)
        {
            var result = await _logicService.ChangeStatusAsync(id, isActive);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("transactions")]
        public async Task<IActionResult> GetBankTransactions(
            [FromQuery] BankTransactionFilterRequest? filter, CancellationToken cancellationToken)
        {
            var result = await _logicService.GetBankTransactionsAsync(filter, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
