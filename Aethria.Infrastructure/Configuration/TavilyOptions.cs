namespace Aethria.Infrastructure.Configuration;

internal class TavilyOptions
{
    public const string SectionName = "Tavily";

    public required string ApiKey { get; set; }
}
