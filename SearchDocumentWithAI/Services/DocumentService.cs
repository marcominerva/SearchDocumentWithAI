using System.Text;
using System.Text.RegularExpressions;
using LanguageDetection;
using Microsoft.ML;
using SearchDocumentWithAI.Services;
using UglyToad.PdfPig;
using static Microsoft.ML.Transforms.Text.StopWordsRemovingEstimator;

namespace SearchDocumentWithAI;

public partial class DocumentService : IDocumentService
{
    [GeneratedRegex("[\\s]+")]
    private static partial Regex TrimTextRegex();

    private static LanguageDetector languageDetector;

    static DocumentService()
    {
        languageDetector = new LanguageDetector();
        languageDetector.AddAllLanguages();
    }

    public Task<string> NormalizeAsync(string input)
    {
        var detectedLanguage = languageDetector.Detect(input);
        var language = detectedLanguage switch
        {
            "fra" => Language.French,
            "due" => Language.German,
            "nld" => Language.Dutch,
            "dan" => Language.Danish,
            "swe" => Language.Swedish,
            "ita" => Language.Italian,
            "spa" => Language.Spanish,
            "por" => Language.Portuguese,
            "nor" => Language.Norwegian_Bokmal,
            "rus" => Language.Russian,
            "pol" => Language.Polish,
            "slk" => Language.Czech,
            "ara" => Language.Arabic,
            "jpn" => Language.Japanese,
            _ => Language.English
        };

        var mlContext = new MLContext();

        // Create an empty list as the dataset. The 'RemoveDefaultStopWords' API does not
        // require training data. The empty list is only needed to pass input schema to the pipeline.
        var emptySamples = new List<TextData>();

        // Convert sample list to an empty IDataView.
        var emptyDataView = mlContext.Data.LoadFromEnumerable(emptySamples);

        // A pipeline for normalizing text.
        var normalizationTextPipeline = mlContext.Transforms.Text.TokenizeIntoWords(
                    inputColumnName: nameof(TextData.Text),
                    outputColumnName: "Words")
                    .Append(mlContext.Transforms.Text.RemoveDefaultStopWords(
                        inputColumnName: "Words",
                        outputColumnName: nameof(TransformedTextData.SanitizedWords),
                        language: language));

        // Fit to data.
        var normalizationTextTransformer = normalizationTextPipeline.Fit(emptyDataView);

        // Create the prediction engine to get the normalized text from the input text/string.
        var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData, TransformedTextData>(normalizationTextTransformer);

        // Call the prediction API.
        var prediction = predictionEngine.Predict(new TextData(input));

        // Get the sanitized text.
        var content = TrimTextRegex().Replace(string.Join(' ', prediction.SanitizedWords), " ").Trim();
        return Task.FromResult(content);
    }

    internal record class TextData(string Text);

    internal record class TransformedTextData
    {
        public string[] SanitizedWords { get; init; } = Array.Empty<string>();
    }

    public Task<string> ExtractTextFromPdfAsync(Stream stream)
    {
        var content = new StringBuilder();
        using var document = PdfDocument.Open(stream);

        foreach (var page in document.GetPages())
        {
            content.Append(page.Text);
            content.Append(' ');
        }

        return Task.FromResult(content.ToString());
    }
}
