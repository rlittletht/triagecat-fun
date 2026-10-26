using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Security.Claims;
using TCore.Logging;

namespace tt.Executors;

public class ExecutorContext
{
    public HttpRequestData Request { get; init; }
    public FunctionContext FunctionContext { get; init; }
    public CorrelationID CorrelationId { get; init; }
    public ILogger? Logger { get; init; }
    public ImmutableList<Claim> Claims { get; init; }

    public ExecutorContext(HttpRequestData request, FunctionContext functionContext, CorrelationID correlationId, ILogger? logger, ImmutableList<Claim>? claims)
    {
        Request = request;
        FunctionContext = functionContext;
        CorrelationId = correlationId;
        Logger = logger;
        Claims = claims ?? ImmutableList<Claim>.Empty;
    }
}
