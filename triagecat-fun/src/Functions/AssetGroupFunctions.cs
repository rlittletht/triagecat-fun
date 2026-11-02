using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TCore.Logging;
using tt.Controllers;
using tt.Executors;
using tt.FunctionAuthentication;
using tt.Model;
using tt.Types.Wire.AssetGroups;

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


    static async Task<AssetGroup> CreateAssetGroupAsync(
        ExecutorContext context, Types.Wire.AssetGroups.CreateAssetGroupParams? createParams)
    {
        tt.Types.Model.AssetGroups.CreateAssetGroupParams createParamsModel =
            tt.Types.Model.AssetGroups.CreateAssetGroupParams.CreateFromWire(createParams!);

        return AssetGroups.CreateAssetGroup(context.CorrelationId, context.Logger, createParamsModel);
    }

    [Authorize(
        RequiredClaims = [TtClaimTypes.AssetAccess, ClaimTypes.NameIdentifier],
        RequiredClaimValues = [TtClaimTypes.Access_Create])]
    [Function("CreateAssetGroup")]
    public async Task<HttpResponseData> CreateAssetGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "assetgroups/CreateAssetGroup")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecutePost<Types.Wire.AssetGroups.CreateAssetGroupParams, AssetGroup>(
            req,
            context,
            new CorrelationID(),
            log,
            CreateAssetGroupAsync);
    }
}