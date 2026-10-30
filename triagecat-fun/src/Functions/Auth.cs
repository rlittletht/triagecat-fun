using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TCore.Logging;
using tt.Executors;
using tt.Types.Wire.Auth;

namespace tt.Functions;

public class Auth
{

    /*----------------------------------------------------------------------------
        %%Function: CreateUserAsync
        %%Qualified: tr.Functions.UserFunctions.CreateUserAsync
    ----------------------------------------------------------------------------*/
    static async Task<CreateAuthUserResult> CreateUserAsync(
        ExecutorContext context, CreateAuthUserParam? createParams)
    {
        Types.Model.Auth.CreateAuthUserParam modelCreateParams = Types.Model.Auth.CreateAuthUserParam.CreateFromWire(createParams!);
        
        return tt.Model.Auth.CreateAuthUser(context.CorrelationId, context.Logger, modelCreateParams).Wire;
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateUserAndGroup
        %%Qualified: tr.Functions.UserFunctions.CreateUserAndGroup
    ----------------------------------------------------------------------------*/
    [Function("CreateAuthUser")]
    public async Task<HttpResponseData> CreateAuthUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/CreateAuthUser")]
        HttpRequestData req,
        FunctionContext context)
    {
        ILogger? log = context.GetLogger("TriageCat");
        
        return await Executor.ExecutePost<CreateAuthUserParam, CreateAuthUserResult>(
            req,
            context,
            new CorrelationID(),
            log,
            CreateUserAsync);
    }
}