using System;
using System.Collections.Generic;
using System.Data;

namespace RSP.AgileAssistant.Business.Base.Extensions
{
    /// <summary>
    /// Mapping helpers that project <see cref="DataTable"/> rows onto strongly
    /// typed objects using a caller supplied row mapper.
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Maps the first row of the table to a single object.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="table">Source data table.</param>
        /// <param name="dataRowFunc">Mapper that builds an object from a row.</param>
        /// <returns>The mapped object, or default when no rows exist.</returns>
        public static T? ToObj<T>(this DataTable table, Func<DataRow, T> dataRowFunc)
        {
            if (dataRowFunc == null)
            {
                throw new ArgumentNullException(nameof(dataRowFunc));
            }

            if ((table == null) || (table.Rows.Count == 0))
            {
                return default;
            }

            return dataRowFunc(table.Rows[0]);
        }

        /// <summary>
        /// Maps every row of the table to a list of objects.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="table">Source data table.</param>
        /// <param name="dataRowFunc">Mapper that builds an object from a row.</param>
        /// <returns>The mapped objects, never null.</returns>
        public static IEnumerable<T> ToObjList<T>(this DataTable table, Func<DataRow, T> dataRowFunc)
        {
            if (dataRowFunc == null)
            {
                throw new ArgumentNullException(nameof(dataRowFunc));
            }

            List<T> results = new List<T>();
            if (table == null)
            {
                return results;
            }

            foreach (DataRow row in table.Rows)
            {
                results.Add(dataRowFunc(row));
            }

            return results;
        }
    }
}
