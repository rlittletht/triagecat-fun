using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.AssetGroups;

namespace tt.Database.SqlShared;

public class AssetGroups
{
    public static TableAliases s_tableAliases = new(
        new Dictionary<string, string> { { "tt_assetgroup", "TAG" } });

    public static SqlCommandTextInit s_insertAssetGroup =
        new(
            "INSERT INTO tt_assetgroup (id, name, description) VALUES (@id, @name, @description)",
            s_tableAliases);

    public static SqlCommandTextInit s_queryAssetGroup =
        new(
            "SELECT $$tt_assetgroup$$.id, $$tt_assetgroup$$.name, $$tt_assetgroup$$.description "
            + "FROM $$#tt_assetgroup$$ WHERE $$tt_assetgroup$$.id = @id",
            s_tableAliases);

    public static SqlCommandTextInit s_queryAllAssetGroups =
        new(
            "SELECT $$tt_assetgroup$$.id, $$tt_assetgroup$$.name, $$tt_assetgroup$$.description "
            + "FROM $$#tt_assetgroup$$",
            s_tableAliases);

    public static void CreateAssetGroup(ISql sql, CorrelationID crid, AssetGroup assetGroup)
    {
        sql.ExecuteNonQuery(
            s_insertAssetGroup,
            cmd =>
            {
                cmd.AddParameterWithValue("@id", assetGroup.Id);
                cmd.AddParameterWithValue("@name", assetGroup.Name);
                cmd.AddParameterWithValue("@description", assetGroup.Description);
            });
    }

    public static AssetGroup GetAssetGroup(ISql sql, CorrelationID crid, Guid id)
    {
        return sql.ExecuteDelegatedQuery(
            crid,
            s_queryAssetGroup.CommandText,
            (ISqlReader reader, Guid _, ref AssetGroup building) =>
            {
                building.Id = reader.GetGuid(0);
                building.Name = reader.GetString(1);
                building.Description = reader.GetString(2);
            },
            s_queryAssetGroup.Aliases,
            cmd => cmd.AddParameterWithValue("@id", id));
    }

    public static IReadOnlyCollection<AssetGroup> GetAllAssetGroups(ISql sql, CorrelationID crid)
    {
        List<AssetGroup> assetGroups = sql.ExecuteDelegatedQuery(
            crid,
            s_queryAllAssetGroups.CommandText,
            (ISqlReader reader, Guid _, ref List<AssetGroup> building) =>
            {
                AssetGroup assetGroup = new()
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2)
                };

                building.Add(assetGroup);
            },
            s_queryAllAssetGroups.Aliases,
            null);

        return assetGroups;
    }
}
