using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TCore.Logging;
using tt.Executors;
using tt.FunctionAuthentication;
using tt.Model;

namespace tt.Functions;

public class AssetGroupFunctions
{
    [Authorize(RequiredClaims = [ClaimTypes.NameIdentifier])]
    [Function("GetAllAssetGroups")]
    public async Task<HttpResponseData> GetAllAssetGroups(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "assetgroups/GetAllAssetGroups")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecuteGet<IReadOnlyCollection<Types.Wire.AssetGroups.AssetGroup>>(
            req,
            context,
            new CorrelationID(),
            log,
            async _context => AssetGroups.GetAllAssetGroups(_context.CorrelationId, _context.Logger));
    }
}
