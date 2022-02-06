
using Google.Cloud.Storage.V1;

namespace BlazorApp1.Services
{
    public class CloudUploadService
    {
        private readonly ILogger<CloudUploadService> _logger;
        private StorageClient _storageClient;
        private readonly string _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        private string objectName;
        public string Link { get; private set; }

        public CloudUploadService(ILogger<CloudUploadService> logger)
        {
            _logger = logger;
            _storageClient = StorageClient.Create();
        }

        public async void UploadFile(string path)
        {
            try
            {
                objectName = Path.GetFileName(path);
                using var fileStream = File.OpenRead(path);
                _storageClient.UploadObject(_bucketName, objectName, null, fileStream);
                var objectMeta = await _storageClient.GetObjectAsync(_bucketName, objectName);
                Link = objectMeta.MediaLink;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw new Exception("Something went wrong uploading the file to the cloud");
            }
        }

        public string GetObjectLink()
        {
            return $"gs://{_bucketName}/{objectName}";

        }
    }
}
