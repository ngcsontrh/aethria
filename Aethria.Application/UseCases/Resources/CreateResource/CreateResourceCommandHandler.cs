using Aethria.Application.Abstractions.Chunking;
using Aethria.Application.Abstractions.Embedding;
using Aethria.Application.Abstractions.Storage;
using Aethria.Application.UseCases.Resources.Events;
using Aethria.Application.Utils;

namespace Aethria.Application.UseCases.Resources.CreateResource;

public sealed class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, ValueTask<Result<Guid>>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IMediator _mediator;
    private readonly ITextChunkingService _textChunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceChunkVectorStore _resourceChunkVectorStore;
    private readonly IUnitOfWork _unitOfWork;

    public CreateResourceCommandHandler(
        IFileStorageService fileStorageService,
        IMediator mediator,
        ITextChunkingService textChunkingService,
        IEmbeddingService embeddingService,
        IResourceRepository resourceRepository,
        IResourceChunkVectorStore resourceChunkVectorStore,
        IUnitOfWork unitOfWork)
    {
        _fileStorageService = fileStorageService;
        _mediator = mediator;
        _textChunkingService = textChunkingService;
        _embeddingService = embeddingService;
        _resourceRepository = resourceRepository;
        _resourceChunkVectorStore = resourceChunkVectorStore;
        _unitOfWork = unitOfWork;
    }

    private const string ContainerName = "resources";

    public async ValueTask<Result<Guid>> Handle(CreateResourceCommand command, CancellationToken cancellationToken)
    {
        var trimmedName = TextSanitizationUtils.RemoveNullCharacters(command.Name).Trim();
        var description = TextSanitizationUtils.RemoveNullCharactersOrNull(command.Description);

        var contentType = command.ContentType.ToLowerInvariant();
        var fileExtension = Path.GetExtension(command.FileName).ToLowerInvariant();
        var isPdf = contentType == "application/pdf" || fileExtension == ".pdf";
        var isTxt = contentType == "text/plain" || fileExtension == ".txt";

        command.FileStream.Position = 0;

        string extractedText;
        if (isPdf)
        {
            extractedText = PdfParsingUtils.ExtractTextFromPdf(command.FileStream);
        }
        else
        {
            extractedText = TxtParsingUtils.ExtractTextFromTxt(command.FileStream);
        }

        extractedText = TextSanitizationUtils.RemoveNullCharacters(extractedText);
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Result.Fail(new ValidationError("File does not contain readable text."));
        }

        var chunks = await _textChunkingService.ChunkTextAsync(
            extractedText,
            new ChunkingOptions
            {
                MaxTokensPerChunk = 1_200,
                OverlapTokens = 200,
                MaxTokensPerLine = 600
            },
            cancellationToken);
        if (chunks.Count == 0)
        {
            return Result.Fail(new ValidationError("File content could not be chunked for querying."));
        }

        var chunkTexts = chunks.Select(c => c.Content).ToList();
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunkTexts, cancellationToken);

        command.FileStream.Position = 0;
        var uniqueFileName = GenerateUniqueFileName(command.FileName);
        var fileUri = await _fileStorageService.UploadFileAsync(
            command.FileStream,
            ContainerName,
            uniqueFileName,
            command.ContentType,
            cancellationToken);

        var resourceId = Guid.CreateVersion7();
        var now = DateTimeOffset.UtcNow;
        var resource = new Resource
        {
            Id = resourceId,
            Name = trimmedName,
            Description = description,
            FileUri = fileUri,
            FileType = command.ContentType,
            FileSize = command.FileSize,
            UserId = command.UserId,
            Content = extractedText,
            CreatedAt = now,
            UpdatedAt = now
        };

        var resourceChunks = new List<ResourceChunkVectorInput>();
        for (var i = 0; i < chunks.Count; i++)
        {
            resourceChunks.Add(new ResourceChunkVectorInput(
                Id: Guid.CreateVersion7(),
                ResourceId: resource.Id,
                ChunkIndex: i,
                Content: chunks[i].Content,
                Embedding: embeddings[i]));
        }

        await _resourceRepository.AddAsync(resource, cancellationToken);
        await _resourceChunkVectorStore.UpsertAsync(resource.Id, resourceChunks, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var completedEvent = new ResourceCreatedEvent(resourceId, command.UserId);

        await _mediator.Publish(completedEvent, cancellationToken);

        return Result.Ok(resourceId);
    }

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid()}{extension}";
    }
}
