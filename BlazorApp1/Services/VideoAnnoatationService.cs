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
        /// <summary>
        /// Path to WebVTT file
        /// </summary>
        public string WebVTTFile { get; private set; }

        // logger will be passed in through Dependency injection
        public VideoAnnoatationService(ILogger<VideoAnnoatationService> logger, IWebHostEnvironment webHostEnvironment, TranslationService translationService)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _translationService = translationService;
            _client = VideoIntelligenceServiceClient.Create();
        }
        public void SubVideo(string linkToVideo, string TargetLanguage = "en-EN")
        {
            try
            {
                RepeatedField<TextAnnotation> textAnnoatations = AnnotateVideo(linkToVideo, TargetLanguage);
                string path = ToWebVTT(textAnnoatations, TargetLanguage);
                WebVTTFile = path;
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
        private RepeatedField<TextAnnotation> AnnotateVideo(string linkToVideo, string languageCode)
        {
            TextDetectionConfig config = new TextDetectionConfig();
            config.LanguageHints.Add(languageCode);

            Operation<AnnotateVideoResponse, AnnotateVideoProgress> operation = _client.AnnotateVideo(
                    linkToVideo,
                 new[] { Feature.TextDetection });

            Operation<AnnotateVideoResponse, AnnotateVideoProgress> resultOperation = operation.PollUntilCompleted();
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
            string pathToSubs = Path.Combine(@"/BlazorApp1/Server/bin/Release/net6.0/publish/wwwroot/static", "subs.vtt");
            using var file = File.Create(pathToSubs);
            using (var subFile = new StreamWriter(file, encoding: System.Text.Encoding.UTF8))
            {
                int lineCount = 1;
                string startTimeStamp = "00:00:00";
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
