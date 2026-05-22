namespace Aethria.Application.UseCases.Chat.Contracts;

internal sealed class ChatAgentTimeoutScope : IDisposable
{
    private readonly CancellationTokenSource _maxDurationCts;
    private readonly CancellationTokenSource _firstTokenCts;
    private readonly CancellationTokenSource _agentCts;
    private bool _hasFirstToken;

    private ChatAgentTimeoutScope(
        CancellationTokenSource maxDurationCts,
        CancellationTokenSource firstTokenCts,
        CancellationTokenSource agentCts)
    {
        _maxDurationCts = maxDurationCts;
        _firstTokenCts = firstTokenCts;
        _agentCts = agentCts;
    }

    public CancellationToken Token => _agentCts.Token;

    public static ChatAgentTimeoutScope Start(
        TimeSpan firstTokenTimeout,
        TimeSpan maxStreamDuration,
        CancellationToken cancellationToken)
    {
        var maxDurationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        maxDurationCts.CancelAfter(maxStreamDuration);

        var firstTokenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        firstTokenCts.CancelAfter(firstTokenTimeout);

        var agentCts = CancellationTokenSource.CreateLinkedTokenSource(
            maxDurationCts.Token,
            firstTokenCts.Token);

        return new ChatAgentTimeoutScope(maxDurationCts, firstTokenCts, agentCts);
    }

    public void MarkFirstTokenReceived()
    {
        if (_hasFirstToken)
        {
            return;
        }

        _hasFirstToken = true;
        _firstTokenCts.CancelAfter(Timeout.InfiniteTimeSpan);
    }

    public void Dispose()
    {
        _agentCts.Dispose();
        _firstTokenCts.Dispose();
        _maxDurationCts.Dispose();
    }
}
