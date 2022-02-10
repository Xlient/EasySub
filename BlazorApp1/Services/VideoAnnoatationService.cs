using Google.Cloud.VideoIntelligence.V1;
using Google.LongRunning;
using Google.Protobuf.Collections;

namespace BlazorApp1.Services
{
    public class VideoAnnoatationService
    {
        private readonly ILogger<VideoAnnoatationService> _logger;
        private VideoIntelligenceServiceClient _client;
        private IWebHostEnvironment _webHostEnvironment;
        private TranslationService _translationService;
        CloudUploadService _uploadService;
        public int ProgressPercent { get; set; }

        /// <summary>
        /// Path to WebVTT file
        /// </summary>
        public string WebVTTFile { get; private set; }

        // logger will be passed in through Dependency injection
        public VideoAnnoatationService(ILogger<VideoAnnoatationService> logger, IWebHostEnvironment webHostEnvironment, TranslationService translationService, CloudUploadService uploadService)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _translationService = translationService;
            _uploadService = uploadService;
            _client = VideoIntelligenceServiceClient.Create();
        }
        public async Task SubVideo(string linkToVideo, string TargetLanguage = "en")
        {
            try
            {
                RepeatedField<TextAnnotation> textAnnoatations = await AnnotateVideo(linkToVideo, TargetLanguage);
                string path = ToWebVTT(textAnnoatations, TargetLanguage);
                await _uploadService.UploadFile(path, CloudUploadService.FileType.SUBTITLE);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} \n \n {ex.StackTrace}");
                throw new Exception("Something went wrong while subbing your video, Please Try again");
            }

        }

        /// <summary>
        /// Extracts text from video stored in google cloud
        /// </summary>
        /// <param name="linkToVideo"></param>
        /// <param name="languageCode"></param>
        /// <returns> Text Annoations from the video
        /// </returns>
        private async Task<RepeatedField<TextAnnotation>> AnnotateVideo(string linkToVideo, string languageCode) // out config)
        {

            TextDetectionConfig config = new TextDetectionConfig();
            config.LanguageHints.Add(languageCode);

            Operation<AnnotateVideoResponse, AnnotateVideoProgress> operation = await _client.AnnotateVideoAsync(
                    linkToVideo,
                 new[] { Feature.TextDetection });

            Operation<AnnotateVideoResponse, AnnotateVideoProgress> resultOperation = await operation.PollUntilCompletedAsync();
            VideoAnnotationResults result = resultOperation.Result.AnnotationResults[0];

            return result.TextAnnotations;
        }
        /// <summary>
        /// Takes Text Annotations and formats them into WEBVTT formated file
        /// </summary>
        /// <param name="textAnnotations"></param>
        /// <returns> path to the file</returns>
        private string ToWebVTT(RepeatedField<TextAnnotation> textAnnotations, string language)
        {
            //REPLACE WITH NAME with videoName.vtt  - videoName is the filename we pass in ...
            string randomFileName = Path.GetRandomFileName().Replace(".", "-");
            string pathToSubs = Path.Combine(_webHostEnvironment.WebRootPath, "subtitles", $"{randomFileName}.vtt");
            using var file = File.Create(pathToSubs);
            using (var subFile = new StreamWriter(file, encoding: System.Text.Encoding.UTF8))
            {
                int lineCount = 1;
                subFile.WriteLine("WEBVTT");
                subFile.WriteLine();
                subFile.WriteLine();

                // ordering segments by time they appear
                List<TextAnnotation> orderedTextAnnotations = textAnnotations.OrderBy<TextAnnotation, TimeSpan>(x =>
                {
                    return x.Segments[0].Segment.StartTimeOffset.ToTimeSpan();


                }).ToList();


                foreach (TextAnnotation annotation in orderedTextAnnotations)
                {
                    TimeSpan startTime = annotation.Segments[0].Segment.StartTimeOffset.ToTimeSpan();
                    TimeSpan endTime = annotation.Segments[0].Segment.EndTimeOffset.ToTimeSpan();


                    subFile.WriteLine(lineCount);
                    subFile.WriteLine($"0{startTime.Hours}:0{startTime.Minutes}:0{startTime.Seconds}.{startTime.Milliseconds} --> 0{ endTime.Hours}:0{ endTime.Minutes}:0{ endTime.Seconds}.{ endTime.Milliseconds}");
                    //TODO create translation Method  then call it here
                    string translatedText = _translationService.TranslateLanguage(annotation.Text, language);
                    subFile.WriteLine($" - {translatedText}");
                    subFile.WriteLine();

                    lineCount++;
                }

            }
            return pathToSubs;
        }
    }
}
