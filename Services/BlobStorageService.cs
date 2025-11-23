using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace IndoorBookingSystem.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            // Try Key Vault format first (BlobStorage-ConnectionString), then appsettings format
            var connectionString = configuration["BlobStorage-ConnectionString"] 
                ?? configuration["BlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException("BlobStorage ConnectionString missing");
            var containerName = configuration["BlobStorage:ContainerName"] 
                ?? throw new InvalidOperationException("BlobStorage ContainerName missing");

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // Create container without public access (private by default)
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = _containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            // Generate SAS URL valid for 10 years (or use a shorter duration if preferred)
            var sasUri = blobClient.GenerateSasUri(
                Azure.Storage.Sas.BlobSasPermissions.Read,
                DateTimeOffset.UtcNow.AddYears(10)
            );

            return sasUri.ToString();
        }

        public async Task<List<string>> UploadMultipleFilesAsync(IFormFileCollection files)
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var url = await UploadFileAsync(file);
                    urls.Add(url);
                }
            }
            return urls;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
