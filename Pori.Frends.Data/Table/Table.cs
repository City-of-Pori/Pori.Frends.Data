using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Pori.Frends.Data
{
    using RowDict = IDictionary<string, dynamic>;

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
        /// <param name="errors">An enuemrable that will contain any errors encountered while creating the table rows.</param>
        internal Table(IEnumerable<string> columns, IEnumerable<dynamic> rows, IEnumerable<Error> errors)
        {
            // Convert the columns and rows to a list
            // to make performance more predictable.
            Columns = columns.ToList();
            Rows    = rows.ToList();
            Errors  = errors;
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
        /// List of errors encountered while creating the table.
        /// </summary>
        public IEnumerable<Error> Errors { get; private set; }

        /// <summary>
        /// Get the rows of the table in a format suitable for use with the
        /// Frends.Csv.Create task.
        /// </summary>
        /// <returns>The rows of the table as a list of rows values, where each
        /// row's values are a list of object.</returns>
        public List<List<object>> ToCsvRows()
        {
            return Rows
                    .Cast<RowDict>()
                    .Select(row => row.Values.Cast<object>().ToList())
                    .ToList();
        }

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
        /// <param name="errors">An enumerable that will contain any errors encountered while creating the table rows.</param>
        /// <returns>The created table.</returns>
        public static Table From<TValue>(IEnumerable<string> columns, IEnumerable<IDictionary<string, TValue>> data, IEnumerable<Error> errors = null)
        {
            // Create column ordered rows for the table
            var rows = data.Select(row => Table.Row(columns, row));

            // Return a new table using the columns and created rows
            return new Table(columns, rows, errors);
        }

        /// <summary>
        /// Create a new table from the specified data.
        /// </summary>
        /// <typeparam name="TCollection">The data type for each row's values</typeparam>
        /// <param name="columns">Ordered list of the columns for the table</param>
        /// <param name="data">The table's data (rows) as a list of row values.</param>
        /// <param name="errors">An enumerable that will contain any errors encountered while creating the table rows.</param>
        /// <returns>The created table.</returns>
        public static Table From<TCollection>(IEnumerable<string> columns, IEnumerable<TCollection> data, IEnumerable<Error> errors = null)
            where TCollection : IEnumerable<object>
        {
            // Create column ordered rows for the table
            var rows = data.Select(row => Table.Row(columns, row));

            // Return a new table using the columns and created rows
            return new Table(columns, rows, errors);
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
            {
                TValue value;
                
                if(!values.TryGetValue(col, out value))
                    value = default;

                row[col] = value;
            }

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

        /// <summary>
        /// A class for reporting errors encountered while processing a table.
        /// </summary>
        public class Error : InvalidOperationException
        {
            /// <summary>
            /// Create a new table error.
            /// </summary>
            /// <param name="row">The row being processed when the error occurred.</param>
            /// <param name="index">The index of the row in the source table.</param>
            /// <param name="exception">The exception that caused the error.</param>
            public Error(dynamic row, long index, Exception exception) : base(exception.Message, exception)
            {
                Row   = row;
                Index = index;
            }

            /// <summary>
            /// The index of the row that caused the error.
            /// </summary>
            public long Index { get; set; }

            /// <summary>
            /// The row which resulted in the error.
            /// </summary>
            public dynamic Row { get; set; }
        }

        /// <summary>
        /// An exception class for failing the processing of a table.
        /// </summary>
        public class FailedOperationException : InvalidOperationException
        {
            /// <summary>
            /// Create a new failed table operation exception.
            /// </summary>
            /// <param name="errors"></param>
            public FailedOperationException(IEnumerable<Error> errors) : base("One or more operations on a table failed.")
            {
                Errors = errors;
            }

            /// <summary>
            /// List of the errors that caused this exception to be thrown.
            /// </summary>
            public IEnumerable<Error> Errors { get; set; }
        }

        /// <summary>
        /// How errors encountered while processing a table should be handled.
        /// </summary>
        public enum ErrorHandling
        {
            /// <summary>
            /// Continue processing rows even if previous rows caused errors.
            /// For tasks that modify the row values, a value of null is used
            /// when an error occurs.
            /// </summary>
            Continue,

            /// <summary>
            /// Discard any rows that cause errors and continue processing.
            /// </summary>
            Discard,

            /// <summary>
            /// Stop processing when the first error is encountered and
            /// throw an exception.
            /// </summary>
            Fail,

            /// <summary>
            /// Process all rows but fail after processing all rows if any
            /// errors were encountered.
            /// </summary>
            ContinueAndFail,
        }
    }
}
