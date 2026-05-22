namespace Aethria.Application.UseCases.Resources.CreateResource;

internal sealed class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 200)
            .WithMessage("Name must be between 1 and 200 characters.");

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithMessage("Description must be less than 2000 characters.");

        RuleFor(command => command.FileStream)
            .NotNull()
            .WithMessage("File stream is required.")
            .Must(stream => stream is null || stream.CanSeek)
            .WithMessage("Stream does not support seeking.");

        RuleFor(command => command.FileSize)
            .GreaterThan(0)
            .WithMessage("File must not be empty.");

        RuleFor(command => command.FileName)
            .NotEmpty()
            .WithMessage("File name is required.");

        RuleFor(command => command.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.");

        RuleFor(command => command)
            .Must(command => HasSupportedFileType(command.ContentType, command.FileName))
            .WithMessage(command => $"Unsupported file type: {command.ContentType}. Supported types: PDF, TXT.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }

    private static bool HasSupportedFileType(string contentType, string fileName)
    {
        if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var normalizedContentType = contentType.ToLowerInvariant();
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        return normalizedContentType == "application/pdf" ||
            fileExtension == ".pdf" ||
            normalizedContentType == "text/plain" ||
            fileExtension == ".txt";
    }
}
