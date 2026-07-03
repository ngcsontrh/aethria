using System.Diagnostics;

namespace Aethria.Infrastructure.AgentFramework;

public static class AgentFrameworkTelemetry
{
    public const string SourceName = "Aethria.AgentFramework";
    public const string WorkflowsSourceName = "Aethria.AgentFramework.Workflows";
    public const bool EnableSensitiveData = true;

    public static readonly ActivitySource WorkflowsActivitySource = new(WorkflowsSourceName);
}
