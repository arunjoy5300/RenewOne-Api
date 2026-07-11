using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.QRMapping;
using RenewOne.Application.Filters.QRMapping;
using RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging;

namespace RenewOne.API.Controllers.AdminEntry.QRManaging
{
    [ApiController]
    [Route("api/qr-mappings")]
    [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
    public class QRMappingController : ControllerBase
    {
        private readonly IQRMappingLogicService _logicService;

        public QRMappingController(IQRMappingLogicService logicService)
        {
            _logicService = logicService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateQRMappingRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.CreateAsync(request, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] QRMappingFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GetAllAsync(filter, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateQRMappingRequest request,
            CancellationToken cancellationToken)
        {
            request.Id = id;

            var result = await _logicService.UpdateAsync(request, cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ChangeStatus(
            Guid id,
            [FromQuery] bool isActive,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.ChangeStatusAsync(
                id,
                isActive,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
    }
}
