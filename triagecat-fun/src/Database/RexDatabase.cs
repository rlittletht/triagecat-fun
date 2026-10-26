namespace tt.Database;

using tt.Database.Sql;
using tt.Database.SQLiteDatabase;

public class RexDatabase
{
    public static IRexDatabase InitializeFromEnvironment(string databaseSource)
    {
        if (databaseSource == "SQLite")
        {
            return new SQLiteDatabase.SQLiteDatabase("rex.db");
        }
        else if (databaseSource == "SqlServer")
        {
            return new SqlDatabase(System.Environment.GetEnvironmentVariable("Sql.Local.ConnectionString") ?? "");
        }
        else
        {
            throw new Exception($"Unknown database source: {databaseSource}");
        }
    }
}
