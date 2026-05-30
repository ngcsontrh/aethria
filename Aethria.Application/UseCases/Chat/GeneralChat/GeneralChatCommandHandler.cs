using Aethria.Application.UseCases.Chat.Contracts;
using System.Runtime.CompilerServices;

namespace Aethria.Application.UseCases.Chat.GeneralChat;

public sealed class GeneralChatCommandHandler : IStreamRequestHandler<GeneralChatCommand, Result<ChatStreamResponse>>
{
    private static readonly TimeSpan _firstTokenTimeout = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan _maxStreamDuration = TimeSpan.FromMinutes(3);
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IChatAgent _generalChatAgent;
    private readonly IUnitOfWork _unitOfWork;

    public GeneralChatCommandHandler(
        IChatSessionRepository chatSessionRepository,
        [FromKeyedServices("general-chat")] IChatAgent generalChatAgent,
        IUnitOfWork unitOfWork)
    {
        _chatSessionRepository = chatSessionRepository;
        _generalChatAgent = generalChatAgent;
        _unitOfWork = unitOfWork;
    }

    public async IAsyncEnumerable<Result<ChatStreamResponse>> Handle(
        GeneralChatCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var trimmedMessage = command.Message?.Trim() ?? string.Empty;
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

            if (existingSession.UserId != command.UserId || existingSession.MentorId != null || existingSession.ResourceId != null)
            {
                yield return Result.Fail<ChatStreamResponse>(new NotFoundError("ChatSession", command.SessionId.Value.ToString()));
                yield break;
            }

            sessionId = existingSession.Id;
            sessionMessages = [.. (await _chatSessionRepository.ListMessagesByChatSessionIdAsync(command.SessionId.Value, cancellationToken))];
        }
        else
        {
            sessionId = Guid.CreateVersion7();
            sessionMessages = [];
        }

        var aiMessages = sessionMessages
            .OrderBy(m => m.CreatedAt)
            .Select(m => (Role: m.Role.Value, Content: m.Content))
            .ToList();
        aiMessages.Add((Role: ChatRole.User.Value, Content: trimmedMessage));

        yield return Result.Ok(new ChatStreamResponse(
            ChatStreamResponse.Statuses.Started,
            SessionId: sessionId,
            Message: "Chat stream started."));

        IReadOnlyList<(string Role, string Content)>? aiResponseMessages = null;
        string answer = string.Empty;

        var context = new ChatAgentContext(
            Messages: aiMessages,
            Tools: command.Tools);

        using var timeoutScope = ChatAgentTimeoutScope.Start(
            _firstTokenTimeout,
            _maxStreamDuration,
            cancellationToken);
        await using var enumerator = _generalChatAgent
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
            throw new InvalidOperationException("General chat stream completed without response messages.");
        }

        await SaveChatSessionAndMessagesAsync(
            command.UserId,
            sessionId,
            trimmedMessage,
            aiResponseMessages,
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
                ResourceId = null,
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
                Id = Guid.CreateVersion7(),
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
                Id = Guid.CreateVersion7(),
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
