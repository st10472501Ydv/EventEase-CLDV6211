using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _container;

        public BlobService(IConfiguration config)
        {
            var connString = config.GetConnectionString("BlobConnection");

            // Tell the client to use an API version compatible with Azurite 3.35.0
            var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2025_01_05);
            var client = new BlobServiceClient(connString, options);

            _container = client.GetBlobContainerClient("venue-images");
            _container.CreateIfNotExists();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blob = _container.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

            return fileName;
        }

        public async Task<(Stream? stream, string? contentType)> GetFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return (null, null);

            var blob = _container.GetBlobClient(fileName);
            if (!await blob.ExistsAsync())
                return (null, null);

            var download = await blob.DownloadAsync();
            return (download.Value.Content, download.Value.Details.ContentType);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var blob = _container.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }
    }
}