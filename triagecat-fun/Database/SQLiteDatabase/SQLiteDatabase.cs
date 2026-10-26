using System.Data.SQLite;
using TCore.Logging;
using TCore.SqlCore;
using TCore.SQLiteClient;
using tt.Database.Sql;
using tt.Model;
using tt.Types.Model.Users;

namespace tt.Database.SQLiteDatabase;

public class SQLiteDatabase : IRexDatabase
{
    private string m_database;
    private readonly ISql m_connection;

    // nothing special about this guid, its just convenient to have a static one
    public static readonly Guid s_rootUserId = Guid.Parse("F46381A2-66ED-EF11-83FA-C45AB1ABAF7D");

    public SQLiteDatabase(string databaseName)
    {
        m_database = databaseName;

        if (!Path.Exists(databaseName))
        {
            m_connection = Database.SQLiteDatabase.CreateDatabase.CreateNewDatabase(databaseName);
        }
        else
        {
            m_connection = OpenDatabase(databaseName);
            Database.SQLiteDatabase.CreateDatabase.ConvertDatabaseAsNeeded(m_connection);
        }
    }

    public static SQLite OpenDatabase(string database)
    {
        SQLite connection = SQLite.OpenConnection($"Data Source={database}");

        return connection;
    }


}
