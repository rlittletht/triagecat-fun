using System.Data.SQLite;
using TCore.SqlCore;
using TCore.SQLiteClient;
using tt.Controllers;
using tt.Types.Model;

namespace tt.Database.SQLiteDatabase;

public class CreateDatabase
{
    /*----------------------------------------------------------------------------
        %%Function: CreateAuthTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateAuthTable

        Create the auth table and prepopulate with the root user
        (password is testing123)
    ----------------------------------------------------------------------------*/
    static void CreateAuthTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tt_auth ("
            + "email NVARCHAR(128) NOT NULL PRIMARY KEY,"
            + "password NVARCHAR(128) NOT NULL,"
            + "userid VARCHAR(36) NOT NULL);");

        SqlCommandTextInit cmdtext = new("INSERT INTO tt_auth (email, password, userid) VALUES (@email, @password, @userid)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@email", "blue@traynrex.com");
                cmd.AddParameterWithValue("@password", "$2a$11$LcsP0vNs02sdfwKcIYj7FuKM7BRlB61zxKfzjFxrAhFbNgNZcIeLO");
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
            });
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateAssetTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateAssetTable

        also prepopulate the claims for the root user
    ----------------------------------------------------------------------------*/
    static void CreateAssetTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tt_assets ("
            + "assetid VARCHAR(36) NOT NULL,"
            + "path VARCHAR(1024) NOT NULL,"
            + "notes VARCHAR(1024) NOT NULL,"
            + "priority INTEGER NOT NULL, "
            + "PRIMARY KEY('assetid'));");
    }

    static void CreateUserPreferencesTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tt_userpreferences ("
            + "id VARCHAR(36) NOT NULL,"
            + "visibleRows INTEGER NOT NULL,"
            + "imagesPerPage INTEGER NOT NULL,"
            + "tileSize INTEGER NOT NULL,"
            + "PRIMARY KEY('id'));");

        SqlCommandTextInit cmdtext = new("INSERT INTO tt_userpreferences (id, visibleRows, imagesPerPage, tileSize) VALUES (@id, @visibleRows, @imagesPerPage, @tileSize)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@id", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@visibleRows", Config.VisibleRows);
                cmd.AddParameterWithValue("@imagesPerPage", Config.ImagesPerPage);
                cmd.AddParameterWithValue("@tileSize", Config.TileSize);
            });
    }

    static void CreateClaimsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tt_claims ("
            + "userid VARCHAR(36) NOT NULL,"
            + "claim NVARCHAR(128) NOT NULL,"
            + "value NVARCHAR(512) NULL,"
            + "PRIMARY KEY('userid','claim'));");

        SqlCommandTextInit cmdtext = new("INSERT INTO tt_claims (userid, claim, value) VALUES (@userid, @claim, @value)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@claim", TtClaimTypes.AssetAccess);
                cmd.AddParameterWithValue("@value", string.Join(",", [TtClaimTypes.Access_Read, TtClaimTypes.Access_Write, TtClaimTypes.Access_Create]));
            });

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@claim", TtClaimTypes.UserProfile);
                cmd.AddParameterWithValue("@value", string.Join(",", [TtClaimTypes.Access_Read, TtClaimTypes.Access_Write, TtClaimTypes.Access_Create]));
            });
    }
    
    /*----------------------------------------------------------------------------
        %%Function: CreateNewDatabase
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateNewDatabase
    ----------------------------------------------------------------------------*/
    public static ISql CreateNewDatabase(string database)
    {
        SQLiteConnection.CreateFile(database);

        SQLite connection = SQLiteDatabase.OpenDatabase(database);

        CreateAuthTable(connection);
        CreateAssetTable(connection);
        CreateUserPreferencesTable(connection);
        CreateClaimsTable(connection);

        return connection;
    }

    static bool IsTableDefined(ISql connection, string table)
    {
        TableInfo info = TableInfo.CreateTableInfo(connection, table);

        return !info.IsEmpty;
    }

    public static void ConvertDatabaseAsNeeded(ISql connection)
    {
        if (!IsTableDefined(connection, "tt_assets"))
        {
            CreateAssetTable(connection);
        }

        if (!IsTableDefined(connection, "tt_userpreferences"))
        {
            CreateUserPreferencesTable(connection);
        }
        
        if (!IsTableDefined(connection, "tt_claims"))
        {
            CreateClaimsTable(connection);
        }
    }
}
