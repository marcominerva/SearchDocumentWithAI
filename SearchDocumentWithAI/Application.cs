using ChatGptNet;
using SearchDocumentWithAI.Services;

namespace SearchDocumentWithAI;

internal class Application
{
    private readonly IChatGptClient chatGptClient;
    private readonly IDocumentService documentService;

    public Application(IChatGptClient chatGptClient, IDocumentService documentService)
    {
        this.chatGptClient = chatGptClient;
        this.documentService = documentService;
    }

    public async Task ExecuteAsync()
    {
        string? message = null;
        var conversationId = Guid.NewGuid();

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
