using System.Data.SQLite;
using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.Assets;

namespace tt.Database.SqlShared;

public class Assets
{
#region Queries

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string> { { "tt_assets", "TTA" } });

    public static SqlCommandTextInit s_queryAssets =
        new(
            "SELECT $$tt_assets$$.assetid, $$tt_assets$$.path, $$tt_assets$$.notes, $$tt_assets$$.priority "
            + "FROM $$#tt_assets$$ ",
            s_tableAliases);


    static string s_insertBase = "INSERT INTO tt_assets (assetid, path, notes, priority) ";

    public static SqlCommandTextInit s_insertNewAsset =
        new(
            s_insertBase
            + " "
            + "VALUES (@assetid, @path, @notes, @priority)",
            s_tableAliases);

    public static SqlCommandTextInit s_deleteAsset =
        new(
            "DELETE FROM tt_assets WHERE assetid=@assetid",
            s_tableAliases);

#endregion

    private static BaseGeneralDataTable<Asset> m_generalDataTable = new BaseGeneralDataTable<Asset>(s_queryAssets, s_insertNewAsset, s_deleteAsset, "assetid");

    public static IReadOnlyCollection<Asset> GetAllAssets(ISql sql, CorrelationID crid)
    {
        return m_generalDataTable.QueryDataWithCustomizedQuery(
            sql,
            crid,
            null,
            reader =>
                new Asset() { AssetId = reader.GetGuid(0), Path = reader.GetString(1), Notes = reader.GetString(2), Priority = reader.GetInt32(3) },
            null);
    }

    public static int AddAssets(ISql sql, CorrelationID crid, IEnumerable<Types.Model.Assets.Asset> assets)
    {
        return TtSql.DoGenericPartedCommands(
            sql,
            $"{s_insertBase} VALUES ",
            assets,
            (asset, index) =>
            {
                return new TtSql.PartedLineAndParams
                       {
                           Line = $" (@assetid{index}, @path{index}, @notes{index}, @priority{index})",
                           Params = new[]
                                    {
                                        new Tuple<string, object>($"@assetid{index}", asset.AssetId),
                                        new Tuple<string, object>($"@path{index}", asset.Path),
                                        new Tuple<string, object>($"@notes{index}", asset.Notes),
                                        new Tuple<string, object>($"@priority{index}", asset.Priority)
                                    }
                       };
            },
            2,
            ",",
            s_tableAliases);
    }

    public static void UpdateAssetItemValues(ISql sql, CorrelationID crid, Guid assetId, Dictionary<string, object?> newValues)
    {
        m_generalDataTable.UpdateItemValues(sql, crid, "tt_assets", assetId, newValues);
    }
}