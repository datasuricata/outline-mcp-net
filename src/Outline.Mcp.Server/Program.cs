using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outline.Mcp.Shared.Api;

var baseUrl = Environment.GetEnvironmentVariable("OUTLINE_BASE_URL") 
    ?? throw new InvalidOperationException("OUTLINE_BASE_URL environment variable is required");

var apiKey = Environment.GetEnvironmentVariable("OUTLINE_API_KEY") 
    ?? throw new InvalidOperationException("OUTLINE_API_KEY environment variable is required");

// Auto-detect transport mode based on environment variable
var ssePort = Environment.GetEnvironmentVariable("MCP_SSE_PORT");
var useSse = !string.IsNullOrEmpty(ssePort);

if (useSse)
{
    // SSE Mode: Use ASP.NET Core WebApplication
    Console.Error.WriteLine($"[INFO] Starting MCP Server in SSE mode on port {ssePort}");
    
    var port = int.Parse(ssePort!);
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Logging.AddConsole();
    
    // Set ASP.NET Core to listen on the specified port
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");
    
    builder.Services.AddSingleton<IOutlineApiClient>(sp => 
        new OutlineApiClient(baseUrl, apiKey));
    
    builder.Services
        .AddMcpServer()
        .WithHttpTransport()
        .WithToolsFromAssembly()
        .WithPromptsFromAssembly();
    
    var app = builder.Build();
    
    app.MapMcp();
    
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("MCP Server running in SSE mode on port {Port}", port);
    
    await app.RunAsync();
}
else
{
    // Stdio Mode: Use Host (console app)
    Console.Error.WriteLine("[INFO] Starting MCP Server in stdio mode");
    
    var builder = Host.CreateApplicationBuilder(args);
    
    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });
    
    builder.Services.AddSingleton<IOutlineApiClient>(sp => 
        new OutlineApiClient(baseUrl, apiKey));
    
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly()
        .WithPromptsFromAssembly();
    
    var app = builder.Build();
    
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("MCP Server running in stdio mode");
    
    await app.RunAsync();
}
