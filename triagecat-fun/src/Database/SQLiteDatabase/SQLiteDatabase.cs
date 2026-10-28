using System.Data.SQLite;
using TCore.Logging;
using TCore.SqlCore;
using TCore.SQLiteClient;
using tt.Database.Sql;
using tt.Model;
using tt.Types.Model.Assets;
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

    public List<AuthUserEx> GetAuthAndClaimsForEmail(CorrelationID crid, string email) => SqlShared.Auth.GetAuthAndClaimsForEmail(m_connection, crid, email);

    public void UpdatePasswordForUser(CorrelationID crid, string email, string newPasswordHash) =>
        SqlShared.Auth.UpdatePasswordForUser(m_connection, crid, email, newPasswordHash);

    public IReadOnlyCollection<Asset> GetAllAssets(CorrelationID crid) => SqlShared.Assets.GetAllAssets(m_connection, crid);

    public int AddAssets(CorrelationID crid, IEnumerable<Types.Model.Assets.Asset> assets) => SqlShared.Assets.AddAssets(m_connection, crid, assets);
    public void UpdateAssetItemValues(CorrelationID crid, Guid assetId, Dictionary<string, object?> newValues) => SqlShared.Assets.UpdateAssetItemValues(m_connection, crid, assetId, newValues);
}