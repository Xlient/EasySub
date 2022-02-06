
using Google.Cloud.Storage.V1;

namespace EasySub.Services
{
    public class CloudUploadService
    {
        private readonly ILogger<CloudUploadService> _logger;
        private StorageClient _storageClient;
        private readonly string _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        private string objectName;


        public CloudUploadService(ILogger<CloudUploadService> logger)
        {
            _logger = logger;
            _storageClient = StorageClient.Create();
        }

        public void UploadFile(string path)
        {
            try
            {
                string objectName = Path.GetFileName(path);
                using var fileStream = File.OpenRead(path);
                _storageClient.UploadObject(_bucketName, objectName, null, fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw new Exception("Something went wrong uploading the file to the cloud");
            }
        }

        public async Task<string> GetObjectLink()
        {
            var gObject = await _storageClient.GetObjectAsync(_bucketName, objectName);
            return gObject.SelfLink;
        }

    }
}
