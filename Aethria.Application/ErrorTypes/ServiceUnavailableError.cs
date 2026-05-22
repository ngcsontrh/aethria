namespace Aethria.Application.ErrorTypes;

public class ServiceUnavailableError : Error
{
    public ServiceUnavailableError(string message = "Service temporarily unavailable. Please try again later.")
        : base(message)
    {
    }
}
