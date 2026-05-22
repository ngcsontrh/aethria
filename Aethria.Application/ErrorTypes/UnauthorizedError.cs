namespace Aethria.Application.ErrorTypes;

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message = "Authentication failed.")
        : base(message)
    {
    }
}
