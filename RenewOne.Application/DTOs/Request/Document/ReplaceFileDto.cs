using Microsoft.AspNetCore.Http;

namespace RenewOne.Application.DTOs.Request.Document
{
    public class ReplaceFileDto
    {
        public Guid FileId { get; set; }
        public IFormFile File { get; set; } = default!;
    }
}
