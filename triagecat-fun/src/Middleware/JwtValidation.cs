using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Reflection;
using System.Security.Claims;
using tt.Controllers;
using NUnit.Framework.Internal;
using tt.FunctionAuthentication;
using tt.Model;
using Claims = tt.Controllers.Claims;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace tt.Middleware;

public class JwtValidation : IFunctionsWorkerMiddleware
{
    public static bool DoesFunctionRequireValidation(FunctionContext context)
    {
        List<AuthorizeAttribute> attributes =
            ReflectionHelp.GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(context);

        // no attributes, no auth requirement
        if (attributes.Count == 0)
            return false;

        foreach (AuthorizeAttribute attribute in attributes)
        {
            if (attribute.Value)
                return true;
        }

        return false;
    }

    public static List<(string, string)> ClaimsRequired(FunctionContext context)
    {
        List<(string, string)> claimsRequired = new();

        List<AuthorizeAttribute> attributes =
            ReflectionHelp.GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(context);

        // no attributes, no auth requirement
        if (attributes.Count == 0)
            return claimsRequired;

        foreach (AuthorizeAttribute attribute in attributes)
        {
            if (attribute.RequiredClaims != null)
            {
                for (int i = 0; i < attribute.RequiredClaims.Length; i++)
                {
                    string claim = attribute.RequiredClaims[i];
                    string value = attribute.RequiredClaimValues == null || i >= attribute.RequiredClaimValues.Length
                        ? TtClaimTypes.ClaimPresent
                        : attribute.RequiredClaimValues[i];

                    claimsRequired.Add((claim, value));
                }
            }
        }

        return claimsRequired;
    }

    public static string? GetJwtTokenFromHeaderOrCookies(HttpRequestData request)
    {
        // Extract the Authorization header
        string? authorizationHeader = request.Headers.FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
           .Value?.FirstOrDefault();

        if (!string.IsNullOrEmpty(authorizationHeader) && !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authorizationHeader.Substring("Bearer ".Length);

        // look for a jwt cookie
        foreach (IHttpCookie cookie in request.Cookies)
        {
            if (cookie.Name == "jwt")
                return cookie.Value;
        }

        return null;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (DoesFunctionRequireValidation(context))
        {
            HttpRequestData? request = await context.GetHttpRequestDataAsync();

            if (request == null)
            {
                // If not an HTTP request, proceed
                await next(context);
                return;
            }

            if (request.Method == "OPTIONS")
                throw new InvalidOperationException("should never get here with OPTIONS");

            ILogger? logger = context.GetLogger("JwtValidationMiddleware");

            string? token = GetJwtTokenFromHeaderOrCookies(request);
            if (token == null)
            {
                logger?.LogWarning("No valid Authorization header found.");
                DenyRequest(context, request, "Authorization header missing or incorrect.");

                // don't continue with middleware. we're done.
                return;
            }

            // Validate the JWT token
            ClaimsPrincipal? principal = ValidateJwtToken(token, out SecurityToken? validatedToken, logger);

            if (principal == null)
            {
                DenyRequest(context, request, "Invalid or expired token.");
                // don't continue with middleware. we're done.
                return;
            }

            context.Items["Claims"] = principal.Claims.ToImmutableList();
            // fallthrough to proceed to the next middleware or function

            List<(string, string)> claimsRequired = ClaimsRequired(context);

            foreach ((string, string) claimValue in claimsRequired)
            {
                if (!Claims.DoesClaimContain(principal.Claims, claimValue.Item1, claimValue.Item2))
                {
                    DenyRequest(context, request, $"Missing required claim: {claimValue.Item1}({claimValue.Item2})");
                    return;
                }
            }
        }

        await next(context);
    }

    private ClaimsPrincipal? ValidateJwtToken(string token, out SecurityToken? tokenOut, ILogger? logger)
    {
        tokenOut = null;

        try
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Auth.Issuer,
                    ValidAudience = Auth.Audience,
                    IssuerSigningKey = Auth.Key
                };

            // Validate the token
            return tokenHandler.ValidateToken(token, validationParameters, out tokenOut);
        }
        catch (SecurityTokenException ex)
        {
            logger?.LogError($"Token validation failed: {ex.Message}");
            return null;
        }
    }

    private void DenyRequest(FunctionContext context, HttpRequestData request, string reason)
    {
        // Return an Unauthorized response
        HttpResponseData response = request.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
        response.WriteString($"Unauthorized: {reason}");

        context.GetInvocationResult().Value = response;
    }
}
