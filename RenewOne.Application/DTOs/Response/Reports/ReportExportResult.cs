namespace RenewOne.Application.DTOs.Response.Reports
{
    public sealed class ReportExportResult
    {
        public byte[] FileBytes { get; init; } = [];
        public string ContentType { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
    }
}
