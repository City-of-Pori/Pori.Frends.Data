using System.Collections.Generic;
using System.Linq;

namespace Pori.Frends.Data.Tests
{
    /// <summary>
    /// A helper class for easily creating table data in code.
    /// Allows initializing from a collection initializer of object arrays
    /// </summary>
    public class InlineTable : IEnumerable<object[]>
    {
        /// <summary>
        /// The rows of the inline table.
        /// </summary>
        private List<object[]> rows    = new List<object[]>();

        /// <summary>
        /// The columns of the inline table.
        /// </summary>
        private List<string>   columns = null;

        // Implement IEnumerator<object[]>
        public IEnumerator<object[]> GetEnumerator() => rows.GetEnumerator();

        // Implement IEnumerator<object[]>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => rows.GetEnumerator();

        /// <summary>
        /// Add a row to the inline table. The first added row must be an 
        /// array of strings and is used as the columns.
        /// </summary>
        /// <param name="row">The row to add to the table.</param>
        public void Add(params object[] row)
        {
            // If columns hasn't been set yet, use the row data as the columns
            if(columns == null)
                columns = row.Cast<string>().ToList();
            else
                rows.Add(row);
        }

        /// <summary>
        /// Return the columns of the inline table.
        /// </summary>
        public IEnumerable<string> Columns { get => columns; }

        /// <summary>
        /// Return the columns of the inline table.
        /// </summary>
        public IEnumerable<object[]> Rows { get => rows; }

        /// <summary>
        /// Convert an inline table into an actual table.
        /// </summary>
        /// <param name="data">The inline table to convert.</param>
        public static implicit operator Table(InlineTable data) => Table.From(data.columns, data.rows);
    }
}