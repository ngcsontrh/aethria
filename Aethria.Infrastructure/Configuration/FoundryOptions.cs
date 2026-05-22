namespace Aethria.Infrastructure.Configuration;

internal class FoundryOptions
{
    public const string SectionName = "Foundry";

    public required string ProjectEndpoint { get; set; }
    public required string AzureOpenAIEndPoint { get; set; }
    public required string ApiKey { get; set; }
}
