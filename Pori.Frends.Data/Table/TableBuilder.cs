using System;
using System.Collections.Generic;
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
        internal TableBuilder(Table source)
        {
            // Create a copy the source table's columns to allow in-place
            // modifications to the list
            columns = new List<string>(source.Columns);

            // Create a new RowBuilder using the source table's rows as the
            // starting point.
            rows = new RowBuilder(source.Rows);
        }

        /// <summary>
        /// Produce the result of the applied operations as a table.
        /// </summary>
        /// <returns>The table resulting from the operations applied to the builder.</returns>
        public Table CreateTable()
        {
            // Create the resulting table using the columns and rows
            return Table.From(columns, rows.ToRows());
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
        /// Create a new table builder using a given table as the source.
        /// </summary>
        /// <param name="source">The table to use as the source for the new table</param>
        /// <returns></returns>
        public static TableBuilder From(Table source) => new TableBuilder(source);

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
