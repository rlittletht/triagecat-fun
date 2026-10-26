using TCore.Logging;
using TCore.SqlClient;
using TCore.SqlCore;
using tt.Database.SqlShared;
using tt.Types.Model;
using tt.Types.Model.Users;

namespace tr.Database.SqlShared;

public class Auth
{
    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string>
        {
        });

    /*----------------------------------------------------------------------------
        %%Function: GetAuthAndClaimsForEmailAndGroup
        %%Qualified: tr.Database.SqlShared.Auth.GetAuthAndClaimsForEmailAndGroup
    ----------------------------------------------------------------------------*/
    public static List<AuthUserEx> GetAuthAndClaimsForEmailAndGroup(ISql sql, CorrelationID crid, string email, Guid? groupId)
    {
        // validate the user
        SqlSelect select = new(TtSql.s_SelectBase_AuthUserEx);
        select.Where.Add("email = @email", SqlWhere.Op.And);
        if (groupId != null)
            select.Where.Add("$$tr_usergroupsmap$$.groupId = @groupId", SqlWhere.Op.And);

        return sql.ExecuteDelegatedQuery(
            crid,
            select.ToString(),
            (ISqlReader reader, Guid _crid, ref List<AuthUserEx> building) =>
            {
                AuthUserEx authUserEx =
                    new()
                    {
                        UserId = reader.GetGuid(0),
                        Email = reader.GetString(1),
                        Password = reader.GetString(2),
                        DisplayName = $"{reader.GetString(3)} {reader.GetString(4)}",
                        Claim = reader.GetNullableString(5),
                        ClaimValue = reader.GetNullableString(6)
                    };

                building.Add(authUserEx);
            },
            TtSql.s_SelectBase_AuthUserEx.Aliases,
            (cmd) =>
            {
                cmd.AddParameterWithValue("@email", email);
            });
    }

    /*----------------------------------------------------------------------------
        %%Function: UpdatePasswordForUser
        %%Qualified: tt.Database.SqlShared.Auth.UpdatePasswordForUser
    ----------------------------------------------------------------------------*/
    public static void UpdatePasswordForUser(ISql sql, CorrelationID crid, string email, string newPasswordHash)
    {
        try
        {
            sql.ExecuteNonQuery(
                "UPDATE tr_auth SET password = @newPasswordHash WHERE email = @email",
                (cmd) =>
                {
                    cmd.AddParameterWithValue("@newPasswordHash", newPasswordHash);
                    cmd.AddParameterWithValue("@email", email);
                });
        }
        catch (SqlExceptionNoResults)
        {
            throw new TtExceptionAuthenticationFailure();
        }
    }

    /*----------------------------------------------------------------------------
        %%Function: GetAuthInfoFromUserAndGroup
        %%Qualified: tt.Database.SqlShared.Auth.GetAuthInfoFromUserAndGroup
    ----------------------------------------------------------------------------*/
    public static AuthUserEx GetAuthInfoFromUserAndGroup(ISql sql, CorrelationID crid, Guid userId)
    {
        // validate the user
        SqlSelect select = new(TtSql.s_SelectBase_AuthUserEx);
        select.Where.Add("$$tt_auth$$.userid = @userId", SqlWhere.Op.And);

        AuthUserEx authUserEx = sql.ExecuteDelegatedQuery<AuthUserEx>(
            crid,
            select.ToString(),
            (ISqlReader reader, Guid _, ref AuthUserEx authUserEx) =>
            {
                authUserEx.UserId = reader.GetGuid(0);
                authUserEx.Email = reader.GetString(1);
                authUserEx.DisplayName = $"{reader.GetString(3)} {reader.GetString(4)}";
                authUserEx.Claim = reader.GetNullableString(5);
                authUserEx.ClaimValue = reader.GetNullableString(6);
            },
            TtSql.s_SelectBase_AuthUserEx.Aliases,
            (cmd) =>
            {
                cmd.AddParameterWithValue("@userId", userId);
            });

        return authUserEx;
    }
}
