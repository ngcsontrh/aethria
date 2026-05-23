using Aethria.Application.UseCases.Chat.Contracts;
using DispatchR.Abstractions.Stream;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace Aethria.Application.UseCases.Chat.ResourceChat;

public sealed class ResourceChatCommandHandler : IStreamRequestHandler<ResourceChatCommand, Result<ChatStreamResponse>>
{
    private static readonly TimeSpan _firstTokenTimeout = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan _maxStreamDuration = TimeSpan.FromMinutes(5);
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceChunkVectorStore _resourceChunkVectorStore;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IChatAgent _resourceChatAgent;
    private readonly IUnitOfWork _unitOfWork;

    public ResourceChatCommandHandler(
        IResourceRepository resourceRepository,
        IResourceChunkVectorStore resourceChunkVectorStore,
        IChatSessionRepository chatSessionRepository,
        [FromKeyedServices("resource-chat")] IChatAgent resourceChatAgent,
        IUnitOfWork unitOfWork)
    {
        _resourceRepository = resourceRepository;
        _resourceChunkVectorStore = resourceChunkVectorStore;
        _chatSessionRepository = chatSessionRepository;
        _resourceChatAgent = resourceChatAgent;
        _unitOfWork = unitOfWork;
    }

    public async IAsyncEnumerable<Result<ChatStreamResponse>> Handle(
        ResourceChatCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var trimmedMessage = command.Message?.Trim() ?? string.Empty;
        var resourceExists = await _resourceRepository.ExistsByIdAndUserAsync(command.ResourceId, command.UserId, cancellationToken);
        if (!resourceExists)
        {
            yield return Result.Fail<ChatStreamResponse>(new NotFoundError("Resource", command.ResourceId.ToString()));
            yield break;
        }

        var hasChunks = await _resourceChunkVectorStore.ExistsByResourceIdAsync(command.ResourceId, cancellationToken);
        if (!hasChunks)
        {
            yield return Result.Fail<ChatStreamResponse>(new ValidationError("Resource is not ready for querying. Please wait for processing to complete."));
            yield break;
        }

        Guid sessionId;
        List<ChatMessage> sessionMessages;
        if (command.SessionId.HasValue)
        {
            var existingSession = await _chatSessionRepository.GetByIdAsync(command.SessionId.Value, cancellationToken);
            if (existingSession is null)
            {
                yield return Result.Fail<ChatStreamResponse>(new NotFoundError("ChatSession", command.SessionId.Value.ToString()));
                yield break;
            }

            if (existingSession.UserId != command.UserId ||
                existingSession.ResourceId != command.ResourceId ||
                existingSession.MentorId != null)
            {
                yield return Result.Fail<ChatStreamResponse>(new NotFoundError("ChatSession", command.SessionId.Value.ToString()));
                yield break;
            }

            sessionId = existingSession.Id;
            sessionMessages = [.. (await _chatSessionRepository.ListMessagesByChatSessionIdAsync(sessionId, cancellationToken))];
        }
        else
        {
            sessionId = Guid.NewGuid();
            sessionMessages = [];
        }

        var messages = sessionMessages
            .OrderBy(m => m.CreatedAt)
            .Select(m => (Role: m.Role.Value, Content: m.Content))
            .ToList();
        messages.Add((Role: ChatRole.User.Value, Content: trimmedMessage));

        yield return Result.Ok(new ChatStreamResponse(
            ChatStreamResponse.Statuses.Started,
            SessionId: sessionId,
            Message: "Chat stream started."));

        IReadOnlyList<(string Role, string Content)>? aiResponseMessages = null;
        string answer = string.Empty;

        var context = new ChatAgentContext(
            Messages: messages,
            ResourceId: command.ResourceId);

        using var timeoutScope = ChatAgentTimeoutScope.Start(
            _firstTokenTimeout,
            _maxStreamDuration,
            cancellationToken);
        await using var enumerator = _resourceChatAgent
            .RunStreamingAsync(context, timeoutScope.Token)
            .GetAsyncEnumerator();

        while (await enumerator.MoveNextAsync())
        {
            var update = enumerator.Current;
            if (!string.IsNullOrEmpty(update.Delta) || update.IsCompleted)
            {
                timeoutScope.MarkFirstTokenReceived();
            }

            if (!string.IsNullOrEmpty(update.Delta))
            {
                yield return Result.Ok(new ChatStreamResponse(
                    ChatStreamResponse.Statuses.Delta,
                    Delta: update.Delta));
            }

            if (update.IsCompleted)
            {
                answer = update.Answer ?? string.Empty;
                aiResponseMessages = update.Messages;
            }
        }

        if (aiResponseMessages is null)
        {
            throw new InvalidOperationException("Resource chat stream completed without response messages.");
        }

        await SaveChatSessionAndMessagesAsync(
            command.UserId,
            sessionId,
            trimmedMessage,
            aiResponseMessages,
            command.ResourceId,
            cancellationToken);

        yield return Result.Ok(new ChatStreamResponse(
            ChatStreamResponse.Statuses.Completed,
            Answer: answer,
            SessionId: sessionId,
            Message: "Chat stream completed."));
    }

    private async Task SaveChatSessionAndMessagesAsync(
        Guid userId,
        Guid sessionId,
        string userMessage,
        IReadOnlyList<(string Role, string Content)> aiResponseMessages,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var existingSession = await _chatSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        ChatSession session;
        if (existingSession is not null)
        {
            session = existingSession;
        }
        else
        {
            session = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                MentorId = null,
                ResourceId = resourceId,
                Name = userMessage.Length > 50 ? userMessage[..50] : userMessage,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await _chatSessionRepository.AddAsync(session, cancellationToken);
        }

        session.UpdatedAt = DateTimeOffset.UtcNow;
        var baseTimestamp = DateTimeOffset.UtcNow;
        var newDomainSessionMessages = new List<ChatMessage>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = ChatRole.User,
                Content = userMessage,
                CreatedAt = baseTimestamp,
                UpdatedAt = baseTimestamp
            }
        };

        var messageOffsetMilliseconds = 0;
        foreach (var msg in aiResponseMessages)
        {
            messageOffsetMilliseconds++;
            var messageTimestamp = baseTimestamp.AddMilliseconds(messageOffsetMilliseconds);
            newDomainSessionMessages.Add(new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = ChatRole.FromValue(msg.Role),
                Content = msg.Content,
                CreatedAt = messageTimestamp,
                UpdatedAt = messageTimestamp
            });
        }

        if (existingSession is not null)
        {
            await _chatSessionRepository.UpdateAsync(session, cancellationToken);
        }
        await _chatSessionRepository.AddMessagesAsync(newDomainSessionMessages, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
