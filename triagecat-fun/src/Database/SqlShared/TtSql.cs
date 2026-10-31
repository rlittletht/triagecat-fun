using Microsoft.Extensions.Logging;
using System.Text;
using TCore;
using TCore.Logging;
using TCore.SqlCore;
using tt.Database.Sql;
using static tt.Database.SqlShared.TtSql;

namespace tt.Database.SqlShared;

public static class TtSql
{
    public static string ConnectionString { get; set; } = "";

    public static SqlCommandTextInit s_SelectBase_Auth = new("SELECT userid, email, password FROM tt_auth");

    public static SqlCommandTextInit s_SelectBase_AuthUserEx =
        new(
            "SELECT $$tt_auth$$.userid, $$tt_auth$$.email, $$tt_auth$$.password, "
            + "$$tt_claims$$.claim, $$tt_claims$$.value "
            + "FROM $$#tt_auth$$ "
            + "LEFT OUTER JOIN $$#tt_claims$$ "
            + "ON $$tt_auth$$.userid = $$tt_claims$$.userid ",
            new TableAliases(
                new Dictionary<string, string> { { "tt_auth", "AUTH" }, { "tt_claims", "CLAIMS" } })
        );

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

    public struct PartedLineAndParams
    {
        public string Line;
        public Tuple<string, object>[] Params;
    }

    public static int ExecutePartedCommands<T>(
        ISql sql,
        string commandBase,
        IEnumerable<T> items,
        Func<T, int, PartedLineAndParams> buildLineAndParams,
        int partLimit,
        string joinString,
        TableAliases? aliases)
    {
        StringBuilder sb = new StringBuilder();
        int current = 0;
        int totalCount = 0;

        sb.Clear();
        sb.Append(commandBase);

        List<Tuple<string, object>> parameters = new List<Tuple<string, object>>();

        foreach (T item in items)
        {
            if (current == partLimit)
            {
                string command = sb.ToString();

                if (!string.IsNullOrWhiteSpace(command))
                {
                    sql.ExecuteNonQuery(
                        new SqlCommandTextInit(sb.ToString(), aliases),
                        (cmd) =>
                        {
                            foreach (Tuple<string, object> param in parameters)
                            {
                                cmd.AddParameterWithValue(param.Item1, param.Item2);
                            }
                        });
                    current = 0;
                }

                sb.Clear();
                sb.Append(commandBase);
                parameters.Clear();
            }

            if (current > 0)
                sb.Append(joinString);

            PartedLineAndParams lineAndParams = buildLineAndParams(item, current);
            sb.Append(lineAndParams.Line);
            parameters.AddRange(lineAndParams.Params);

            current++;
            totalCount++;
        }

        if (current > 0)
        {
            string sCmd = sb.ToString();

            if (!string.IsNullOrWhiteSpace(sCmd))
            {
                sql.ExecuteNonQuery(
                    new SqlCommandTextInit(sb.ToString(), aliases),
                    (cmd) =>
                    {
                        foreach (Tuple<string, object> param in parameters)
                        {
                            cmd.AddParameterWithValue(param.Item1, param.Item2);
                        }
                    });
            }
        }

        return totalCount;
    }

    public static int DoGenericPartedCommands<T>(
        ISql sql,
        string commandBase,
        IEnumerable<T> items,
        Func<T, int, PartedLineAndParams> buildLineAndParams,
        int partLimit,
        string joinString,
        TableAliases? aliases)
    {
        sql.BeginTransaction();

        try
        {
            int count = ExecutePartedCommands(
                sql,
                commandBase,
                items,
                buildLineAndParams,
                partLimit,
                joinString,
                aliases);

            sql.Commit();
            return count;
        }
        catch (Exception)
        {
            sql.Rollback();
            throw;
        }
    }
}