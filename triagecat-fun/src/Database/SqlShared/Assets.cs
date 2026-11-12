using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using System.Data.SQLite;
using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.Assets;
using tt.Types.Model.AssetTransformations;

namespace tt.Database.SqlShared;

public class Assets
{
#region Queries

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string> { { "tt_assets", "TTA" }, { "tt_assetgroup", "TAG" }, { "tt_userassetstatus", "TUAS" } });

    public static SqlCommandTextInit s_queryAssets =
        new(
            "SELECT $$tt_assets$$.assetid, $$tt_assets$$.path, $$tt_assets$$.notes, $$tt_assets$$.priority, $$tt_assets$$.assetgroupid, $$tt_assetgroup$$.name "
            + "FROM $$#tt_assets$$ LEFT JOIN $$#tt_assetgroup$$ ON $$tt_assets$$.assetgroupid = $$tt_assetgroup$$.id ",
            s_tableAliases);

    public static SqlCommandTextInit s_queryAssetsEx =
        new(
            "SELECT $$tt_assets$$.assetid, $$tt_assets$$.path, $$tt_assets$$.notes, $$tt_assets$$.priority, $$tt_assets$$.assetgroupid, $$tt_assetgroup$$.name, "
            + "CASE WHEN $$tt_userassetstatus$$.triaged IS NULL THEN 0 WHEN $$tt_userassetstatus$$.triaged = 1 THEN 1 ELSE 0 END, "
            + "CASE WHEN $$tt_userassetstatus$$.trashed IS NULL THEN 0 WHEN $$tt_userassetstatus$$.trashed= 1 THEN 1 ELSE 0 END "
            + "FROM $$#tt_assets$$ "
            + "LEFT JOIN $$#tt_assetgroup$$ ON $$tt_assets$$.assetgroupid = $$tt_assetgroup$$.id "
            + "LEFT JOIN $$#tt_userassetstatus$$ ON $$tt_assets$$.assetid = $$tt_userassetstatus$$.assetid AND $$tt_userassetstatus$$.userid = @userid ",
            s_tableAliases);


    static string s_insertBase = "INSERT INTO tt_assets (assetid, path, notes, priority, assetgroupid) ";

    public static SqlCommandTextInit s_insertNewAsset =
        new(
            s_insertBase
            + " "
            + "VALUES (@assetid, @path, @notes, @priority, @assetgroupid)",
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
                new Asset()
                {
                    AssetId = reader.GetGuid(0),
                    Path = reader.GetString(1),
                    Notes = reader.GetString(2),
                    Priority = reader.GetInt32(3),
                    AssetGroupId = string.IsNullOrWhiteSpace(reader.GetNullableString(4)) ? null : Guid.Parse(reader.GetNullableString(4)!),
                    AssetGroupName = reader.GetNullableString(5)
                },
            null);
    }

    class TransformationJsonItem
    {
        public string transformation { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    public static IReadOnlyCollection<AssetEx> QueryAssets(
        ISql sql, CorrelationID crid, Guid userId, bool? triaged, bool? trashed, string? assetGroupName, SqlCommandTextInit querySqlCommand)
    {
        SqlSelect select = new(querySqlCommand);

        if (triaged.HasValue)
        {
            select.Where.Add(
                "(CASE WHEN $$tt_userassetstatus$$.triaged IS NULL THEN 0 WHEN $$tt_userassetstatus$$.triaged = 1 THEN 1 ELSE 0 END) = @triaged",
                SqlWhere.Op.And);
        }

        if (trashed.HasValue)
        {
            select.Where.Add(
                "(CASE WHEN $$tt_userassetstatus$$.trashed IS NULL THEN 0 WHEN $$tt_userassetstatus$$.trashed = 1 THEN 1 ELSE 0 END) = @trashed",
                SqlWhere.Op.And);
        }

        if (!string.IsNullOrWhiteSpace(assetGroupName))
        {
            select.Where.Add("$$tt_assetgroup$$.name = @assetGroupName", SqlWhere.Op.And);
        }

        JsonSerializer json = JsonSerializer.Create();

        select.AddOrderBy("$$tt_assetgroup$$.name, $$tt_assets$$.path");
        List<AssetEx> assets = sql.ExecuteDelegatedQuery(
            crid,
            select.ToString(),
            (ISqlReader reader, Guid _, ref List<AssetEx> building) =>
            {
                List<AssetTransformation> transformations = new List<AssetTransformation>();
                if (!reader.IsDBNull(8))
                {
                    List<TransformationJsonItem> items = JsonConvert.DeserializeObject<List<TransformationJsonItem>>(reader.GetString(8))
                        ?? new List<TransformationJsonItem>();

                    foreach (TransformationJsonItem item in items)
                    {
                        transformations.Add(new AssetTransformation { Id = reader.GetGuid(0), Transformation = item.transformation, Value = item.value });
                    }
                }

                AssetEx assetEx = new()
                                  {
                                      AssetId = reader.GetGuid(0),
                                      Path = reader.GetString(1),
                                      Notes = reader.GetString(2),
                                      Priority = reader.GetInt32(3),
                                      AssetGroupId = reader.IsDBNull(4) ? null : reader.GetGuid(4),
                                      AssetGroupName = reader.GetNullableString(5),
                                      Triaged = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                                      Trashed = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                                      AssetTransformations = transformations.ToArray()
                                  };

                building.Add(assetEx);
            },
            s_tableAliases,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", userId);

                if (triaged.HasValue)
                {
                    cmd.AddParameterWithValue("@triaged", triaged.Value ? 1 : 0);
                }

                if (trashed.HasValue)
                {
                    cmd.AddParameterWithValue("@trashed", trashed.Value ? 1 : 0);
                }

                if (!string.IsNullOrWhiteSpace(assetGroupName))
                {
                    cmd.AddParameterWithValue("@assetGroupName", assetGroupName);
                }
            });

        return assets;
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
                           Line = $" (@assetid{index}, @path{index}, @notes{index}, @priority{index}, @assetgroupid{index})",
                           Params = new[]
                                    {
                                        new Tuple<string, object>($"@assetid{index}", asset.AssetId),
                                        new Tuple<string, object>($"@path{index}", asset.Path),
                                        new Tuple<string, object>($"@notes{index}", asset.Notes),
                                        new Tuple<string, object>($"@priority{index}", asset.Priority),
                                        new Tuple<string, object>($"@assetgroupid{index}", asset.AssetGroupId ?? (object)DBNull.Value)
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