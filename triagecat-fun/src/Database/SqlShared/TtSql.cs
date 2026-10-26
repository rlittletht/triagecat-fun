using System.Text;
using TCore;
using TCore.Logging;
using TCore.SqlCore;
using tt.Database.Sql;

namespace tt.Database.SqlShared;

public static class TtSql
{
    public static string ConnectionString { get; set; } = "";

    public static SqlCommandTextInit s_SelectBase_Auth = new("SELECT userid, email, password FROM tt_auth");

    public static SqlCommandTextInit s_SelectBase_AuthUserEx =
        new(
            "SELECT $$tt_auth$$.userid, $$tt_auth$$.email, $$tt_auth$$.password "
            + "FROM $$#tt_auth$$ ",
            new TableAliases(
                new Dictionary<string, string>
                {
                    { "tt_auth", "AUTH" }
                })
        );

    /*----------------------------------------------------------------------------
        %%Function: BuildInsertAndRetrieveNewId
        %%Qualified: tr.Model.Types.TrSql.BuildInsertAndRetrieveNewId

        Most of our tables that are keyed by an id have an automatically
        generated key using newsequentialid().  when we insert, the id
        is autmatically generated, so we have to retrieve that ID that was
        created. This function creates that pattern.
    ----------------------------------------------------------------------------*/
    public static string BuildInsertAndRetrieveNewId(string insert, string values, string idField)
    {
        return string.Concat(
            $"DECLARE @InsertOutput TABLE ({idField} UNIQUEIDENTIFIER); ",
            insert,
            $"OUTPUT INSERTED.{idField} INTO @InsertOutput ",
            values,
            $"; SELECT {idField} FROM @InsertOutput");
    }

    /*----------------------------------------------------------------------------
        %%Function: BuildAndExecuteUpdateValuesForId
        %%Qualified: tr.Database.SqlShared.TrSql.BuildAndExecuteUpdateValuesForId
    ----------------------------------------------------------------------------*/
    public static void BuildAndExecuteUpdateValuesForId(
        ISql sql, CorrelationID crid, string baseUpdate, string idField, Guid id, Dictionary<string, object?> newValues)
    {
        if (newValues.Count == 0)
            return;

        // make sure there is a trailing space...
        StringBuilder updateQuery = new StringBuilder($"{baseUpdate} ");

        bool first = true;
        foreach (string field in newValues.Keys)
        {
            if (!first)
                updateQuery.Append(",");

            first = false;
            updateQuery.Append($"{field} = @{field} ");
        }

        updateQuery.Append($" WHERE {idField}=@id");

        sql.ExecuteNonQuery(
            updateQuery.ToString(),
            (cmd) =>
            {
                cmd.AddParameterWithValue("@id", id);
                foreach (string field in newValues.Keys)
                {
                    cmd.AddParameterWithValue($"@{field}", newValues[field]);
                }
            });
    }
}
