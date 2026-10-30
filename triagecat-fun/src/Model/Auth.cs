using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TCore.Logging;
using tt.Types.Model;
using tt.Types.Model.Auth;
using tt.Types.Wire;

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

    public static LoginResult GetAuthInfoFromUser(CorrelationID crid, ILogger? logger, Guid userId, ImmutableList<Claim> claims)
    {
        return BuildLoginResult(claims, Session.RexDatabase.GetAuthInfoFromUser(crid, userId));
    }

    /*----------------------------------------------------------------------------
        %%Function: BuildLoginResultFromAuthAndClaimsList
        %%Qualified: tt.Functions.Auth.BuildLoginResultFromAuthAndClaimsList

        This is a slightly simplified version from Traynrex.  There are no
        groups to worry about
    ----------------------------------------------------------------------------*/
    static LoginResult BuildLoginResultFromAuthAndClaimsList(List<AuthUserEx> authAndClaims, out string? jwt, out DateTime? expires)
    {
        jwt = null;
        expires = null;

        AuthUserEx firstAuthUserEx = authAndClaims[0];

        // at this point we know who the user is. let's build a token including
        // the name, email, and userid
        List<Claim> thisClaims = new();

        foreach (AuthUserEx authUserEx in authAndClaims)
        {
            if (authUserEx.Claim == null)
                continue;

            if (authUserEx.ClaimValue != null && authUserEx.ClaimValue.Contains(","))
            {
                // expand this into multiple claims
                string[] values = authUserEx.ClaimValue.Split(",", StringSplitOptions.TrimEntries);

                foreach (string value in values)
                {
                    thisClaims.Add(new Claim(authUserEx.Claim, value));
                }
            }
            else
            {
                thisClaims.Add(new Claim(authUserEx.Claim, authUserEx.ClaimValue ?? ""));
            }
        }

        // we only have one group. finish setting up the claims
        List<Claim> returnClaims = thisClaims;

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


    /*----------------------------------------------------------------------------
        %%Function: isValidEmail
        %%Qualified: tr.Controllers.AuthController.isValidEmail

        very simple match.  No whitespace allowed,
    ----------------------------------------------------------------------------*/
    static bool isValidEmail(string email)
    {
        Regex rex = new(@"^[^\s\.]+@([^\s\.]+\.)+[^\s\.]+$");

        return rex.IsMatch(email);
    }


    /*----------------------------------------------------------------------------
        %%Function: ValidateUserCreateParamsAndHashPassword
        %%Qualified: tr.Model.Users.ValidateUserCreateParamsAndHashPassword

        Validate the CreateParams data and hash the password

        THIS IS DESCRUCTIVE TO THE DATA IN THE CreateParams.  The password will
        be hashed and the email will be trimmed
    ----------------------------------------------------------------------------*/
    public static void ValidateUserCreateParamsAndHashPassword(CreateAuthUserParam data)
    {
        if (string.IsNullOrWhiteSpace(data.Email))
            throw new ArgumentNullException(nameof(data.Email));

        if (string.IsNullOrWhiteSpace(data.Password))
            throw new ArgumentNullException(nameof(data.Password));

        data.Email = data.Email.Trim();

        if (!isValidEmail(data.Email))
            throw new ArgumentException("email address invalid", nameof(data));

        // first, create the auth user
        data.Password = BCrypt.Net.BCrypt.HashPassword(data.Password);
    }


    /*----------------------------------------------------------------------------
        %%Function: CreateAuthUser
        %%Qualified: tt.Model.Auth.CreateAuthUser
    ----------------------------------------------------------------------------*/
    public static Types.Model.Auth.CreateAuthUserResult CreateAuthUser(
        CorrelationID crid, ILogger? logger,
        CreateAuthUserParam createParams)
    {
        if (string.IsNullOrEmpty(createParams?.Email))
            throw new TtExceptionInvalidArgument("email address null or empty");

        ValidateUserCreateParamsAndHashPassword(createParams);

        Guid guid = Session.RexDatabase.CreateAuthUserForEmailPassword(
            crid,
            createParams.Email,
            createParams.Password);

        return new Types.Model.Auth.CreateAuthUserResult
        {
            UserId = guid.ToString()
        };
    }
}
