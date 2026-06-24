using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Base.Extensions;
using RSP.Common.Business;
using RSP.Common.DataAccess;

namespace RSP.AgileAssistant.Business.Base.Actions
{
    /// <summary>
    /// Project-level base class for asynchronous SQL Server actions. Adds typed
    /// query/non-query helpers on top of the RSP framework
    /// <see cref="SQLServerADOActionAsync{TResult}"/> base.
    /// </summary>
    /// <typeparam name="TResult">Result type produced by the action.</typeparam>
    public abstract class SqlActionAsyncBase<TResult> : SQLServerADOActionAsync<TResult>
    {
        /// <summary>
        /// Initializes the action with the supplied ADO configuration.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        protected SqlActionAsyncBase(IADOConfigurable adoConfigurable)
            : base(adoConfigurable)
        {
        }

        /// <summary>
        /// Executes a SELECT and maps the first row to a single object.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="sql">Parameterized SQL text.</param>
        /// <param name="parameters">SQL parameters.</param>
        /// <param name="dataRowFunc">Row mapper.</param>
        /// <param name="defaultValue">Value returned when no rows match.</param>
        /// <returns>The mapped object or the default value.</returns>
        protected async Task<T?> RunObjQueryAsync<T>(
            string sql,
            DbParameter[] parameters,
            Func<DataRow, T> dataRowFunc,
            T? defaultValue = default)
        {
            DbCommand command = this.CreateDbCommand(sql, parameters);
            DataTable table = await this.RunQueryAsync(command);
            if ((table == null) || (table.Rows.Count == 0))
            {
                return defaultValue;
            }

            return table.ToObj(dataRowFunc);
        }

        /// <summary>
        /// Executes a SELECT and maps every row to a list of objects.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="sql">Parameterized SQL text.</param>
        /// <param name="parameters">SQL parameters.</param>
        /// <param name="dataRowFunc">Row mapper.</param>
        /// <returns>The mapped objects, never null.</returns>
        protected async Task<IEnumerable<T>> RunListQueryAsync<T>(
            string sql,
            DbParameter[] parameters,
            Func<DataRow, T> dataRowFunc)
        {
            DbCommand command = this.CreateDbCommand(sql, parameters);
            DataTable table = await this.RunQueryAsync(command);
            return table.ToObjList(dataRowFunc);
        }

        /// <summary>
        /// Executes an INSERT/UPDATE/DELETE statement.
        /// </summary>
        /// <param name="sql">Parameterized SQL text.</param>
        /// <param name="parameters">SQL parameters.</param>
        /// <returns>Number of affected rows.</returns>
        protected async Task<int> RunNonQueryAsync(string sql, DbParameter[] parameters)
        {
            DbCommand command = this.CreateDbCommand(sql, parameters);
            return await this.RunNonQueryAsync(command);
        }
    }
}
