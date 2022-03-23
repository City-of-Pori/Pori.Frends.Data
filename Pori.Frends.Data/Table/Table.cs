using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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
        /// Serialize the table into a JToken.
        /// </summary>
        /// <returns>JArray containing all the rows of the table, each as a JObject.</returns>
        public JToken ToJson()
        {
            // Do the conversion
            return JToken.FromObject(Rows);
        }

        /// <summary>
        /// Create a new table from the specified data.
        /// </summary>
        /// <typeparam name="TValue">Value type of the input data.</typeparam>
        /// <param name="columns">Ordered list of the columns for the table</param>
        /// <param name="data">The table's data (rows) as a collection of dictionary-like objects.</param>
        /// <returns>The created table.</returns>
        public static Table From<TValue>(IEnumerable<string> columns, IEnumerable<IDictionary<string, TValue>> data)
        {
            // Create column ordered rows for the table
            var rows = data.Select(row => Table.Row(columns, row));

            // Return a new table using the columns and created rows
            return new Table(columns, rows);
        }

        /// <summary>
        /// Create a table from row data supplied as JObjects.
        /// </summary>
        /// <param name="columns">The columns for the table.</param>
        /// <param name="data">The row data for the table.</param>
        /// <returns>The created table.</returns>
        public static Table From(IEnumerable<string> columns, IEnumerable<JObject> data)
        {
            var rows = data.Select(row => Table.Row(columns, row));

            return new Table(columns, rows);
        }

        /// <summary>
        /// Create a new table from the specified data.
        /// </summary>
        /// <typeparam name="TCollection">The data type for each row's values</typeparam>
        /// <param name="columns">Ordered list of the columns for the table</param>
        /// <param name="data">The table's data (rows) as a list of row values.</param>
        /// <returns>The created table.</returns>
        public static Table From<TCollection>(IEnumerable<string> columns, IEnumerable<TCollection> data)
            where TCollection : IEnumerable<object>
        {
            // Create column ordered rows for the table
            var rows = data.Select(row => Table.Row(columns, row));

            // Return a new table using the columns and created rows
            return new Table(columns, rows);
        }

        internal static dynamic Row(IEnumerable<string> columns, JObject source)
        {
            return source.ToObject<ExpandoObject>();
        }

        /// <summary>
        /// Create a new table row from a dictionary-like object.
        /// </summary>
        /// <typeparam name="TValue">The value type of the input data.</typeparam>
        /// <param name="columns">Ordered list of the columns for the row.</param>
        /// <param name="values">A dictionary-like object containing the row's values.</param>
        /// <returns>The new table row as a dynamic object.</returns>
        internal static dynamic Row<TValue>(IEnumerable<string> columns, IDictionary<string, TValue> values)
        {
            // Create a new object for the row
            IDictionary<string, dynamic> row = new ExpandoObject();

            // Store the values in the column order
            foreach(var col in columns)
                row[col] = values[col];

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
        internal static dynamic Row<TValue>(IEnumerable<string> columns, IEnumerable<TValue> values)
        {
            IDictionary<string, dynamic> row = new ExpandoObject();

            // Store the values in the column order
            foreach(var (column, value) in columns.Zip(values, (c, v) => (c, v)))
                row[column] = value;

            // Return the resulting row object
            return row;
        }

        /// <summary>
        /// Create a new table row from another row.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static dynamic Row<TValue>(dynamic source)
        {
            IDictionary<string, dynamic> row = new ExpandoObject();

            foreach(var kvp in source as IDictionary<string, TValue>)
                row[kvp.Key] = kvp.Value;

            return row;
        }

        /// <summary>
        /// Create a table row with all column values set to null.
        /// </summary>
        /// <param name="columns">The columns for the row.</param>
        /// <returns>The new table row with all values set to null.</returns>
        internal static dynamic NullRow(IEnumerable<string> columns)
        {
            // Create an enumerable with null values.
            IEnumerable<dynamic> values = columns.Select(c => null as dynamic);

            // Create a new row using the columns and the null values.
            return Table.Row(columns, values);
        }
    }
}
