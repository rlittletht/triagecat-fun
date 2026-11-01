using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.UserAssetStatus;

namespace tt.Database.SqlShared;

public class UserAssetStatuses
{
    public static TableAliases s_tableAliases = new(
        new Dictionary<string, string> { { "tt_userassetstatus", "TUAS" } });

    public static SqlCommandTextInit s_insertUserAssetStatus =
        new(
            "INSERT INTO tt_userassetstatus (assetid, userid, triaged) VALUES (@assetid, @userid, @triaged)",
            s_tableAliases);

    public static SqlCommandTextInit s_queryUserAssetStatus =
        new(
            "SELECT $$tt_userassetstatus$$.assetid, $$tt_userassetstatus$$.userid, $$tt_userassetstatus$$.triaged "
            + "FROM $$#tt_userassetstatus$$ WHERE $$tt_userassetstatus$$.assetid = @assetid AND $$tt_userassetstatus$$.userid = @userid",
            s_tableAliases);

    public static SqlCommandTextInit s_updateUserAssetStatus =
        new(
            "UPDATE tt_userassetstatus SET triaged = @triaged WHERE assetid = @assetid AND userid = @userid",
            s_tableAliases);

    public static void CreateUserAssetStatus(ISql sql, CorrelationID crid, UserAssetStatus userAssetStatus)
    {
        sql.ExecuteNonQuery(
            s_insertUserAssetStatus,
            cmd =>
            {
                cmd.AddParameterWithValue("@assetid", userAssetStatus.AssetId);
                cmd.AddParameterWithValue("@userid", userAssetStatus.UserId);
                cmd.AddParameterWithValue("@triaged", userAssetStatus.Triaged);
            });
    }

    public static UserAssetStatus GetUserAssetStatus(ISql sql, CorrelationID crid, Guid assetId, Guid userId)
    {
        return sql.ExecuteDelegatedQuery(
            crid,
            s_queryUserAssetStatus.CommandText,
            (ISqlReader reader, Guid _, ref UserAssetStatus building) =>
            {
                building.AssetId = reader.GetGuid(0);
                building.UserId = reader.GetGuid(1);
                building.Triaged = reader.GetInt32(2) != 0;
            },
            s_queryUserAssetStatus.Aliases,
            cmd =>
            {
                cmd.AddParameterWithValue("@assetid", assetId);
                cmd.AddParameterWithValue("@userid", userId);
            });
    }

    public static void UpdateUserAssetStatus(ISql sql, CorrelationID crid, UserAssetStatus userAssetStatus)
    {
        sql.ExecuteNonQuery(
            s_updateUserAssetStatus,
            cmd =>
            {
                cmd.AddParameterWithValue("@assetid", userAssetStatus.AssetId);
                cmd.AddParameterWithValue("@userid", userAssetStatus.UserId);
                cmd.AddParameterWithValue("@triaged", userAssetStatus.Triaged);
            });
    }
}
