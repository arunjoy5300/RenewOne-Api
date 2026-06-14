using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.Enquiry;

namespace RenewOne.API.Controllers.Enquiry
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryLogicService _enquiryLogicService;
        public EnquiryController(IEnquiryLogicService enquiryLogicService)
        {
            _enquiryLogicService = enquiryLogicService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnquiry(Guid id)
        {
            var result = await _enquiryLogicService.GetEnquiryAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }

    }
}
