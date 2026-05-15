using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

// MCP-03-LocalClient: connects to the local MCP demo server and uses Ollama to answer questions.
//
// Run order:
//   1. dotnet build ..\MCP-03-LocalServer\MCP-03-LocalServer.csproj
//   2. dotnet run   (from this project folder)

var serverExe = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory,
        "../../../../MCP-03-LocalServer/bin/Debug/net10.0/MCP-03-LocalServer.exe"));

Console.WriteLine($"Starting local MCP server: {Path.GetFileName(serverExe)}");

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "LocalDemoServer",
    Command = serverExe,
});

await using var mcpClient = await McpClient.CreateAsync(transport);

var tools = await mcpClient.ListToolsAsync();
Console.WriteLine("Available tools:");
foreach (var tool in tools)
    Console.WriteLine($"  - {tool.Name}: {tool.Description}");
Console.WriteLine();

IChatClient client = ((IChatClient)new OllamaApiClient(new Uri("http://localhost:11434/"), "qwen2.5:7b"))
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

var chatOptions = new ChatOptions { Tools = [.. tools] };

string[] queries =
[
    "Tell me a programming joke!",
    "What's the weather like in Zurich right now?",
    "What time and date is it today?"
];

foreach (var query in queries)
{
    Console.WriteLine($"User:  {query}");
    var response = await client.GetResponseAsync(query, chatOptions);
    Console.WriteLine($"Agent: {response.Text}");
    Console.WriteLine();
}
