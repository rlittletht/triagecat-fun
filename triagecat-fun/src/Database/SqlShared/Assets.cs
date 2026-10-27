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

    private static BaseGeneralDataTable<Asset> m_generalDataTable = new BaseGeneralDataTable<Asset>(s_queryAssets, s_insertNewAsset, s_deleteAsset);

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
        List<string> values = new();

        foreach (Types.Model.Assets.Asset asset in assets)
        {
            values.Add(
                $" ("
                + $"{TCore.SqlCore.SqlText.SqlifyQuoted(asset.AssetId.ToString())},"
                + $"{TCore.SqlCore.SqlText.SqlifyQuoted(asset.Path)},"
                + $"{TCore.SqlCore.SqlText.SqlifyQuoted(asset.Notes)},"
                + $"{asset.Priority}) ");
        }

        if (values.Count == 0)
            return 0;

        SqlCommandTextInit cmdTextInit =
            new($"{s_insertBase} VALUES {String.Join(",", values)}", s_tableAliases);

        sql.ExecuteNonQuery(cmdTextInit);
        return values.Count;
    }
}