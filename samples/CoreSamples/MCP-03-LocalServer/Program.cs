using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

// Local MCP Server with demo tools for workshop use.
// This server runs as a subprocess started by MCP-03-LocalClient.

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<DemoTools>();

await builder.Build().RunAsync();

[McpServerToolType]
public class DemoTools
{
    [McpServerTool, Description("Returns a random programming joke.")]
    public static string GetJoke()
    {
        string[] jokes =
        [
            "Why do Java developers wear glasses? Because they can't C#!",
            "What's a developer's favorite place? The foo bar!",
            "How many programmers does it take to change a light bulb? None — it's a hardware problem.",
            "A SQL query walks into a bar, walks up to two tables and asks: 'Can I join you?'"
        ];
        return jokes[Random.Shared.Next(jokes.Length)];
    }

    [McpServerTool, Description("Returns the current simulated weather for a given city.")]
    public static string GetWeather(
        [Description("The name of the city")] string city)
    {
        string[] conditions = ["sunny ☀️", "cloudy ☁️", "rainy 🌧️", "snowy ❄️", "windy 💨"];
        int temp = Random.Shared.Next(5, 32);
        string condition = conditions[Random.Shared.Next(conditions.Length)];
        return $"The weather in {city} is {condition} with {temp}°C.";
    }

    [McpServerTool, Description("Returns the current date and time.")]
    public static string GetCurrentTime() =>
        $"It is {DateTime.Now:HH:mm:ss} on {DateTime.Now:dddd, MMMM d, yyyy}.";
}
