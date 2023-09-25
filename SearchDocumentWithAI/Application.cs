using System.Diagnostics;
using ChatGptNet;
using SearchDocumentWithAI.Services;
using SharpToken;

namespace SearchDocumentWithAI;

internal class Application
{
    private readonly IChatGptClient chatGptClient;
    private readonly IDocumentService documentService;

    private static readonly GptEncoding encoding = GptEncoding.GetEncoding("cl100k_base");

    public Application(IChatGptClient chatGptClient, IDocumentService documentService)
    {
        this.chatGptClient = chatGptClient;
        this.documentService = documentService;
    }

    public async Task ExecuteAsync(string? fileName)
    {
        string? message = null;
        var conversationId = Guid.NewGuid();

        while (string.IsNullOrWhiteSpace(fileName))
        {
            Console.Write("Provide the file name of the PDF document you want to ask about: ");
            fileName = Console.ReadLine();
        }

        using (var stream = File.OpenRead(fileName))
        {
            var content = await documentService.ExtractTextFromPdfAsync(stream);

            var tokenCount = encoding.Encode(content).Count;
            Debug.WriteLine($"Token count: {tokenCount}");

            content = await documentService.NormalizeAsync(content);

            tokenCount = encoding.Encode(content).Count;
            Debug.WriteLine($"Token count: {tokenCount}");

            var systemMessage = $$"""
                You are an assistant that knows the following information only:
                ---
                {{content}}
                ---
                You can use only the information above to answer questions. If you don't know the answer, reply suggesting to refine the question.
                """;

            await chatGptClient.SetupAsync(conversationId, systemMessage);
        }

        do
        {
            try
            {
                Console.Write("Ask me anything: ");
                message = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("I'm thinking...");

                    // Requests a streaming response.
                    var responseStream = chatGptClient.AskStreamAsync(conversationId, message);

                    await foreach (var response in responseStream)
                    {
                        Console.Write(response.GetContent());
                        await Task.Delay(80);
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Message);
                Console.WriteLine();

                Console.ResetColor();
            }
        } while (!string.IsNullOrWhiteSpace(message));
    }
}
