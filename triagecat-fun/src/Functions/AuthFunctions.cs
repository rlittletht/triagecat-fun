using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TCore.Logging;
using tt.Controllers;
using tt.Executors;
using tt.FunctionAuthentication;
using tt.Model;
using tt.Types.Model;
using tt.Types.Model.Auth;
using tt.Types.Wire;
using tt.Types.Wire.Auth;

namespace tt.Functions;

public class AuthFunctions
{
    [Authorize(RequiredClaims = [ClaimTypes.NameIdentifier])]
    [Function("GetAuthInfo")]
    public async Task<HttpResponseData> GetAuthInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "auth/GetAuthInfo")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecuteGet<LoginResult>(
            req,
            context,
            new CorrelationID(),
            log,
            async _context => Auth.GetAuthInfoFromUser(
                _context.CorrelationId,
                _context.Logger,
                _context.UserId!.Value,
                _context.Claims));
    }

    /*----------------------------------------------------------------------------
        %%Function: Logout
        %%Qualified: tr.Functions.AuthFunctions.Logout

        Clear the jwt token so we are no longer logged in...
    ----------------------------------------------------------------------------*/
    [Function("Logout")]
    public async Task<HttpResponseData> Logout(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "auth/Logout")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        return await Executor.ExecuteGet<string>(
            req,
            context,
            new CorrelationID(),
            log,
            async _ => "Logged out",
            async (response, _) => { response.Headers.Add("Set-Cookie", "jwt=;path=/;secure;samesite=none;httponly;expires=01 Jan 1970 00:00:00 GMT"); });
    }

    /*----------------------------------------------------------------------------
        %%Function: SetJwtCookieOnSuccessfulLogin
        %%Qualified: tr.Functions.AuthFunctions.SetJwtCookieOnSuccessfulLogin
    ----------------------------------------------------------------------------*/
    public static void SetJwtCookieOnSuccessfulLogin(HttpResponseData response, LoginResult result, string? jwt, DateTime? expires)
    {
        // if there's no token, then don't set the cookie. (this isn't a delete)
        if (jwt == null || expires == null)
            return;

        response.Headers.Add("Set-Cookie", $"jwt={jwt};path=/;secure;samesite=none;httponly;expires={expires:R}");
    }

    
    /*----------------------------------------------------------------------------
        %%Function: Login
        %%Qualified: tr.Functions.AuthFunctions.Login

        we have to handle session cookies so we have to control the headers
        we return
    ----------------------------------------------------------------------------*/
    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/Login")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        string? jwt = null;
        DateTime? expires = null;

        return await Executor.ExecutePost<LoginParams, LoginResult>(
            req,
            context,
            new CorrelationID(),
            log,
            async (_context, login) => Model.Auth.Login(_context.CorrelationId, _context.Logger, login, out jwt, out expires),
            async (response, result) => SetJwtCookieOnSuccessfulLogin(response, result, jwt, expires));
    }
    
    /*----------------------------------------------------------------------------
        %%Function: CreateUserAsync
        %%Qualified: tr.Functions.UserFunctions.CreateUserAsync
    ----------------------------------------------------------------------------*/
    static async Task<Types.Wire.Auth.CreateAuthUserResult> CreateUserAsync(
        ExecutorContext context, Types.Wire.Auth.CreateAuthUserParam? createParams)
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
        
        return await Executor.ExecutePost<Types.Wire.Auth.CreateAuthUserParam, Types.Wire.Auth.CreateAuthUserResult>(
            req,
            context,
            new CorrelationID(),
            log,
            CreateUserAsync);
    }
}