using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Pori.Frends.Data.Linq;

namespace Pori.Frends.Data
{
    using RowDict = IDictionary<string, dynamic>;

    /// <summary>
    /// A helper class for creating new tables by applying different 
    /// operations to an existing table.
    /// </summary>
    public class TableBuilder
    {
        /// <summary>
        /// The list of columns for the resulting table
        /// </summary>
        private List<string> columns;

        /// <summary>
        /// Rows for the resulting table.
        /// </summary>
        private RowBuilder rows;

        /// <summary>
        /// Create a new table builder instance using the given table as the
        /// source.
        /// </summary>
        /// <param name="source">The table to use as the source for the new table.</param>
        internal TableBuilder(Table source) : this(source.Columns, source.Rows)
        {}

        /// <summary>
        /// Create a new table builder from the given columns and rows as the
        /// starting point.
        /// </summary>
        /// <param name="columns">The original columns.</param>
        /// <param name="rows">The original rows.</param>
        internal TableBuilder(IEnumerable<string> columns, IEnumerable<dynamic> rows)
        {
            // Create a copy the source table's columns to allow in-place
            // modifications to the list
            this.columns = new List<string>(columns);

            // Create a new RowBuilder using the source table's rows as the
            // starting point.
            this.rows = new RowBuilder(rows);
        }

        /// <summary>
        /// Produce the result of the applied operations as a table.
        /// </summary>
        /// <returns>The table resulting from the operations applied to the builder.</returns>
        public Table CreateTable()
        {
            // Create the resulting table using the columns and rows
            return Table.From(columns, rows.ToRows(), rows.Errors);
        }
        

        /// <summary>
        /// Add a new column to the result.
        /// </summary>
        /// <param name="column">The name of the column to add to the result.</param>
        /// <param name="generator">
        /// A function to generate a values for the new column. Receives a
        /// row as it parameter and should produce values for the new column.
        /// </param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder AddColumn(string column, Func<dynamic, dynamic> generator)
        {
            // Add the new column to the list of column names
            columns.Add(column);

            // Add the new column to each row using the provided generator
            // function.
            rows.AddColumn(column, generator);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Add a new column to the result.
        /// </summary>
        /// <param name="column">The name of the column to add to the result</param>
        /// <param name="generator">
        /// A function to generate a values for the new column. Receives a
        /// row and its index as it parameters and should produce values for
        /// the new column.
        /// </param>
        /// <returns></returns>
        public TableBuilder AddColumn(string column, Func<dynamic, int, dynamic> generator)
        {
            columns.Add(column);

            rows.AddColumn(column, generator);

            return this;
        }

        /// <summary>
        /// Add the rows of one or more tables in to the result table.
        /// </summary>
        /// <param name="tables">The tables whose rows are to be added to the result table.</param>
        /// <returns>The table builder itself(for method chaining).</returns>
        public TableBuilder Concatenate(IEnumerable<Table> tables)
        {
            rows.Concatenate(tables);

            return this; // Enable method chaining
        }


        /// <summary>
        /// Split the source table into multiple tables of a given (maximum) size.
        /// </summary>
        /// <param name="size">The maximum size of each chunk table.</param>
        /// <returns>An enumerable that yields the chunk tables.</returns>
        public IEnumerable<Table> Chunk(int size)
        {
            IEnumerable<RowDict> rowsLeft = rows.ToRows();

            var chunkCount = Math.Ceiling(rowsLeft.Count() / (double)size);

            for(int i = 0; i < chunkCount; i++)
            {
                yield return Table.From(columns, rowsLeft.Take(size));

                rowsLeft = rowsLeft.Skip(size);
            }
        }

        /// <summary>
        /// Expand the nested values of a column into new columns.
        /// </summary>
        /// <param name="column">The column to expand</param>
        /// <param name="nestedColumns">The nested columns to include in the result table.</param>
        /// <returns>The table builder itself(for method chaining).</returns>
        public TableBuilder ExpandColumn(string column, IEnumerable<string> nestedColumns)
        {
            // Calculate the new columns for the result table.
            // Expanded columns are placed at the end of the row
            columns = columns
                        .Where(c => c != column)
                        .Concat(nestedColumns)
                        .ToList();

            // Actually expand the column for each row
            rows.ExpandColumn(column, nestedColumns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Filter the rows using the given filter function.
        /// </summary>
        /// <param name="filter">The funtion to use for filtering the rows.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder Filter(Func<dynamic, bool> filter)
        {
            rows.Filter(filter);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Filter the rows using the given filter function.
        /// </summary>
        /// <param name="filter">The funtion to use for filtering the rows.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder Filter(Func<dynamic, int, bool> filter)
        {
            rows.Filter(filter);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Perform a full outer join on the source table and another table.
        /// </summary>
        /// <param name="leftKeyColumns">The columns to use as the key for the source (left side) table.</param>
        /// <param name="leftResultColumn">The column name to use in the result table for the rows of the left table.</param>
        /// <param name="right">The right side table to join to the source (left side) table.</param>
        /// <param name="rightKeyColumns">The columns to as the key for the right side table.</param>
        /// <param name="rightResultColumn">The column name to use in the result table for the rows of the right side table.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder FullOuterJoin(IEnumerable<string> leftKeyColumns, string leftResultColumn,
                                          Table right, IEnumerable<string> rightKeyColumns, string rightResultColumn)
        {
            // The columns of the result table
            columns = (new[] { leftResultColumn, rightResultColumn }).ToList();

            // Perform the inner join on the rows.
            rows.FullOuterJoin(leftKeyColumns, right, rightKeyColumns, columns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Group rows based on the value of a given column.
        /// </summary>
        /// <param name="keyColumns"></param>
        /// <param name="elementSelector"></param>
        /// <param name="resultColumn"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public TableBuilder GroupBy(IEnumerable<string> keyColumns, Func<dynamic, dynamic> elementSelector,
                            string resultColumn, Func<IEnumerable<dynamic>, dynamic> resultSelector)
        {
            // Calculate the column list for the result
            columns = keyColumns.Concat(new string[] { resultColumn }).ToList();

            // Perform the grouping
            rows.GroupBy(keyColumns, elementSelector, resultColumn, resultSelector);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Perform an inner join on the source table and another table.
        /// </summary>
        /// <param name="leftKeyColumns">The columns to use as the key for the source (left side) table.</param>
        /// <param name="leftResultColumn">The column name to use in the result table for the rows of the left table.</param>
        /// <param name="right">The right side table to join to the source (left side) table.</param>
        /// <param name="rightKeyColumns">The columns to as the key for the right side table.</param>
        /// <param name="rightResultColumn">The column name to use in the result table for the rows of the right side table.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder InnerJoin(IEnumerable<string> leftKeyColumns, string leftResultColumn,
                                      Table right, IEnumerable<string> rightKeyColumns, string rightResultColumn)
        {
            // The columns of the result table
            columns = (new[] { leftResultColumn, rightResultColumn }).ToList();

            // Perform the inner join on the rows.
            rows.InnerJoin(leftKeyColumns, right, rightKeyColumns, columns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Perform a left outer join on the source table and another table.
        /// </summary>
        /// <param name="leftKeyColumns">The columns to use as the key for the source (left side) table.</param>
        /// <param name="leftResultColumn">The column name to use in the result table for the rows of the left side table.</param>
        /// <param name="right">The right side table to join to the source (left side) table.</param>
        /// <param name="rightKeyColumns">The columns to as the key for the right side table.</param>
        /// <param name="rightResultColumn">The column name to use in the result table for the rows of the right side table.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder LeftOuterJoin(IEnumerable<string> leftKeyColumns, string leftResultColumn,
                                      Table right, IEnumerable<string> rightKeyColumns, string rightResultColumn)
        {
            // The columns of the result table
            columns = (new[] { leftResultColumn, rightResultColumn }).ToList();

            // Perform the left outer join on the rows.
            rows.LeftOuterJoin(leftKeyColumns, right, rightKeyColumns, columns);

            return this; // Enable method chaining
        }

        private TableBuilder Load<TRow>(IEnumerable<TRow> rows, Func<TRow, RowDict> transform)
        {
            this.rows.Load(columns, rows, transform);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Specify how to handle errors encountered while building the
        /// result table.
        /// </summary>
        /// <param name="errorHandling">How to handle any errors.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder OnError(Table.ErrorHandling errorHandling)
        {
            rows.OnError(errorHandling);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Remove duplicate rows from the result table.
        /// </summary>
        /// <param name="keyColumns">The names of the columns to use as the key for matching duplicate rows.</param>
        /// <returns></returns>
        public TableBuilder RemoveDuplicates(IEnumerable<string> keyColumns)
        {
            rows.RemoveDuplicates(keyColumns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Rename columns in the result.
        /// </summary>
        /// <param name="renamings">A mapping of column names to new names.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder RenameColumns(IDictionary<string, string> renamings)
        {
            // Produce a new list of columns using the provided mapping.
            // The names columns not found in the mapping are not changed.
            columns = columns
                        .Select(c => renamings.ContainsKey(c) ? renamings[c] : c)
                        .ToList();

            // Use the new column names for each row
            rows.RenameColumns(columns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Change the order of columns in the result.
        /// </summary>
        /// <param name="columnOrder">The order of columns for the result.</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder ReorderColumns(IEnumerable<string> columnOrder)
        {
            // Reorder the column list
            columns = columns.Reorder(columnOrder).ToList();

            // Reorder each row to match the new column order.
            rows.ReorderColumns(columns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Select a subset of columns to include in the result.
        /// </summary>
        /// <param name="cols">The columns to include in the result (in order).</param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder SelectColumns(IEnumerable<string> cols)
        {
            // Replace the list of columns with the given columns
            columns = cols.ToList();

            // Select only the specified columns for each row
            rows.SelectColumns(columns);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Sort rows according to the specified sorting criteria
        /// </summary>
        /// <param name="criteria">The sorting criteria to use.</param>
        /// <returns></returns>
        public TableBuilder Sort(IEnumerable<SortingCriterion> criteria)
        {
            rows.Sort(criteria);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Transform the values of a given column using a transformation function.
        /// </summary>
        /// <param name="column">The column whose values are transformed.</param>
        /// <param name="transform">
        /// The function to produce new values for the column. Receives the
        /// row as a parameter and should produce a value for the specified
        /// column.
        /// </param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder TransformColumn(string column, Func<dynamic, dynamic> transform)
        {
            rows.TransformColumn(column, transform);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Transform the values of a given column using a transformation function.
        /// </summary>
        /// <param name="column">The column whose values are transformed.</param>
        /// <param name="transform">
        /// The function to produce new values for the column. Receives the
        /// row and its index as a parameter and should produce a value for
        /// the specified column.
        /// </param>
        /// <returns>The table builder itself (for method chaining).</returns>
        public TableBuilder TransformColumn(string column, Func<dynamic, int, dynamic> transform)
        {
            rows.TransformColumn(column, transform);

            return this; // Enable method chaining
        }

        /// <summary>
        /// Create a wrapped function that when called with a table row,
        /// calls the original function with the value of a specific column.
        /// </summary>
        /// <typeparam name="TResult">The result of the function</typeparam>
        /// <param name="column">The column whose values are fed to the given function</param>
        /// <param name="function">The function to wrap.</param>
        /// <returns>The wrapped function.</returns>
        public static Func<dynamic, TResult> ColumnFunction<TResult>(string column, Func<dynamic, TResult> function)
        {
            return row => function((row as RowDict)[column]);
        }

        /// <summary>
        /// Create a wrapped function that when called with a table row and
        /// its index, calls the original function with the value of a 
        /// specific column.
        /// </summary>
        /// <typeparam name="TResult">The result of the function</typeparam>
        /// <param name="column">The column whose values are fed to the given function</param>
        /// <param name="function">The function to wrap.</param>
        /// <returns>The wrapped function.</returns>
        public static Func<dynamic, int, TResult> IndexedColumnFunction<TResult>(string column, Func<dynamic, TResult> function)
        {
            return (row, index) => function((row as RowDict)[column]);
        }

        /// <summary>
        /// Create a wrapped function that when called with a table row and
        /// its index, calls the original function with the value of a 
        /// specific column.
        /// </summary>
        /// <typeparam name="TResult">The result of the function</typeparam>
        /// <param name="column">The column whose values are fed to the given function</param>
        /// <param name="function">The function to wrap.</param>
        /// <returns>The wrapped function.</returns>
        public static Func<dynamic, int, TResult> IndexedColumnFunction<TResult>(string column, Func<dynamic, int, TResult> function)
        {
            return (row, index) => function((row as RowDict)[column], index);
        }

        /// <summary>
        /// Create a new table builder using a given table as the source.
        /// </summary>
        /// <param name="source">The table to use as the source for the new table</param>
        /// <returns></returns>
        public static TableBuilder From(Table source) => new TableBuilder(source.Columns, source.Rows);

        /// <summary>
        /// Load data into a table from an enumerable.
        /// </summary>
        /// <param name="columns">The columns for the table to be created.</param>
        /// <param name="data">The data for the table rows.</param>
        /// <param name="rowLoader">Function for extracting row data for each row.</param>
        /// <returns>A table builder with the loaded data as the starting data.</returns>
        public static TableBuilder Load<TRow>(IEnumerable<string> columns, IEnumerable<TRow> data, Func<TRow, RowDict> rowLoader)
        {
            var builder = new TableBuilder(columns, Enumerable.Empty<dynamic>());

            return builder.Load(data, rowLoader);
        }

        /// <summary>
        /// Create a new table from the specified data.
        /// </summary>
        /// <typeparam name="TCollection">The data type for each row's values</typeparam>
        /// <param name="columns">Ordered list of the columns for the table</param>
        /// <param name="data">The table's data (rows) as a list of row values.</param>
        /// <returns>A table builder with the loaded data as the starting data.</returns>
        public static TableBuilder Load<TCollection>(IEnumerable<string> columns, IEnumerable<TCollection> data)
            where TCollection : IEnumerable<object>
        {
            var builder = new TableBuilder(columns, Enumerable.Empty<dynamic>());

            return builder.Load(data, row => Table.Row(columns, row));
        }

        /// <summary>
        /// A single sorting criterion for sorting the rows of a table.
        /// </summary>
        public class SortingCriterion
        {
            /// <summary>
            /// Function to extract the key for the sorting operations.
            /// </summary>
            public Func<dynamic, dynamic> KeySelector { get; set; }

            /// <summary>
            /// The order of the sorting for this criterion.
            /// </summary>
            public Order Order { get; set; }
        }
    }
}
