using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.QRMapping;
using RenewOne.Application.Filters.QRMaster;
using RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging;

namespace RenewOne.API.Controllers.AdminEntry.QRManaging
{
    [ApiController]
    [Route("api/qr-master")]
    [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
    public class QRMasterController : ControllerBase
    {
        private readonly IQRMasterLogicService _logicService;

        public QRMasterController(IQRMasterLogicService logicService)
        {
            _logicService = logicService;
        }

        /// <summary>
        /// Create QR Master
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateQRMasterRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.CreateAsync(
                request,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get QR Masters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] QRMasterFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GetAllAsync(
                filter,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get QR Master By Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GetByIdAsync(
                id,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Update QR Master
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateQRMasterRequest request,
            CancellationToken cancellationToken)
        {
            request.Id = id;

            var result = await _logicService.UpdateAsync(
                request,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Activate / Deactivate QR Master
        /// </summary>
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
