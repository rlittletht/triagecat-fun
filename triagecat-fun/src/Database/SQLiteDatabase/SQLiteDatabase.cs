using System.Data.SQLite;
using TCore.Logging;
using TCore.SqlCore;
using TCore.SQLiteClient;
using tt.Database.Sql;
using tt.Model;
using tt.Types.Model.AssetGroups;
using tt.Types.Model.Auth;
using tt.Types.Model.Assets;
using tt.Types.Model.UserAssetStatus;
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

    public AuthUserEx GetAuthInfoFromUser(CorrelationID crid, Guid userId) => SqlShared.Auth.GetAuthInfoFromUser(m_connection, crid, userId);
    public List<AuthUserEx> GetAuthAndClaimsForEmail(CorrelationID crid, string email) => SqlShared.Auth.GetAuthAndClaimsForEmail(m_connection, crid, email);

    public void UpdatePasswordForUser(CorrelationID crid, string email, string newPasswordHash) =>
        SqlShared.Auth.UpdatePasswordForUser(m_connection, crid, email, newPasswordHash);

    public IReadOnlyCollection<Asset> GetAllAssets(CorrelationID crid) => SqlShared.Assets.GetAllAssets(m_connection, crid);

    public Guid CreateAssetGroup(CorrelationID crid, AssetGroup assetGroup) => SqlShared.AssetGroups.CreateAssetGroup(m_connection, crid, assetGroup);
    public AssetGroup GetAssetGroup(CorrelationID crid, Guid id) => SqlShared.AssetGroups.GetAssetGroup(m_connection, crid, id);
    public IReadOnlyCollection<AssetGroup> GetAllAssetGroups(CorrelationID crid) => SqlShared.AssetGroups.GetAllAssetGroups(m_connection, crid);

    public void CreateUserAssetStatus(CorrelationID crid, UserAssetStatus userAssetStatus) => SqlShared.UserAssetStatuses.CreateUserAssetStatus(m_connection, crid, userAssetStatus);
    public UserAssetStatus GetUserAssetStatus(CorrelationID crid, Guid assetId, Guid userId) => SqlShared.UserAssetStatuses.GetUserAssetStatus(m_connection, crid, assetId, userId);
    public void UpdateUserAssetStatus(CorrelationID crid, Guid assetId, Guid userId, bool triaged) => SqlShared.UserAssetStatuses.UpdateUserAssetStatus(m_connection, crid, assetId, userId, triaged);

    public int AddAssets(CorrelationID crid, IEnumerable<Types.Model.Assets.Asset> assets) => SqlShared.Assets.AddAssets(m_connection, crid, assets);
    public void UpdateAssetItemValues(CorrelationID crid, Guid assetId, Dictionary<string, object?> newValues) => SqlShared.Assets.UpdateAssetItemValues(m_connection, crid, assetId, newValues);

    public Guid CreateAuthUserForEmailPassword(CorrelationID crid, string email, string passwordHash) =>
        SqlShared.Auth.CreateAuthUserForEmailPassword(m_connection, crid, email, passwordHash);

    public void CreateUserPreferences(CorrelationID crid, Guid userId, UserPreferences userPreferences) => SqlShared.Users.CreateUserPreferences(m_connection, crid, userId, userPreferences);
    public UserPreferences GetUserPreferences(CorrelationID crid, Guid userId) => SqlShared.Users.GetUserPreferences(m_connection, crid, userId);
    public void UpdateUserPreferences(CorrelationID crid, Guid userId, Dictionary<string, object?> updates) => SqlShared.Users.UpdateUserPreferences(m_connection, crid, userId, updates);

}