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

public class AssetTransformationFunctions
{
    static async Task<int> SetAssetTransformationAsync(
        ExecutorContext context,
        Types.Wire.AssetTransformations.SetAssetTransformationParam? setParams)
    {
        Types.Model.AssetTransformations.SetAssetTransformationParam modelParams =
            Types.Model.AssetTransformations.SetAssetTransformationParam.CreateFromWire(setParams!);

        return AssetTransformations.SetAssetTransformation(context.CorrelationId, context.Logger, modelParams);
    }

    [Authorize(
        RequiredClaims = [TtClaimTypes.AssetAccess, ClaimTypes.NameIdentifier],
        RequiredClaimValues = [TtClaimTypes.Access_Write])]
    [Function("SetAssetTransformation")]
    public async Task<HttpResponseData> SetAssetTransformation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "assets/SetAssetTransformation")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecutePost<Types.Wire.AssetTransformations.SetAssetTransformationParam, int>(
            req,
            context,
            new CorrelationID(),
            log,
            SetAssetTransformationAsync);
    }
}
