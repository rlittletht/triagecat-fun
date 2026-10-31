using TCore.Logging;
using TCore.SqlClient;
using TCore.SqlCore;
using tt.Database.SqlShared;
using tt.Types.Model;

namespace tt.Database.SqlShared;

public class Claims
{
    public static SqlCommandTextInit s_insertUserClaim =
        new("INSERT INTO tt_claims (userid, claim, value) VALUES (@userid, @claim, @value)");

    public static SqlCommandTextInit s_deleteUserClaim =
        new("DELETE FROM tt_claims WHERE userid=@userid AND claim=@claim");

    private static TableAliases s_aliases = new TableAliases(
        new Dictionary<string, string>
        {
            { "tt_claims", "CLM" }
        });

    // the where will be built by the caller
    public static SqlCommandTextInit s_queryClaims = new("SELECT $$tt_claims$$.value FROM $$#tt_claims$$", s_aliases);

    /*----------------------------------------------------------------------------
        %%Function: SetOrUpdateClaimsForUser
        %%Qualified: tt.Database.SqlShared.Claims.SetOrUpdateClaimsForUser
    ----------------------------------------------------------------------------*/
    public static void SetOrUpdateClaimsForUser(ISql sql, CorrelationID crid, Guid userId, string claimType, string? claims)
    {
        bool fStartedTransaction = false;
        
        try
        {
            if (!sql.InTransaction)
            {
                sql.BeginTransaction();
                fStartedTransaction = true;
            }

            // first delete the existing claim, if any
            sql.ExecuteNonQuery(
                s_deleteUserClaim,
                cmd =>
                {
                    cmd.AddParameterWithValue("@userid", userId);
                    cmd.AddParameterWithValue("@claim", claimType);
                });

            // now insert the new claim, if the value isn't null
            if (claims != null)
            {
                sql.ExecuteNonQuery(
                    s_insertUserClaim,
                    cmd =>
                    {
                        cmd.AddParameterWithValue("@userid", userId);
                        cmd.AddParameterWithValue("@claim", claimType);
                        cmd.AddParameterWithValue("@value", claims);
                    });
            }

            if (fStartedTransaction)
            {
                sql.Commit();
            }
        }
        catch
        {
            if (fStartedTransaction)
            {
                sql.Rollback();
            }

            throw;
        }
    }

    /*----------------------------------------------------------------------------
        %%Function: GetClaimsForUserGroup
        %%Qualified: tt.Database.SqlShared.Claims.GetClaimsForUser
    ----------------------------------------------------------------------------*/
    public static string? GetClaimsForUser(ISql sql, CorrelationID crid, Guid userId, string claimType)
    {
        try
        {
            List<string> claims = new List<string>();

            SqlSelect select = new SqlSelect(s_queryClaims);

            select.Where.Add("$$tt_claims$$.userid = @userid", SqlWhere.Op.And);
            select.Where.Add("$$tt_claims$$.claim = @claim", SqlWhere.Op.And);

            return sql.TExecuteScalar<string>(
                select.ToCommandTextInit(),
                cmd =>
                {
                    cmd.AddParameterWithValue("@userid", userId);
                    cmd.AddParameterWithValue("@claim", claimType);
                });
        }
        catch (SqlExceptionNoResults)
        {
            return null;
        }
    }
}
