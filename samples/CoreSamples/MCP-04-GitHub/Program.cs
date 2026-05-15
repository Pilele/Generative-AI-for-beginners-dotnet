using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using OllamaSharp;

// MCP-04-GitHub: Uses GitHub's MCP HTTP endpoint to answer questions about repos.
//
// Setup (one-time):
//   dotnet user-secrets set "GITHUB_PAT" "github_pat_your_token_here"
//
// The PAT needs: repo (read), issues (read)
// Create one at: https://github.com/settings/tokens

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var githubPat = config["GITHUB_PAT"]
    ?? throw new InvalidOperationException(
        "Missing 'GITHUB_PAT'. Run: dotnet user-secrets set \"GITHUB_PAT\" \"your_token\"");

// Connect to GitHub's MCP HTTP endpoint (no Node.js needed!)
Console.WriteLine("Connecting to GitHub MCP server...");
var transport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Name = "GitHub",
        Endpoint = new Uri("https://api.githubcopilot.com/mcp/"),
        AdditionalHeaders = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {githubPat}" }
        }
    });

await using var mcpClient = await McpClient.CreateAsync(transport);

var allTools = await mcpClient.ListToolsAsync();
Console.WriteLine($"Connected! Total tools available: {allTools.Count}");

// Only keep the tools we need — fewer tools = faster model response
string[] relevantToolNames = ["search_repositories", "list_issues", "list_pull_requests", "get_file_contents"];
var tools = allTools.Where(t => relevantToolNames.Contains(t.Name)).ToList();
Console.WriteLine($"Using {tools.Count} tools: {string.Join(", ", tools.Select(t => t.Name))}");
Console.WriteLine();

// Create Ollama chat client with MCP tool support
IChatClient client = ((IChatClient)new OllamaApiClient(new Uri("http://localhost:11434/"), "qwen2.5:7b"))
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

var chatOptions = new ChatOptions { Tools = [.. tools] };

// System message to enforce English and tool usage
var systemMessage = new ChatMessage(ChatRole.System,
    "You are a helpful assistant. " +
    "IMPORTANT: Always respond in English only, never in Chinese or any other language. " +
    "Always use the provided tools to fetch real data — never guess or make up answers.");

// Demo questions about GitHub
string[] queries =
[
    "Search for the repository 'microsoft/Generative-AI-for-beginners-dotnet' and tell me how many stars it has.",
    "What are the 3 most recent open issues in the repo owner='microsoft', repo='Generative-AI-for-beginners-dotnet'?",
    "What are the 3 most recent open pull requests in owner='microsoft', repo='Generative-AI-for-beginners-dotnet'?"
];

foreach (var query in queries)
{
    Console.WriteLine($"User:  {query}");
    var messages = new List<ChatMessage>
    {
        systemMessage,
        new ChatMessage(ChatRole.User, query)
    };
    var response = await client.GetResponseAsync(messages, chatOptions);
    Console.WriteLine($"Agent: {response.Text}");
    Console.WriteLine();
}
