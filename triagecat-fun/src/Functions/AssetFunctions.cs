using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TCore.Logging;
using tt.Executors;
using tt.FunctionAuthentication;
using tt.Model;

namespace tt.Functions;

public class AssetFunctions
{
    [Function("GetAllAssets")]
    public async Task<HttpResponseData> GetAllAssets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "assets/GetAllAssets")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log,
        string? gameId)
    {
        return await Executor.ExecuteGet(
            req,
            context,
            new CorrelationID(),
            log,
            async _context => Assets.GetAllAssets(_context.CorrelationId, _context.Logger));
    }

    static async Task<int> AddAssetsAsync(
        ExecutorContext context, IEnumerable<Types.Wire.Assets.AddAsset>? assets)
    {
        List<Types.Model.Assets.AddAsset> modelAssets = Types.Model.Assets.AddAsset.CreateFromWireList(assets);
        
        return Assets.AddAssets(context.CorrelationId, context.Logger, modelAssets);
    }

    [Function("AddAssets")]
    public async Task<HttpResponseData> AddAssets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "assets/AddAssets")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecutePost<List<Types.Wire.Assets.AddAsset>, int>(
            req,
            context,
            new CorrelationID(),
            log,
            AddAssetsAsync);
    }

}