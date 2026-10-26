using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using tt.Types.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace tt.Controllers;

public static class Claims
{
    public static string GetFirstMatchingClaimValue(IEnumerable<Claim>? claims, string claimReq)
    {
        if (claims != null)
        {
            foreach (Claim claim in claims)
            {
                if (claim.Type == claimReq)
                    return claim.Value;
            }
        }

        return string.Empty;
    }

    public static Guid? GetUserIdFromClaims(IEnumerable<Claim>? claims)
    {
        string? userId = Claims.GetFirstMatchingClaimValue(claims, ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out Guid parsedId))
            return null;

        return parsedId;
    }

    public static string? GetEmailFromClaims(IEnumerable<Claim>? claims)
    {
        return Claims.GetFirstMatchingClaimValue(claims, ClaimTypes.Email);
    }

    public static string[] GetMatchingClaimValues(IEnumerable<Claim>? claims, string claimReq)
    {
        List<string> values = new List<string>();

        if (claims != null)
        {
            foreach (Claim claim in claims)
            {
                if (claim.Type == claimReq)
                    values.Add(claim.Value);
            }
        }

        return values.ToArray();
    }

    public static bool DoesClaimContain(IEnumerable<Claim>? claims, string claimReq, string check)
    {
        if (claims != null)
        {
            foreach (Claim claim in claims)
            {
                if (claim.Type == claimReq)
                {
                    if (claim.Value == check)
                        return true;
                }
            }
        }

        return false;
    }

    public static Guid? ParseGuidOrNull(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        if (!Guid.TryParse(s, out Guid parsedGuid))
            throw new TtExceptionInvalidArgument($"invalid guid: {s}");

        return parsedGuid;
    }
}
