using TCore.Logging;
using TCore.SqlClient;
using TCore.SqlCore;
using tt.Database.SqlShared;
using tt.Types.Model;

namespace tt.Database.SqlShared;

public class Claims
{
    public static SqlCommandTextInit s_insertUserGroupClaim =
        new("INSERT INTO tr_claims (userid, groupid, claim, value) VALUES (@userid, @groupid, @claim, @value)");

    public static SqlCommandTextInit s_deleteUserGroupClaim =
        new("DELETE FROM tr_claims WHERE userid=@userid AND groupid=@groupid AND claim=@claim");

    private static TableAliases s_aliases = new TableAliases(
        new Dictionary<string, string>
        {
            { "tr_claims", "CLM" }
        });

    // the where will be built by the caller
    public static SqlCommandTextInit s_queryClaims = new("SELECT $$tr_claims$$.value FROM $$#tr_claims$$", s_aliases);

    /*----------------------------------------------------------------------------
        %%Function: SetOrUpdateClaimsForUser
        %%Qualified: tr.Database.SqlShared.Claims.SetOrUpdateClaimsForUser
    ----------------------------------------------------------------------------*/
    public static void SetOrUpdateClaimsForUser(ISql sql, CorrelationID crid, Guid userId, Guid groupId, string claimType, string? claims)
    {
        try
        {
            sql.BeginTransaction();

            // first delete the existing claim, if any
            sql.ExecuteNonQuery(
                s_deleteUserGroupClaim,
                cmd =>
                {
                    cmd.AddParameterWithValue("@userid", userId);
                    cmd.AddParameterWithValue("@groupid", groupId);
                    cmd.AddParameterWithValue("@claim", claimType);
                });

            // now insert the new claim, if the value isn't null
            if (claims != null)
            {
                sql.ExecuteNonQuery(
                    s_insertUserGroupClaim,
                    cmd =>
                    {
                        cmd.AddParameterWithValue("@userid", userId);
                        cmd.AddParameterWithValue("@groupid", groupId);
                        cmd.AddParameterWithValue("@claim", claimType);
                        cmd.AddParameterWithValue("@value", claims);
                    });
            }

            sql.Commit();
        }
        catch
        {
            sql?.Rollback();
            throw;
        }
    }

    /*----------------------------------------------------------------------------
        %%Function: GetClaimsForUserGroup
        %%Qualified: tr.Database.SqlShared.Claims.GetClaimsForUserGroup
    ----------------------------------------------------------------------------*/
    public static string? GetClaimsForUserGroup(ISql sql, CorrelationID crid, Guid userId, Guid groupId, string claimType)
    {
        try
        {
            List<string> claims = new List<string>();

            SqlSelect select = new SqlSelect(s_queryClaims);

            select.Where.Add("$$tr_claims$$.userid = @userid", SqlWhere.Op.And);
            select.Where.Add("$$tr_claims$$.groupid = @groupid", SqlWhere.Op.And);
            select.Where.Add("$$tr_claims$$.claim = @claim", SqlWhere.Op.And);

            return sql.TExecuteScalar<string>(
                select.ToCommandTextInit(),
                cmd =>
                {
                    cmd.AddParameterWithValue("@userid", userId);
                    cmd.AddParameterWithValue("@groupid", groupId);
                    cmd.AddParameterWithValue("@claim", claimType);
                });
        }
        catch (SqlExceptionNoResults)
        {
            return null;
        }
    }
}
