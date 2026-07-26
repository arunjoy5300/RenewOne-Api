namespace RenewOne.Application.DTOs.Request.Document
{
    public class DocumentListDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string DocType { get; set; }
        public string DocSide { get; set; }
        public string Url { get; set; }
    }
}
