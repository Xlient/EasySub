
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;

namespace BlazorApp1.Services
{
    public class TranslationService
    {
        private TranslationServiceClient _client;
        private string projectId = "subeasy-340222";
        public TranslationService()
        {
            _client = TranslationServiceClient.Create();
        }

        public string TranslateLanguage(string text, string language)
        {
            TranslateTextRequest request = new TranslateTextRequest
            {
                Contents = { text },
                TargetLanguageCode = language,
                Parent = new ProjectName(projectId).ToString()
            };
            TranslateTextResponse response = _client.TranslateText(request);
            Translation translation = response.Translations[0];
            return translation.TranslatedText;
        }

    }
}
