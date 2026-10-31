using TCore.Logging;
using TCore.SqlCore;
using tt.Types.Model;

namespace tt.Database.SqlShared;

public class BaseGeneralDataTable<T>
{
    private readonly SqlCommandTextInit m_queryDataTable;
    private readonly SqlCommandTextInit m_insertNewItem;
    private readonly SqlCommandTextInit? m_deleteItem;

    private readonly string m_idColumn;
    
    public BaseGeneralDataTable(SqlCommandTextInit queryDataTable, SqlCommandTextInit insertNewItem, SqlCommandTextInit deleteItem, string idColumn = "id")
    {
        m_queryDataTable = queryDataTable;
        m_insertNewItem = insertNewItem;
        m_deleteItem = deleteItem;
        m_idColumn = idColumn;
    }

    public BaseGeneralDataTable(SqlCommandTextInit queryDataTable, SqlCommandTextInit insertNewItem, string idColumn)
    {
        m_queryDataTable = queryDataTable;
        m_insertNewItem = insertNewItem;
        m_deleteItem = null;
        m_idColumn = idColumn;
    }

    /*----------------------------------------------------------------------------
        %%Function: QueryDataWithCustomizedQuery
        %%Qualified: tr.Database.SqlShared.BaseGeneralDataTable.QueryDataWithCustomizedQuery
    ----------------------------------------------------------------------------*/
    public IReadOnlyCollection<T> QueryDataWithCustomizedQuery(
        ISql sql,
        CorrelationID crid,
        Action<SqlSelect>? customizeQuery,
        Func<ISqlReader, T> readItem,
        Action<ISqlCommand>? addQueryParams)
    {
        try
        {
            SqlSelect select = new SqlSelect(m_queryDataTable.CommandText);

            customizeQuery?.Invoke(select);

            List<T> data = sql.ExecuteDelegatedQuery(
                crid,
                select.ToString(),
                (ISqlReader reader, Guid _, ref List<T> building) =>
                {
                    building.Add(readItem(reader));
                },
                m_queryDataTable.Aliases,
                cmd => addQueryParams?.Invoke(cmd));

            return data;
        }
        catch (SqlExceptionNoResults)
        {
            return new List<T>();
        }
    }

    /*----------------------------------------------------------------------------
        %%Function: CreateItem
        %%Qualified: tr.Database.SqlShared.BaseGeneralDataTable.CreateItem
    ----------------------------------------------------------------------------*/
    public void CreateItem(ISql sql, CorrelationID crid, Action<ISqlCommand> addQueryParams)
    {
        sql.ExecuteNonQuery(
            m_insertNewItem.CommandText,
            cmd => addQueryParams(cmd),
            m_insertNewItem.Aliases);
    }

    /*----------------------------------------------------------------------------
        %%Function: DeleteItem
        %%Qualified: tr.Database.SqlShared.BaseGeneralDataTable.DeleteItem
    ----------------------------------------------------------------------------*/
    public void DeleteItem(ISql sql, CorrelationID crid, Action<ISqlCommand> addQueryParams)
    {
        if (m_deleteItem == null)
            throw new TtExceptionInvalidArgument(crid);

        sql.ExecuteNonQuery(
            m_deleteItem.CommandText,
            cmd => addQueryParams(cmd),
            m_deleteItem.Aliases);
    }

    /*----------------------------------------------------------------------------
        %%Function: UpdateItemValues
        %%Qualified: tr.Database.SqlShared.BaseGeneralDataTable.UpdateItemValues
    ----------------------------------------------------------------------------*/
    public void UpdateItemValues(ISql sql, CorrelationID crid, string tableName, Guid id, Dictionary<string, object?> newValues)
    {
        TtSql.BuildAndExecuteUpdateValuesForId(sql, crid, $"UPDATE {tableName} SET ", m_idColumn, id, newValues);
    }
}
