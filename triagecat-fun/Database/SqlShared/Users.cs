using TCore.Logging;
using TCore.SqlCore;
using tr.Database.SqlShared;
using tr.Types.Model.Users;
using tr.Model;
using TCore.SqlClient;
using tr.Controllers;
using System.Reflection.Emit;
using System.Diagnostics.Metrics;
using Microsoft.Graph.Models;
using tr.Types.Model;

namespace tr.Database.SqlShared;

public class Users
{
#region Query Definitions

    public static TableAliases s_tableAliases = new TableAliases(
        new Dictionary<string, string> { { "tt_auth", "AUTH" } });

    public static SqlCommandTextInit s_queryUserIdScalar =
        new("SELECT $$tt_auth$$.userId FROM $$#tt_auth$$ WHERE $$tt_auth$$.email = @email", s_tableAliases);

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
}