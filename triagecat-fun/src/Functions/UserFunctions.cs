using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TCore.Logging;
using tt.Executors;
using tt.FunctionAuthentication;
using UserPreferences = tt.Types.Wire.Users.UserPreferences;
using UpdateUserPreferencesParams = tt.Types.Wire.Users.UpdateUserPreferencesParams;

namespace tt.Functions;

public class UserFunctions
{
    [Authorize(RequiredClaims = [ClaimTypes.NameIdentifier])]
    [Function("GetUserPreferences")]
    public async Task<HttpResponseData> GetUserPreferences(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "users/GetUserPreferences")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecuteGet<UserPreferences>(
            req,
            context,
            new CorrelationID(),
            log,
            async _context => tt.Model.Users.GetUserPreferences(
                _context.CorrelationId,
                _context.Logger,
                _context.UserId!.Value));
    }

    static async Task<int> UpdateUserPreferencesAsync(
        ExecutorContext context, UpdateUserPreferencesParams? updateParams)
    {
        tt.Types.Model.Users.UpdateUserPreferencesParams modelParams = tt.Types.Model.Users.UpdateUserPreferencesParams.CreateFromWire(updateParams!);

        return tt.Model.Users.UpdateUserPreferences(context.CorrelationId, context.Logger, context.UserId!.Value, modelParams);
    }

    [Authorize(RequiredClaims = [ClaimTypes.NameIdentifier])]
    [Function("UpdateUserPreferences")]
    public async Task<HttpResponseData> UpdateUserPreferences(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "users/UpdateUserPreferences")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecutePost<UpdateUserPreferencesParams, int>(
            req,
            context,
            new CorrelationID(),
            log,
            UpdateUserPreferencesAsync);
    }
}
