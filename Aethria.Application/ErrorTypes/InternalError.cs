namespace Aethria.Application.ErrorTypes;

public class InternalError : Error
{
    public InternalError(string message = "An unexpected error occurred.")
        : base(message)
    {
    }
}
