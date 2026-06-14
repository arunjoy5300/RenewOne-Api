namespace RenewOne.Application.Interfaces.Services.External.R2
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream,string fileName,string contentType);
    }
}
