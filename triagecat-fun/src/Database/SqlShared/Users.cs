using TCore.Logging;
using TCore.SqlCore;
using tt.Database.SqlShared;
using tt.Model;
using TCore.SqlClient;
using tt.Controllers;
using System.Reflection.Emit;
using System.Diagnostics.Metrics;
using tt.Types.Model;
using tt.Types.Model.Users;

namespace tt.Database.SqlShared;

public class Users
{
#region Query Definitions

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string>
        {
            { "tt_auth", "AUTH" },
            { "tt_userpreferences", "TUP" }
        });

    public static SqlCommandTextInit s_queryUserIdScalar =
        new("SELECT $$tt_auth$$.userId FROM $$#tt_auth$$ WHERE $$tt_auth$$.email = @email", s_tableAliases);

    public static SqlCommandTextInit s_queryUserPreferences =
        new(
            "SELECT $$tt_userpreferences$$.id, $$tt_userpreferences$$.visibleRows, $$tt_userpreferences$$.imagesPerPage "
            + "FROM $$#tt_userpreferences$$ WHERE $$tt_userpreferences$$.id = @id",
            s_tableAliases);

    public static SqlCommandTextInit s_insertUserPreferences =
        new(
            "INSERT INTO tt_userpreferences (id, visibleRows, imagesPerPage) VALUES (@id, @visibleRows, @imagesPerPage)",
            s_tableAliases);

#endregion


    /*----------------------------------------------------------------------------
        %%Function: GetUserIdForEmail
        %%Qualified: tr.Database.SqlShared.Users.GetUserIdForEmail
    ----------------------------------------------------------------------------*/
    public static Guid GetUserIdForEmail(ISql sql, CorrelationID crid, string email)
    {
        return sql.ExecuteDelegatedQuery(
            crid,
            s_queryUserIdScalar.CommandText,
            (ISqlReader reader, Guid _, ref Guid building) => { building = reader.GetGuid(0); },
            s_queryUserIdScalar.Aliases,
            cmd => cmd.AddParameterWithValue("@email", email));
    }

    public static UserPreferences GetUserPreferences(ISql sql, CorrelationID crid, Guid userId)
    {
        return sql.ExecuteDelegatedQuery(
            crid,
            s_queryUserPreferences.CommandText,
            (ISqlReader reader, Guid _, ref UserPreferences building) =>
            {
                building.VisibleRows = reader.GetInt32(1);
                building.ImagesPerPage = reader.GetInt32(2);
            },
            s_queryUserPreferences.Aliases,
            cmd => cmd.AddParameterWithValue("@id", userId));
    }

    public static void CreateUserPreferences(ISql sql, CorrelationID crid, Guid userId, UserPreferences userPreferences)
    {
        sql.ExecuteNonQuery(
            s_insertUserPreferences,
            cmd =>
            {
                cmd.AddParameterWithValue("@id", userId);
                cmd.AddParameterWithValue("@visibleRows", userPreferences.VisibleRows);
                cmd.AddParameterWithValue("@imagesPerPage", userPreferences.ImagesPerPage);
            });
    }

    public static void UpdateUserPreferences(ISql sql, CorrelationID crid, Guid userId, Dictionary<string, object?> newValues)
    {
        TtSql.BuildAndExecuteUpdateValuesForId(sql, crid, "UPDATE tt_userpreferences SET", "id", userId, newValues);
    }
}