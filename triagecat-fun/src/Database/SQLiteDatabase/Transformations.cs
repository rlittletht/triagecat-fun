using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.AssetTransformations;

namespace tt.Database.SQLiteDatabase;

public class Transformations
{
#region Query Definitions

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string> { { "tt_assettransformations", "AT" } });

    public static SqlCommandTextInit s_upsertAssetTransformation =
        new(
            "INSERT INTO tt_assettransformations (id, transformation, value) "
            + "VALUES (@id, @transformation, @value) "
            + "ON CONFLICT (id, transformation) DO UPDATE SET "
            + "value = excluded.value",
            s_tableAliases);

#endregion

    public static void SetAssetTransformation(ISql sql, CorrelationID crid, AssetTransformation assetTransformation)
    {
        sql.ExecuteNonQuery(
            s_upsertAssetTransformation,
            (cmd) =>
            {
                cmd.AddParameterWithValue("id", assetTransformation.Id);
                cmd.AddParameterWithValue("transformation", assetTransformation.Transformation);
                cmd.AddParameterWithValue("value", assetTransformation.Value);
            });
    }
}