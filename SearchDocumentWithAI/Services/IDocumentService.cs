namespace SearchDocumentWithAI.Services;

public interface IDocumentService
{
    Task<string> ExtractTextFromPdfAsync(Stream stream);

    Task<string> NormalizeAsync(string input);
}