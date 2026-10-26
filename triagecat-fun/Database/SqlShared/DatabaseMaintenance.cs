using System.Text;
using TCore.Logging;
using TCore.SqlClient;
using TCore.SqlCore;
using tr.Controllers;
using tr.Database.SqlShared;
using tr.Types.Model;

namespace tr.Database.SqlShared;

using ClaimsMap = Dictionary<Guid, Dictionary<Guid, HashSet<string>>>;

public class DatabaseMaintenance
{
#region Queries

    private static TableAliases s_aliases = new TableAliases(
        new Dictionary<string, string>
        {
            { "tr_claims", "CLM" },
            { "tr_usergroupsmap", "UGM" }
        }
    );

    public static SqlCommandTextInit s_queryAllClaims =
        new("SELECT $$tr_claims$$.userid, $$tr_claims$$.groupid, $$tr_claims$$.claim, $$tr_claims$$.value FROM $$#tr_claims$$", s_aliases);

    public static SqlCommandTextInit s_deleteOrphanedClaims =
        new(
            "DELETE CLM "
            + "FROM $$#tr_claims$$ "
            + "LEFT OUTER JOIN $$#tr_usergroupsmap$$ ON $$tr_usergroupsmap$$.userid = $$tr_claims$$.userid AND $$tr_usergroupsmap$$.groupid = $$tr_claims$$.groupid "
            + "WHERE $$tr_usergroupsmap$$.userid IS NULL",
            s_aliases);

    public static SqlCommandTextInit s_countOrphanedClaims =
        new(
            "SELECT COUNT(*) "
            + "FROM $$#tr_claims$$ "
            + "LEFT OUTER JOIN $$#tr_usergroupsmap$$ ON $$tr_usergroupsmap$$.userid = $$tr_claims$$.userid AND $$tr_usergroupsmap$$.groupid = $$tr_claims$$.groupid "
            + "WHERE $$tr_usergroupsmap$$.userid IS NULL",
            s_aliases);

#endregion

    /*----------------------------------------------------------------------------
        %%Function: DeleteOrphanedRepairs
        %%Qualified: tr.Database.SqlShared.DatabaseMaintenance.DeleteOrphanedRepairs
    ----------------------------------------------------------------------------*/
    static int DeleteOrphanedRepairs(ISql sql, CorrelationID crid, List<string> results, bool checkOnly)
    {
        if (checkOnly)
            return sql.NExecuteScalar(s_countOrphanedClaims);

        return sql.ExecuteNonQuery(s_deleteOrphanedClaims);
    }

    /*----------------------------------------------------------------------------
        %%Function: AddMissingRequiredClaims
        %%Qualified: tr.Database.SqlShared.DatabaseMaintenance.AddMissingRequiredClaims
    ----------------------------------------------------------------------------*/
    static int AddMissingRequiredClaims(ISql sql, CorrelationID crid, List<string> results, bool checkOnly)
    {
        SqlSelect select = new SqlSelect(s_queryAllClaims);

        ClaimsMap items = sql.ExecuteDelegatedQuery<ClaimsMap>(
            crid,
            select.ToString(),
            (ISqlReader reader, Guid _, ref ClaimsMap building) =>
            {
                ClaimsItem item =
                    new()
                    {
                        UserId = reader.GetGuid(0),
                        GroupId = reader.GetGuid(1),
                        Claim = reader.GetString(2),
                    };

                if (!building.ContainsKey(item.UserId))
                    building.Add(item.UserId, new Dictionary<Guid, HashSet<string>>());

                if (!building[item.UserId].ContainsKey(item.GroupId))
                    building[item.UserId].Add(item.GroupId, new HashSet<string>());

                building[item.UserId][item.GroupId].Add(item.Claim);
            },
            s_queryAllClaims.Aliases);

        List<ClaimsItem> missing = new();

        // now find any missing required items
        foreach (Guid userId in items.Keys)
        {
            foreach (Guid groupId in items[userId].Keys)
            {
                HashSet<string> claimsPresent = items[userId][groupId];

                foreach (string claim in TrClaimTypes.RequiredClaims)
                {
                    if (!claimsPresent.Contains(claim))
                    {
                        missing.Add(
                            new ClaimsItem()
                            {
                                UserId = userId,
                                GroupId = groupId,
                                Claim = claim,
                                Value = string.Empty
                            });
                    }
                }
            }
        }

        if (missing.Count == 0)
            return 0;

        if (!checkOnly)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO tr_claims (userid, groupid, claim, value) VALUES ");

            List<string> values = new();
            foreach (ClaimsItem item in missing)
            {
                StringBuilder value = new StringBuilder();
                value.Append("(");
                value.Append(SqlText.SqlifyQuoted(item.UserId.ToString()));
                value.Append(",");
                value.Append(SqlText.SqlifyQuoted(item.GroupId.ToString()));
                value.Append(",");
                value.Append(SqlText.SqlifyQuoted(item.Claim));
                value.Append(", '')");

                values.Add(value.ToString());
            }

            sb.Append(string.Join(",", values));

            sql.ExecuteNonQuery(sb.ToString());
        }

        return missing.Count;
    }

    /*----------------------------------------------------------------------------
        %%Function: RepairClaims
        %%Qualified: tr.Database.SqlShared.DatabaseMaintenance.RepairClaims
    ----------------------------------------------------------------------------*/
    public static IReadOnlyCollection<string> RepairClaims(ISql sql, CorrelationID crid, bool doRepairs)
    {
        List<string> results = new();

        bool checkingOnly = !doRepairs;

        if (checkingOnly)
            results.Add("NO CHANGES WERE PERFORMED");

        // first, delete all orphaned claims
        int deletedCount = DeleteOrphanedRepairs(sql, crid, results, checkingOnly);
        results.Add($"Deleted {deletedCount} orphaned claims");

        int missingCount = AddMissingRequiredClaims(sql, crid, results, checkingOnly);
        results.Add($"Added {missingCount} missing required claims");

        return results;
    }
}
