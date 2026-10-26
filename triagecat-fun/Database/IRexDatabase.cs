/*---------0-------------------------------------------------------------------
    IRexDatabase

    All database classes must implement this interface

    Any access to the database must be through a class that implements this
    interface so we can use different database providers (e.g. SQL Server,
    SQLite, etc.) without changing the rest of the code.
----------------------------------------------------------------------------*/

using TCore.Logging;

namespace tt.Database;

public interface IRexDatabase
{

}
