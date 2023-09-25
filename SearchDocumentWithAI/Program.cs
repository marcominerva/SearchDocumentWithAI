using ChatGptNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SearchDocumentWithAI;
using SearchDocumentWithAI.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.local.json", optional: true);
    })
    .ConfigureServices(ConfigureServices)
    .Build();

var application = host.Services.GetRequiredService<Application>();
await application.ExecuteAsync(args.ElementAtOrDefault(0));

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddSingleton<Application>();
    services.AddSingleton<IDocumentService, DocumentService>();

    // Adds ChatGPT service using settings from IConfiguration.
    services.AddChatGpt(context.Configuration);
}
