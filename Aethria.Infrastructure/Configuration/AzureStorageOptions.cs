namespace Aethria.Infrastructure.Configuration;

internal class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public required string ConnectionString { get; init; } = null!;
}
