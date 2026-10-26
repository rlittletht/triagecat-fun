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
        returnClaims.Add(new Claim(TrClaimTypes.GroupId, firstAuthUserEx.GroupId.ToString()));

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

        authAndClaims = Session.RexDatabase.GetAuthAndClaimsForEmailAndGroup(crid, login.Email, groupId);

        AuthUserEx firstAuthUserEx = authAndClaims[0];

        // now that we have the hashed password from the database, let's validate
        if (!BCrypt.Net.BCrypt.Verify(login.Password, firstAuthUserEx.Password))
            throw new TtExceptionAuthenticationFailure();

        // ok, the password is valid, but we don't know what group yet. let's try
        // to build the list of claims

        return BuildLoginResultFromAuthAndClaimsList(authAndClaims, out jwt, out expires);
    }


    /*----------------------------------------------------------------------------
        %%Function: SwitchToGroup
        %%Qualified: tr.Model.Auth.SwitchToGroup
    ----------------------------------------------------------------------------*/
    public static LoginResult SwitchToGroup(
        CorrelationID crid, ILogger? logger, string email, Guid newGroupId, out string? jwt, out DateTime? expires)
    {
        List<AuthUserEx> authAndClaims = Session.RexDatabase.GetAuthAndClaimsForEmailAndGroup(crid, email, newGroupId);

        if (authAndClaims.Count == 0)
            throw new TtExceptionUnauthorized("user does not belong to the group");

        return BuildLoginResultFromAuthAndClaimsList(authAndClaims, out jwt, out expires);
    }

    /*----------------------------------------------------------------------------
        %%Function: GetAuthInfoFromUserAndGroup
        %%Qualified: tr.Model.Auth.GetAuthInfoFromUserAndGroup
    ----------------------------------------------------------------------------*/
    public static LoginResult GetAuthInfoFromUserAndGroup(CorrelationID crid, ILogger? logger, Guid userId, Guid groupId, ImmutableList<Claim> claims)
    {
        return BuildLoginResult(claims, Session.RexDatabase.GetAuthInfoFromUserAndGroup(crid, userId, groupId));
    }

    /*----------------------------------------------------------------------------
        %%Function: CleanupTestUserAndGroup
        %%Qualified: tr.Model.Auth.CleanupTestUserAndGroup
    ----------------------------------------------------------------------------*/
    public static void CleanupTestUserAndGroup(CorrelationID crid, ILogger? logger, Guid userId, IReadOnlyCollection<Guid> groupIds)
    {
        Session.RexDatabase.DeleteUserFromEverything(crid, userId);
        foreach (Guid groupId in groupIds)
        {
            Session.RexDatabase.DeleteGroup(crid, groupId);
        }
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateTestUserAndGroup
        %%Qualified: tr.Model.Auth.CreateTestUserAndGroup

        The new user will be associated with the rex root group (and have all
        claims)

        Two new groups will be created:
            * groupIdForOwnAndOther - user will have all claims for this group
            * groupIdForOwnOnly - user will have read claims for this group
    ----------------------------------------------------------------------------*/
    public static CreateTestResult CreateTestUserAndGroup(CorrelationID crid, ILogger? logger, UserProfile profile, out string? jwt, out DateTime? expires)
    {
        DateTime now = DateTime.UtcNow;
        Guid? groupIdForOwnAndOther = null;
        Guid? groupIdForOwnOnly = null;
        Guid? userId = null;

        try
        {
            groupIdForOwnAndOther = Session.RexDatabase.CreateGroup(crid, "Test Group for Own/Other", $"Created {now}");
            groupIdForOwnOnly = Session.RexDatabase.CreateGroup(crid, "Test Group for Own Only", $"Created {now}");

            CreateParams createParams =
                new()
                {
                    Email = $"test{groupIdForOwnAndOther.ToString()}@test.traynrex.com",
                    Password = Guid.NewGuid().ToString() // we don't ever want to know this
                };

            Users.ValidateUserCreateParamsAndHashPassword(createParams);

            CreateNewUserAndGroupResult createResult = Session.RexDatabase.CreateNewUserWithProfileInGroup(
                crid,
                createParams.Email,
                createParams.Password,
                profile,
                TrClaimTypes.s_RexGroupId,
                true /*canWritePeople*/,
                true /*canWriteGroups*/,
                false/*createOneTimeKey*/);

            userId = createResult.UserId;

            string claimsOwnOnly = string.Join(",", [TrClaimTypes.Access_WriteOwn, TrClaimTypes.Access_ReadOwn, TrClaimTypes.Access_ManageOwn]);
            string claimsOwnAndOther = string.Join(",", [TrClaimTypes.Access_WriteOwn, TrClaimTypes.Access_ReadOwn, TrClaimTypes.Access_WriteOther,
                TrClaimTypes.Access_ReadOther, TrClaimTypes.Access_ManageOwn]);


            CreateTestResult result = new CreateTestResult()
            {
                Email = createParams.Email,
                UserId = userId.Value,
                GroupIdForOwnAndOther = groupIdForOwnAndOther.Value,
                GroupIdForOwnOnly = groupIdForOwnOnly.Value
            };

            // join the new user to the new groups
            Session.RexDatabase.AddUserIdToGroupWithAccessClaims(crid, userId.Value, groupIdForOwnAndOther.Value, true, true);
            Session.RexDatabase.AddUserIdToGroupWithAccessClaims(crid, userId.Value, groupIdForOwnOnly.Value, true, true);

            // update the claims to exactly what we want
            Session.RexDatabase.SetOrUpdateClaimsForUser(crid, userId.Value, groupIdForOwnAndOther.Value, TrClaimTypes.GroupAccess, claimsOwnAndOther);
            Session.RexDatabase.SetOrUpdateClaimsForUser(crid, userId.Value, groupIdForOwnOnly.Value, TrClaimTypes.GroupAccess, claimsOwnOnly);

            // and lastly, login to the test account in the rex root group
            SwitchToGroup(crid, logger, result.Email, TrClaimTypes.s_RexGroupId, out jwt, out expires);

            return result;
        }
        catch (Exception)
        {
            // cleanup
            if (userId != null)
                Session.RexDatabase.DeleteUserFromEverything(crid, userId.Value);

            if (groupIdForOwnAndOther != null)
                Session.RexDatabase.DeleteGroup(crid, groupIdForOwnAndOther.Value);

            if (groupIdForOwnOnly != null)
                Session.RexDatabase.DeleteGroup(crid, groupIdForOwnOnly.Value);

            throw;
        }
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
            List<AuthUserEx> authUsers = Session.RexDatabase.GetAuthAndClaimsForEmailAndGroup(crid, email, null);

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
