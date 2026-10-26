using Microsoft.Azure.Functions.Worker;
using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;

namespace tt.Extensions;

public static class HttpRequestDataExtensions
{
    public static string? Origin(this HttpRequestData request) => request.Headers.FirstOrDefault(h => h.Key.Equals("Origin", StringComparison.InvariantCultureIgnoreCase)).Value?.FirstOrDefault();
}
