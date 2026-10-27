/*---------0-------------------------------------------------------------------
    IRexDatabase

    All database classes must implement this interface

    Any access to the database must be through a class that implements this
    interface so we can use different database providers (e.g. SQL Server,
    SQLite, etc.) without changing the rest of the code.
----------------------------------------------------------------------------*/

using TCore.Logging;
using tt.Types.Model.Assets;
using tt.Types.Model.Users;

namespace tt.Database;

public interface IRexDatabase
{
    public List<AuthUserEx> GetAuthAndClaimsForEmail(CorrelationID crid, string email);
    public void UpdatePasswordForUser(CorrelationID crid, string email, string newPasswordHash);

    public IReadOnlyCollection<Asset> GetAllAssets(CorrelationID crid);
    public int AddAssets(CorrelationID crid, IEnumerable<Asset> assets);
}
