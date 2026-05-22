namespace Aethria.Application.ErrorTypes;

public class NotFoundError : Error
{
    public string EntityName { get; }
    public string EntityIdentifier { get; }

    public NotFoundError(string entityName, string entityIdentifier)
        : base($"{entityName} with identifier '{entityIdentifier}' was not found.")
    {
        EntityName = entityName;
        EntityIdentifier = entityIdentifier;
        Metadata.Add("EntityName", entityName);
        Metadata.Add("EntityIdentifier", entityIdentifier);
    }

    public NotFoundError(string message)
        : base(message)
    {
        EntityName = string.Empty;
        EntityIdentifier = string.Empty;
    }

    public static NotFoundError For<TEntity>(object identifier)
    {
        return new NotFoundError(typeof(TEntity).Name, identifier.ToString()!);
    }
}
