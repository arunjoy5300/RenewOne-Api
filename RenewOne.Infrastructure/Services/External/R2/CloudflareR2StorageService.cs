using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RenewOne.Application.Interfaces.Services.External.R2;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string? contentType = null)
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
            if (fileStream.CanSeek)
            {
                const long maxFileSize = 10 * 1024 * 1024;

                if (fileStream.Length > maxFileSize)
                {
                    throw new InvalidOperationException(
                        "File size cannot exceed 10 MB.");
                }
            }

            Stream uploadStream = fileStream;

            // Compress images only
            if (contentType.StartsWith("image/"))
            {
                uploadStream = await CompressImageAsync(fileStream);

                // Since we're saving as JPEG
                contentType = "image/jpeg";

                // Optional: change extension to .jpg
                fileName = Path.ChangeExtension(fileName, ".jpg");
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
                InputStream = uploadStream,
                ContentType = contentType,
                DisablePayloadSigning = true
            };

            await client.PutObjectAsync(request);

            // Dispose compressed stream if created
            if (uploadStream != fileStream)
            {
                await uploadStream.DisposeAsync();
            }

            // Return only object key/path
            return fileName;
        }

        private static async Task<Stream> CompressImageAsync(Stream inputStream)
        {
            inputStream.Position = 0;

            using var image = await Image.LoadAsync(inputStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(1600, 1600)
            }));

            var outputStream = new MemoryStream();

            await image.SaveAsJpegAsync(
                outputStream,
                new JpegEncoder
                {
                    Quality = 75
                });

            outputStream.Position = 0;

            return outputStream;
        }
    }
}
