using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model.Assets;
using tt.Types.Model.AssetTransformations;

namespace tt.Database.SQLiteDatabase;

public class Assets
{
    #region Query Definitions

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string>
        {
            { "tt_assets", "TTA" }, { "tt_assetgroup", "TAG" }, { "tt_userassetstatus", "TUAS" },
            { "tt_assettransformations", "TTAF" }
        });

    public static SqlCommandTextInit s_queryAssetsEx =
        new(
            "SELECT $$tt_assets$$.assetid, $$tt_assets$$.path, $$tt_assets$$.notes, $$tt_assets$$.priority, $$tt_assets$$.assetgroupid, $$tt_assetgroup$$.name, "
            + "CASE WHEN $$tt_userassetstatus$$.triaged IS NULL THEN 0 WHEN $$tt_userassetstatus$$.triaged = 1 THEN 1 ELSE 0 END, "
            + "CASE WHEN $$tt_userassetstatus$$.trashed IS NULL THEN 0 WHEN $$tt_userassetstatus$$.trashed= 1 THEN 1 ELSE 0 END,"
            + "COALESCE ("
            + "   ("
            + "      SELECT json_group_array"
            + "         ("
            + "         json_object"
            + "            ("
            + "            'transformation', $$tt_assettransformations$$.transformation, "
            + "            'value', $$tt_assettransformations$$.value"
            + "            )"
            + "         )"
            + "      FROM $$#tt_assettransformations$$"
            + "      WHERE $$tt_assettransformations$$.id = $$tt_assets$$.assetid"
            + "   ), '[]') AS Transformations "
            + "FROM $$#tt_assets$$ "
            + "LEFT JOIN $$#tt_assetgroup$$ ON $$tt_assets$$.assetgroupid = $$tt_assetgroup$$.id "
            + "LEFT JOIN $$#tt_userassetstatus$$ ON $$tt_assets$$.assetid = $$tt_userassetstatus$$.assetid AND $$tt_userassetstatus$$.userid = @userid ",
            s_tableAliases);


    #endregion
}