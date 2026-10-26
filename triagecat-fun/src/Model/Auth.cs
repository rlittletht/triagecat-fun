using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TCore.Logging;
using System.Collections.Immutable;
using tt.Types.Wire;
using tt.Types.Model;
using tt.Types.Model.Users;

namespace tt.Model;

public class Auth
{
    public static SymmetricSecurityKey? Key { get; set; }
    public static string Issuer { get; set; } = "";
    public static string Audience { get; set; } = "";

    static LoginResult BuildLoginResult(IReadOnlyCollection<Claim> claims, AuthUserEx authUserEx)
    {
        return new LoginResult()
        {
            email = authUserEx.Email,
            displayName = authUserEx.DisplayName,
            userId = authUserEx.UserId.ToString(),
        };
    }

    /*----------------------------------------------------------------------------
        %%Function: BuildLoginResultFromAuthAndClaimsList
        %%Qualified: tt.Model.Auth.BuildLoginResultFromAuthAndClaimsList

        given a list of user + claims, build the login result.
    ----------------------------------------------------------------------------*/
    static LoginResult BuildLoginResultFromAuthAndClaimsList(List<AuthUserEx> authAndClaims, out string? jwt, out DateTime? expires)
    {
        jwt = null;
        expires = null;

        AuthUserEx firstAuthUserEx = authAndClaims[0];

        List<Claim> returnClaims = new();

        returnClaims.Add(new Claim(ClaimTypes.Name, firstAuthUserEx.DisplayName));
        returnClaims.Add(new Claim(ClaimTypes.Email, firstAuthUserEx.Email));
        returnClaims.Add(new Claim(ClaimTypes.NameIdentifier, firstAuthUserEx.UserId.ToString()));

        expires = DateTime.UtcNow.AddDays(14);

        // now add the rest of the claims 
        JwtSecurityTokenHandler handler = new();
        SecurityTokenDescriptor tokenDescriptor =
            new()
            {
                Subject = new ClaimsIdentity(returnClaims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    Auth.Key,
                    SecurityAlgorithms.HmacSha256Signature),
                Audience = Auth.Audience,
                Issuer = Auth.Issuer
            };

        SecurityToken token = handler.CreateToken(tokenDescriptor);
        jwt = handler.WriteToken(token);

        return BuildLoginResult(returnClaims, firstAuthUserEx);
    }

    /*----------------------------------------------------------------------------
        %%Function: Login
        %%Qualified: tr.Model.Auth.Login
    ----------------------------------------------------------------------------*/
    public static LoginResult Login(CorrelationID crid, ILogger? logger, LoginParams? login, out string? jwt, out DateTime? expires)
    {
        if (login == null)
            throw new TtExceptionInvalidArgument();

        List<AuthUserEx> authAndClaims;

        authAndClaims = Session.RexDatabase.GetAuthAndClaimsForEmail(crid, login.Email);

        AuthUserEx firstAuthUserEx = authAndClaims[0];

        // now that we have the hashed password from the database, let's validate
        if (!BCrypt.Net.BCrypt.Verify(login.Password, firstAuthUserEx.Password))
            throw new TtExceptionAuthenticationFailure();

        // ok, the password is valid, but we don't know what group yet. let's try
        // to build the list of claims

        return BuildLoginResultFromAuthAndClaimsList(authAndClaims, out jwt, out expires);
    }

    /*----------------------------------------------------------------------------
        %%Function: ChangePassword
        %%Qualified: tr.Model.Auth.ChangePassword
    ----------------------------------------------------------------------------*/
    public static string ChangePassword(CorrelationID crid, ILogger? logger, string email, string? oldPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(email))
            throw new TtExceptionInvalidArgument("email is required");

        if (string.IsNullOrEmpty(newPassword))
            throw new TtExceptionInvalidArgument("newPassword is required");

        if (!string.IsNullOrEmpty(oldPassword))
        {
            List<AuthUserEx> authUsers = Session.RexDatabase.GetAuthAndClaimsForEmail(crid, email);

            if (authUsers.Count == 0)
                throw new TtExceptionUnauthorized("user not found");

            AuthUserEx firstAuthUserEx = authUsers[0];

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, firstAuthUserEx.Password))
                throw new TtExceptionUnauthorized("old password is incorrect");
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        Session.RexDatabase.UpdatePasswordForUser(crid, email, hashedPassword);
        return "password changed";
    }
}
