namespace Aethria.Application.ErrorTypes;

public class ConflictError : Error
{
    public ConflictError(string message)
        : base(message)
    {
    }

    public static ConflictError WithMessage(string message)
    {
        return new ConflictError(message);
    }
}
