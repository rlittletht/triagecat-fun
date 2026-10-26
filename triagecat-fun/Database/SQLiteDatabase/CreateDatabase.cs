using System.Data.SQLite;
using TCore.SqlCore;
using TCore.SQLiteClient;
using tr.Controllers;

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
            "CREATE TABLE tr_auth ("
            + "email NVARCHAR(128) NOT NULL PRIMARY KEY,"
            + "password NVARCHAR(128) NOT NULL,"
            + "userid VARCHAR(36) NOT NULL);");

        SqlCommandTextInit cmdtext = new("INSERT INTO tr_auth (email, password, userid) VALUES (@email, @password, @userid)");

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
        %%Function: CreateTriageTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateTriageTable

        also prepopulate the claims for the root user
    ----------------------------------------------------------------------------*/
    static void CreateClaimsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_triage ("
            + "userid VARCHAR(36) NOT NULL,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "claim NVARCHAR(128) NOT NULL,"
            + "value NVARCHAR(512) NULL,"
            + "PRIMARY KEY('userid','groupid','claim'));");

        SqlCommandTextInit cmdtext = new("INSERT INTO tr_claims (userid, groupid, claim, value) VALUES (@userid, @groupid, @claim, @value)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@groupid", TrClaimTypes.s_RexGroupId);
                cmd.AddParameterWithValue("@claim", TrClaimTypes.GroupAccess);
                cmd.AddParameterWithValue("@value", "read-own,read-other,write-own,write-other,manage-own");
            });

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@groupid", TrClaimTypes.s_RexGroupId);
                cmd.AddParameterWithValue("@claim", TrClaimTypes.PeopleAccess);
                cmd.AddParameterWithValue("@value", "read-own,read-other,write-own,write-other");
            });
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateGroupsTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateGroupsTable

        also prepopulate the root traynrex group
    ----------------------------------------------------------------------------*/
    static void CreateGroupsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_groups ("
            + "id VARCHAR(36) NOT NULL PRIMARY KEY,"
            + "name NVARCHAR(128) NOT NULL,"
            + "description NVARCHAR(512) NOT NULL);");

        SqlCommandTextInit cmdtext = new("INSERT INTO tr_groups (id, name, description) VALUES (@id, @name, @description)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@id", TrClaimTypes.s_RexGroupId);
                cmd.AddParameterWithValue("@name", "traynrex");
                cmd.AddParameterWithValue("@description", "TraynRex root group");
            });
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateLeaguesTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateLeaguesTable
    ----------------------------------------------------------------------------*/
    static void CreateLeaguesTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_leagues ("
            + "id VARCHAR(36) NOT NULL,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "shortName VARCHAR(32) NOT NULL,"
            + "name VARCHAR(128) NOT NULL,"
            + "description VARCHAR(128) NOT NULL,"
            + "PRIMARY KEY('id', 'groupid'));");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateLevelsTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateLevelsTable
    ----------------------------------------------------------------------------*/
    static void CreateLevelsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_levels ("
            + "id VARCHAR(36) NOT NULL,"
            + "leagueId VARCHAR(36) NOT NULL,"
            + "name VARCHAR(128) NOT NULL,"
            + "description VARCHAR(128) NOT NULL,"
            + "PRIMARY KEY('id', 'leagueId'));");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateTeamsTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateTeamsTable
    ----------------------------------------------------------------------------*/
    static void CreateTeamsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_teams ("
            + "id VARCHAR(36) NOT NULL,"
            + "levelId VARCHAR(36) NOT NULL,"
            + "name VARCHAR(128) NOT NULL,"
            + "description VARCHAR(128) NOT NULL,"
            + "PRIMARY KEY('id', 'levelId'));");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateGamesTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateGamesTable
    ----------------------------------------------------------------------------*/
    static void CreateGamesTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_games ("
            + "id VARCHAR(36) NOT NULL PRIMARY KEY,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "hometeamid VARCHAR(36) NOT NULL,"
            + "visitingteamid VARCHAR(36) NOT NULL,"
            + "ownerid VARCHAR(36) NOT NULL,"
            + "gametag INTEGER NOT NULL,"
            + "start NVARCHAR(64) NOT NULL,"
            + "length INTEGER NOT NULL,"
            + "subsiteid VARCHAR(36) NOT NULL,"
            + "notes VARCHAR(255) NOT NULL);");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreatePositionsTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreatePositionsTable
    ----------------------------------------------------------------------------*/
    static void CreatePositionsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_positions ("
            + "id VARCHAR(36) NOT NULL PRIMARY KEY,"
            + "sportid VARCHAR(36) NOT NULL,"
            + "shortName VARCHAR(32) NOT NULL,"
            + "name VARCHAR(128) NOT NULL);");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateSportsTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateSportsTable
    ----------------------------------------------------------------------------*/
    static void CreateSportsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_sports ("
            + "id VARCHAR(36) NOT NULL,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "shortName VARCHAR(32) NOT NULL,"
            + "name VARCHAR(128) NOT NULL,"
            + "PRIMARY KEY('id','groupid'));");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateUserGroupsMapTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateUserGroupsMapTable

        also prepopulate the root user and the root traynrex group
    ----------------------------------------------------------------------------*/
    static void CreateUserGroupsMapTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_usergroupsmap ("
            + "userid VARCHAR(36) NOT NULL,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "PRIMARY KEY('userid','groupid'));");

        SqlCommandTextInit cmdtext = new("INSERT INTO tr_usergroupsmap (userid, groupid) VALUES (@userid, @groupid)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@groupid", TrClaimTypes.s_RexGroupId);
            });
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateQualifierDefinitionsTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateQualifierDefinitionsTable
    ----------------------------------------------------------------------------*/
    static void CreateQualifierDefinitionsTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_qualifierdefinitions ("
            + "id VARCHAR(36) NOT NULL,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "name NVARCHAR(24) NOT NULL,"
            + "description NVARCHAR(512) NOT NULL,"
            + "PRIMARY KEY('id','groupid'));");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateQualifiersTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateQualifiersTable
    ----------------------------------------------------------------------------*/
    static void CreateQualifiersTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_qualifiers ("
            + "qualifierid VARCHAR(36) NOT NULL,"
            + "groupid VARCHAR(36) NOT NULL,"
            + "userid VARCHAR(36) NOT NULL,"
            + "sportid VARCHAR(36) NOT NULL,"
            + "leagueid VARCHAR(36) NULL,"
            + "isRemoved NVARCHAR(24) NOT NULL,"
            + "PRIMARY KEY('qualifierid','groupid','userid','sportid','leagueid'));");
    }

    static void CreateSitesTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_sites ("
            + "id VARCHAR(36) NOT NULL,"
            + "name NVARCHAR(128) NOT NULL,"
            + "address NVARCHAR(128) NOT NULL,"
            + "city NVARCHAR(64) NOT NULL,"
            + "state NVARCHAR(36) NULL,"
            + "zipcode NVARCHAR(24) NOT NULL,"
            + "phonenumber NVARCHAR(24) NOT NULL,"
            + "PRIMARY KEY('id'));");
    }

    static void CreateSubsitesTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_subsites ("
            + "id VARCHAR(36) NOT NULL,"
            + "siteid VARCHAR(36) NOT NULL,"
            + "name NVARCHAR(128) NOT NULL,"
            + "PRIMARY KEY('siteid','id'));");
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateUserProfileTable
        %%Qualified: tt.Database.SQLiteDatabase.CreateDatabase.CreateUserProfileTable

        also prepopulate the root user profile
    ----------------------------------------------------------------------------*/
    static void CreateUserProfileTable(ISql connection)
    {
        connection.ExecuteNonQuery(
            "CREATE TABLE tr_userprofile ("
            + "userid VARCHAR(36) NOT NULL PRIMARY KEY,"
            + "givenname VARCHAR(128) NOT NULL,"
            + "familyname VARCHAR(128) NOT NULL,"
            + "middlename VARCHAR(128) NOT NULL,"
            + "suffix VARCHAR(32) NOT NULL,"
            + "birthdate NVARCHAR(64) NOT NULL,"
            + "joined NVARCHAR(64) NOT NULL,"
            + "address1 VARCHAR(128) NOT NULL,"
            + "address2 VARCHAR(128) NOT NULL,"
            + "city VARCHAR(128) NOT NULL,"
            + "state VARCHAR(32) NOT NULL,"
            + "country VARCHAR(64) NOT NULL,"
            + "zipcode VARCHAR(16) NOT NULL,"
            + "mobilephone VARCHAR(16) NOT NULL,"
            + "workphone VARCHAR(16) NOT NULL,"
            + "landphone VARCHAR(16) NOT NULL,"
            + "preferredphone INTEGER NOT NULL);");

        SqlCommandTextInit cmdtext = new(
            "INSERT INTO tr_userprofile (userid, givenname, familyname, middlename, suffix, birthdate, joined, address1, address2, city, state, country, zipcode, mobilephone, workphone, landphone, preferredphone) "
            + " VALUES (@userid, @givenname, @familyname, @middlename, @suffix, @birthdate, @joined, @address1, @address2, @city, @state, @country, @zipcode, @mobilephone, @workphone, @landphone, @preferredphone)");

        connection.ExecuteNonQuery(
            cmdtext,
            cmd =>
            {
                cmd.AddParameterWithValue("@userid", SQLiteDatabase.s_rootUserId);
                cmd.AddParameterWithValue("@givenname", "Root");
                cmd.AddParameterWithValue("@familyname", "Admin");
                cmd.AddParameterWithValue("@middlename", "");
                cmd.AddParameterWithValue("@suffix", "");
                cmd.AddParameterWithValue("@birthdate", DateTime.Parse("1/1/2000"));
                cmd.AddParameterWithValue("@joined", DateTime.Parse("1/1/2000"));
                cmd.AddParameterWithValue("@address1", "123 Strikes Place");
                cmd.AddParameterWithValue("@address2", "");
                cmd.AddParameterWithValue("@city", "Nowhere");
                cmd.AddParameterWithValue("@state", "WA");
                cmd.AddParameterWithValue("@country", "USA");
                cmd.AddParameterWithValue("@zipcode", "12345");
                cmd.AddParameterWithValue("@mobilephone", "123-456-7890");
                cmd.AddParameterWithValue("@workphone", "");
                cmd.AddParameterWithValue("@landphone", "");
                cmd.AddParameterWithValue("@preferredphone", 0);
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
        CreateOneTimeKeysTable(connection);
        CreateClaimsTable(connection);
        CreateGroupsTable(connection);
        CreateLeaguesTable(connection);
        CreateLevelsTable(connection);
        CreateTeamsTable(connection);
        CreateGamesTable(connection);
        CreatePositionsTable(connection);
        CreateSportsTable(connection);
        CreateUserGroupsMapTable(connection);
        CreateUserProfileTable(connection);
        CreateQualifierDefinitionsTable(connection);
        CreateQualifiersTable(connection);

        return connection;
    }

    static bool IsTableDefined(ISql connection, string table)
    {
        TableInfo info = TableInfo.CreateTableInfo(connection, table);

        return !info.IsEmpty;
    }

    public static void ConvertDatabaseAsNeeded(ISql connection)
    {
        if (!IsTableDefined(connection, "tr_qualifierdefinitions"))
            CreateQualifierDefinitionsTable(connection);
        if (!IsTableDefined(connection, "tr_qualifiers"))
            CreateQualifiersTable(connection);
        if (!IsTableDefined(connection, "tr_sites"))
            CreateSitesTable(connection);
        if (!IsTableDefined(connection, "tr_subsites"))
            CreateSubsitesTable(connection);
        if (!IsTableDefined(connection, "tr_levels"))
            CreateLevelsTable(connection);
        if (!IsTableDefined(connection, "tr_teams"))
            CreateTeamsTable(connection);
        if (!IsTableDefined(connection, "tr_games"))
            CreateGamesTable(connection);
    }
}
