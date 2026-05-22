namespace Aethria.Application.ErrorTypes;

public class TimeoutError : Error
{
    public TimeoutError(string message = "Request timed out. Please try again.")
        : base(message)
    {
    }
}
