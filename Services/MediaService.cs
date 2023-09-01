using System;
using PEAS.Helpers;
using System.Configuration;
using Azure.Storage.Blobs;

namespace PEAS.Services
{
    public interface IMediaService
    {
        Uri UploadImage(byte[] imageData);
    }

    public class MediaService : IMediaService
    {
        private readonly int imageSizeLimit = 2_097_152; //2MB in bytes
        private readonly string imageSizeLimitDescription = "2MB"; //2MB
        private readonly string imageFileExtension = ".jpeg";
        private readonly string connectionStringSection = "BlobStorage";
        private readonly string imageContainer = "images";

        private readonly ILogger<MediaService> _logger;
        private readonly IConfiguration _configuration;

        public MediaService(ILogger<MediaService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Uri UploadImage(byte[] imageData)
        {
            try
            {
                if (imageData == null)
                {
                    throw new AppException(message: "No Data In Request: Please attach an image data to the request");
                }

                Stream stream = new MemoryStream(imageData);

                if (stream.Length > imageSizeLimit)
                {
                    throw new AppException(message: $"Image Size too large: Please upload an image that is less than {imageSizeLimitDescription}");
                }

                BlobServiceClient blobServiceClient = new(_configuration.GetConnectionString(connectionStringSection));
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(imageContainer);

                string fileName = $"{Guid.NewGuid()}{imageFileExtension}";
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                blobClient.Upload(stream);
                return blobClient.Uri;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }
    }
}