using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Pori.Frends.Data
{
    /// <summary>
    /// A data table.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Create a new table from a column list and a collection of rows.
        /// </summary>
        /// <param name="columns">The columns of the table, in order.</param>
        /// <param name="rows">The rows for the new table. Rows a presumed to be created using Table.Row()</param>
        internal Table(IEnumerable<string> columns, IEnumerable<dynamic> rows)
        {
            // Convert the columns and rows to a list
            // to make performance more predictable.
            Columns = columns.ToList();
            Rows    = rows.ToList();
        }

        /// <summary>
        /// Ordered list of the table's columns.
        /// </summary>
        public List<string> Columns { get; private set; }

        /// <summary>
        /// The rows of the table.
        /// </summary>
        public IEnumerable<dynamic> Rows { get; private set; }

        /// <summary>
        /// Number of rows in this table.
        /// </summary>
        public int Count { get { return Rows.Count(); } }

        /// <summary>
        /// Create a new table from the specified data.
        /// </summary>
        /// <typeparam name="TValue">Value type of the input data.</typeparam>
        /// <param name="columns">Ordered list of the columns for the table</param>
        /// <param name="data">The table's data (rows) as a collection of dictionary-like objects.</param>
        /// <returns>The created table.</returns>
        public static Table From<TValue>(List<string> columns, IEnumerable<IDictionary<string, TValue>> data)
        {
            // Create column ordered rows for the table
            var rows = data.Select(row => Table.Row(columns, row));

            // Return a new table using the columns and created rows
            return new Table(columns, rows);
        }

        /// <summary>
        /// Create a new table from the specified data.
        /// </summary>
        /// <typeparam name="TValue">Value type of the input data.</typeparam>
        /// <param name="columns">Ordered list of the columns for the table</param>
        /// <param name="data">The table's data (rows) as a list of row values.</param>
        /// <returns>The created table.</returns>
        public static Table From<TValue>(List<string> columns, List<List<TValue>> data)
        {
            // Create column ordered rows for the table
            var rows = data.Select(row => Table.Row(columns, row));

            // Return a new table using the columns and created rows
            return new Table(columns, rows);
        }

        /// <summary>
        /// Create a new table row from a dictionary-like object.
        /// </summary>
        /// <typeparam name="TValue">The value type of the input data.</typeparam>
        /// <param name="columns">Ordered list of the columns for the row.</param>
        /// <param name="values">A dictionary-like object containing the row's values.</param>
        /// <returns>The new table row as a dynamic object.</returns>
        private static dynamic Row<TValue>(List<string> columns, IDictionary<string, TValue> values)
        {
            // Create a new object for the row
            IDictionary<string, dynamic> row = new ExpandoObject();

            // Store the values in the column order
            for(int i = 0; i < columns.Count(); i++)
                row[columns[i]] = values[columns[i]];

            // Return the resulting row object
            return row;
        }

        /// <summary>
        /// Create a new table row from a list of column values.
        /// </summary>
        /// <typeparam name="TValue">The value type of the input data.</typeparam>
        /// <param name="columns">Ordered list of the columns for the row.</param>
        /// <param name="values">Ordered list of values for the row. Must be in the same order as the columns.</param>
        /// <returns>The new table row as a dynamic object.</returns>
        private static dynamic Row<TValue>(List<string> columns, List<TValue> values)
        {
            IDictionary<string, dynamic> row = new ExpandoObject();

            // Store the values in the column order
            for(int i = 0; i < columns.Count(); i++)
                row.Add(columns[i], values[i]);

            // Return the resulting row object
            return row;
        }
    }
}