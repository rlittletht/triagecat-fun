using System.Diagnostics;
using TCore.Logging;
using TCore.SqlClient;
using TCore.SqlCore;
using tt.Types.Model.Auth;
using tt.Types.Model.Assets;
using tt.Types.Model.Users;

namespace tt.Database.Sql;

public class SqlDatabase: IRexDatabase
{
    private string m_connectionString;

    public SqlDatabase(string connectionString)
    {
        m_connectionString = connectionString;
    }

    public ISql CreateSql(CorrelationID crid)
    {
        return new LocalSqlHolder(null, crid, m_connectionString);
    }

    public List<AuthUserEx> GetAuthAndClaimsForEmail(CorrelationID crid, string email) =>
        Invoke(SqlShared.Auth.GetAuthAndClaimsForEmail, this, crid, email);

    public void UpdatePasswordForUser(CorrelationID crid, string email, string newPasswordHash) =>
        VoidInvoke(SqlShared.Auth.UpdatePasswordForUser, this, crid, email, newPasswordHash);

    public IReadOnlyCollection<Asset> GetAllAssets(CorrelationID crid) =>
        Invoke(SqlShared.Assets.GetAllAssets, this, crid);

    public int AddAssets(CorrelationID crid, IEnumerable<Types.Model.Assets.Asset> assets) => Invoke(SqlShared.Assets.AddAssets, this, crid, assets);
    public void UpdateAssetItemValues(CorrelationID crid, Guid assetId, Dictionary<string, object?> updates) => VoidInvoke(SqlShared.Assets.UpdateAssetItemValues, this, crid, assetId, updates);

    public Guid CreateAuthUserForEmailPassword(CorrelationID crid, string email, string passwordHash) =>
        Auth.CreateAuthUserForEmailPassword(this, crid, email, passwordHash);

    public AuthUserEx GetAuthInfoFromUser(CorrelationID crid, Guid userId) =>
        Invoke(SqlShared.Auth.GetAuthInfoFromUser, this, crid, userId);

    public void CreateUserPreferences(CorrelationID crid, Guid userId, UserPreferences userPreferences) =>
        VoidInvoke(SqlShared.Users.CreateUserPreferences, this, crid, userId, userPreferences);

    public UserPreferences GetUserPreferences(CorrelationID crid, Guid userId) =>
        Invoke(SqlShared.Users.GetUserPreferences, this, crid, userId);

    public void UpdateUserPreferences(CorrelationID crid, Guid userId, Dictionary<string, object?> updates) =>
        VoidInvoke(SqlShared.Users.UpdateUserPreferences, this, crid, userId, updates);

    #region Generic Invoke
    // Generic Invocations
    public delegate void VoidInvoke1Del<in T1>(ISql isql, CorrelationID crid, T1 t1);
    public delegate void VoidInvoke0Del(ISql isql, CorrelationID crid);
    public delegate void VoidInvoke2Del<out TRet, in T1, in T2>(ISql isql, CorrelationID crid, T1 t1, T2 t2);
    public delegate void VoidInvoke3Del<out TRet, in T1, in T2, in T3>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3);
    public delegate void VoidInvoke4Del<out TRet, in T1, in T2, in T3, in T4>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void VoidInvoke5Del<out TRet, in T1, in T2, in T3, in T4, in T5>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public delegate void VoidInvoke6Del<out TRet, in T1, in T2, in T3, in T4, in T5, in T6>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate void VoidInvoke7Del<out TRet, in T1, in T2, in T3, in T4, in T5, in T6, in T7>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate void VoidInvoke8Del<out TRet, in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);

    public delegate TRet Invoke0Del<out TRet>(ISql isql, CorrelationID crid);
    public delegate TRet Invoke1Del<out TRet, in T1>(ISql isql, CorrelationID crid, T1 t1);
    public delegate TRet Invoke2Del<out TRet, in T1, in T2>(ISql isql, CorrelationID crid, T1 t1, T2 t2);
    public delegate TRet Invoke3Del<out TRet, in T1, in T2, in T3>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3);
    public delegate TRet Invoke4Del<out TRet, in T1, in T2, in T3, in T4>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate TRet Invoke5Del<out TRet, in T1, in T2, in T3, in T4, in T5>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public delegate TRet Invoke6Del<out TRet, in T1, in T2, in T3, in T4, in T5, in T6>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate TRet Invoke7Del<out TRet, in T1, in T2, in T3, in T4, in T5, in T6, in T7>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate TRet Invoke8Del<out TRet, in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(ISql isql, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);

    public static void VoidInvoke(VoidInvoke0Del fun, SqlDatabase database, CorrelationID crid)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1>(VoidInvoke1Del<T1> fun, SqlDatabase database, CorrelationID crid, T1 t1)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2>(VoidInvoke2Del<object?, T1, T2> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2, T3>(VoidInvoke3Del<object?, T1, T2, T3> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2, t3);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2, T3, T4>(VoidInvoke4Del<object?, T1, T2, T3, T4> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2, t3, t4);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2, T3, T4, T5>(VoidInvoke5Del<object?, T1, T2, T3, T4, T5> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2, t3, t4, t5);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2, T3, T4, T5, T6>(VoidInvoke6Del<object?, T1, T2, T3, T4, T5, T6> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2, t3, t4, t5, t6);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2, T3, T4, T5, T6, T7>(VoidInvoke7Del<object?, T1, T2, T3, T4, T5, T6, T7> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2, t3, t4, t5, t6, t7);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static void VoidInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(VoidInvoke8Del<object?, T1, T2, T3, T4, T5, T6, T7, T8> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            fun(sql, crid, t1, t2, t3, t4, t5, t6, t7, t8);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet>(Invoke0Del<TRet> fun, SqlDatabase database, CorrelationID crid)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1>(Invoke1Del<TRet, T1> fun, SqlDatabase database, CorrelationID crid, T1 t1)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2>(Invoke2Del<TRet, T1, T2> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2, T3>(Invoke3Del<TRet, T1, T2, T3> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2, t3);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2, T3, T4>(Invoke4Del<TRet, T1, T2, T3, T4> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2, t3, t4);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2, T3, T4, T5>(Invoke5Del<TRet, T1, T2, T3, T4, T5> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2, t3, t4, t5);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2, T3, T4, T5, T6>(Invoke6Del<TRet, T1, T2, T3, T4, T5, T6> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2, t3, t4, t5, t6);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2, T3, T4, T5, T6, T7>(Invoke7Del<TRet, T1, T2, T3, T4, T5, T6, T7> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2, t3, t4, t5, t6, t7);
        }
        finally
        {
            sql?.Close();
        }
    }

    public static TRet Invoke<TRet, T1, T2, T3, T4, T5, T6, T7, T8>(Invoke8Del<TRet, T1, T2, T3, T4, T5, T6, T7, T8> fun, SqlDatabase database, CorrelationID crid, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
    {
        ISql? sql = null;
        try
        {
            sql = database.CreateSql(crid);
            return fun(sql, crid, t1, t2, t3, t4, t5, t6, t7, t8);
        }
        finally
        {
            sql?.Close();
        }
    }
    #endregion
}
