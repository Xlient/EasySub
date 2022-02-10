
using Google.Cloud.Storage.V1;

namespace BlazorApp1.Services
{
    public class CloudUploadService
    {
        private readonly ILogger<CloudUploadService> _logger;
        private StorageClient _storageClient;
        private readonly string _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        private readonly string _subsBucketName = Environment.GetEnvironmentVariable("SUBS_BUCKET_NAME");
        private string _objectName;

        public enum FileType
        {
            VIDEO,
            SUBTITLE
        };
        public string SubtitleLink { get; private set; }
        public string VideoLink { get; private set; }

        public CloudUploadService(ILogger<CloudUploadService> logger)
        {
            _logger = logger;
            _storageClient = StorageClient.Create();
        }

        public async Task UploadFile(string path, FileType fileType = FileType.VIDEO)
        {
            try
            {
                if (fileType == FileType.SUBTITLE)
                {
                    string objName = Path.GetFileName(path);
                    using var fileStream = File.OpenRead(path);
                    var uploadedObject = await _storageClient.UploadObjectAsync(_subsBucketName, objName, null, fileStream);
                    SubtitleLink = uploadedObject.SelfLink;
                }
                else
                {

                    _objectName = Path.GetFileName(path);
                    using var fileStream = File.OpenRead(path);
                    var uploadedObject = await _storageClient.UploadObjectAsync(_bucketName, _objectName, null, fileStream);
                    VideoLink = uploadedObject.MediaLink;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} \n \n {ex.StackTrace}");
                throw new Exception("Something went wrong uploading the file to the cloud");
            }
        }

        public string GetObjectLink()
        {
            return $"gs://{_bucketName}/{_objectName}";

        }
    }
}
