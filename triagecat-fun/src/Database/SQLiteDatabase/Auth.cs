using TCore.Logging;
using TCore.SqlCore;

namespace tt.Database.SQLiteDatabase;

public class Auth
{
    /*----------------------------------------------------------------------------
        %%Function: CreateAuthUserForEmailPassword
        %%Qualified: tr.Database.SQLiteDatabase.SQLiteDatabase.CreateAuthUserForEmailPassword
    ----------------------------------------------------------------------------*/
    public static Guid CreateAuthUserForEmailPassword(ISql sql, CorrelationID crid, string email, string passwordHash)
    {
        Guid id = RT.Comb.Provider.Sql.Create();

        SqlShared.Auth.InsertNewAuthUser(sql, crid, id, email, passwordHash);

        return id;
    }
}