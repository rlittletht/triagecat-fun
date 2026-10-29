using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TCore.Logging;
using tt.Controllers;
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
        log?.Log(LogLevel.Error, "Starting GetAllAssets handling");
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
        if (assets == null)
            return 0;
        
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

    /*----------------------------------------------------------------------------
        %%Function: UpdateAssetAsync
        %%Qualified: tt.Functions.AssetFunctions.UpdateAssetAsync
    ----------------------------------------------------------------------------*/
    static async Task<int> UpdateAssetAsync(
        ExecutorContext context, Types.Wire.Assets.UpdateAssetParams? updateAssetParams)
    {
        tt.Types.Model.Assets.UpdateAssetParams updateAssetParamsModel = tt.Types.Model.Assets.UpdateAssetParams.CreateFromWire(updateAssetParams!);

        return Assets.UpdateAsset(context.CorrelationId, context.Logger, updateAssetParamsModel);
    }

    /*----------------------------------------------------------------------------
        %%Function: UpdateAsset
        %%Qualified: tt.Functions.AssetFunctions.UpdateAsset
    ----------------------------------------------------------------------------*/
    [Function("UpdateAsset")]
    public async Task<HttpResponseData> UpdateAsset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "assets/UpdateAsset")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecutePost<Types.Wire.Assets.UpdateAssetParams, int>(
            req,
            context,
            new CorrelationID(),
            log,
            UpdateAssetAsync);
    }

}