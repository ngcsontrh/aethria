namespace Aethria.Infrastructure.Configuration;

internal sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
}
