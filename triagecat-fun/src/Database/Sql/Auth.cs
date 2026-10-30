using TCore.Logging;
using TCore.SqlCore;

namespace tt.Database.Sql;

public class Auth
{
    /*----------------------------------------------------------------------------
        %%Function: CreateAuthUserForEmailPassword
        %%Qualified: tr.Database.Sql.Users.CreateAuthUserForEmailPassword

        exported entrypoint that cannot be used within a transaction
    ----------------------------------------------------------------------------*/
    public static Guid CreateAuthUserForEmailPassword(SqlDatabase database, CorrelationID crid, string email, string passwordHash)
    {
        ISql? sql = database.CreateSql(crid);

        try
        {
            Guid id = RT.Comb.Provider.Sql.Create();

            SqlShared.Auth.InsertNewAuthUser(sql, crid, id, email, passwordHash);
            return id;
        }
        finally
        {
            sql?.Close();
        }
    }
}