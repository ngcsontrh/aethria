namespace Aethria.Application.ErrorTypes;

public class ValidationError : Error
{
    public ValidationError(string message)
        : base(message)
    {
    }

    public static ValidationError WithMessage(string message)
    {
        return new ValidationError(message);
    }
}
