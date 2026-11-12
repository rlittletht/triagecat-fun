using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.AssetTransformations;

namespace tt.Database.Sql;

public class Transformations
{
#region Query Definitions

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string> { { "tt_assettransformations", "AT" } });

    public static SqlCommandTextInit s_upsertAssetTransformation =
        new(
            "SET XACT_ABORT ON; "
            + " " +
            " BEGIN TRY "
            + " BEGIN TRANSACTION  "
            + " UPDATE tt_assettransformations WITH (UPDLOCK, HOLDLOCK) " 
            + " SET value = @value "
            + " WHERE id = @id AND transformation = @transformation "
            + "IF @@ROWCOUNT = 0 "
            + "BEGIN "
            + "     INSERT INTO tt_assettransformations (id, transformation, value) "
            + "     VALUES (@id, @transformation, @value) "
            + "END; "
            + "COMMIT TRANSACTION; "
            + "END TRY "
            + "BEGIN CATCH "
            + "IF @@TRANCOUNT > 0 "
            + "   ROLLBACK TRANSACTION; "
            + "   THROW; "
            + "END CATCH",
            s_tableAliases);

#endregion

    public static void SetAssetTransformation(ISql sql, CorrelationID crid, AssetTransformation assetTransformation)
    {
        try
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
        catch
        {
            throw;
        }
    }
}
