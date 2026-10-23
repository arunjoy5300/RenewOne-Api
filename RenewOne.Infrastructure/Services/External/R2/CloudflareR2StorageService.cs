
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RenewOne.Application.Interfaces.Services.External.R2;
using HeyRed.Mime;

namespace RenewOne.Infrastructure.Services.External.R2
{
    public class CloudflareR2StorageService : IFileStorageService
    {
        private readonly IConfiguration _config;

        public CloudflareR2StorageService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string? contentType = null)
        {
            var r2 = _config.GetSection("R2");

            contentType ??= MimeTypesMap.GetMimeType(fileName);

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp",
                "application/pdf"
            };

            if (!allowedTypes.Contains(contentType))
            {
                throw new InvalidOperationException(
                    $"File type '{contentType}' is not supported.");
            }

            // 10 MB limit
            const long maxFileSize = 10 * 1024 * 1024;

            if (fileStream.CanSeek)
            {
                if (fileStream.Length - fileStream.Position > maxFileSize)
                {
                    throw new InvalidOperationException(
                        "File size cannot exceed 10 MB.");
                }

                fileStream.Position = 0;
            }

            var config = new AmazonS3Config
            {
                ServiceURL = r2["ServiceUrl"],
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(
                r2["AccessKey"],
                r2["SecretKey"],
                config);

            var request = new PutObjectRequest
            {
                BucketName = r2["BucketName"],
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType,
                DisablePayloadSigning = true
            };

            await client.PutObjectAsync(request);

            // Return the original object key
            return fileName;
        }
    }
}
