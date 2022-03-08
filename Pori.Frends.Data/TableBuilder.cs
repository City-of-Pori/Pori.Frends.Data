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
        private Rows rows;

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

            // Create a new Rows object using the source table's rows as the
            // starting point.
            rows = new Rows(source.Rows);
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
    }

    /// <summary>
    /// A class for applying TableBuilder operations to table rows.
    /// </summary>
    public class Rows
    {
        /// <summary>
        /// The enumerable that will produce the rows for applied operations.
        /// </summary>
        private IEnumerable<dynamic> rows;

        /// <summary>
        /// Whether or not a copy of the source rows has been made at some point.
        /// Used to prevent making multiple copies of the rows when doing 
        /// multiple in-place operations on the rows
        /// </summary>
        private bool copied = false;

        /// <summary>
        /// Create a new row operations object starting with the given rows.
        /// </summary>
        /// <param name="source"></param>
        public Rows(IEnumerable<dynamic> source)
        {
            this.rows = source;
        }

        /// <summary>
        /// Add a new column to each row.
        /// </summary>
        /// <param name="column">The name of the column to add.</param>
        /// <param name="generator">A function to generate a value for each row.</param>
        public void AddColumn(string column, Func<dynamic, dynamic> generator)
        {
            // As we modify rows in-place, make a copy of the source rows
            // (if not already made)
            if(!copied)
                Copy();

            dynamic StoreColumnValue(dynamic row)
            {
                (row as RowDict)[column] = generator(row);

                return row;
            }

            rows = rows.Select(StoreColumnValue);
        }

        /// <summary>
        /// Create a copy of each row to allow in-place operations on the rows.
        /// </summary>
        private void Copy()
        {
            // Create a copy of each row
            rows = rows.Select(row => Table.Row<dynamic>(row));
            copied = true;
        }

        /// <summary>
        /// Filter the rows using the given filter function.
        /// </summary>
        /// <param name="filter">The function to use to filter the rows</param>
        public void Filter(Func<dynamic, bool> filter)
        {
            rows = rows.Where(filter);
        }

        /// <summary>
        /// Rename all columns of the rows
        /// </summary>
        /// <param name="columns">The new column names, in order.</param>
        public void RenameColumns(List<string> columns)
        {
            rows = rows
                    .Cast<RowDict>()
                    .Select(row => Table.Row(columns, ValuesOf(row)));

            // We produce new row objects, so no need to make a copy for
            // in-place operations
            copied = true;
        }

        /// <summary>
        /// Change the order of the columns for each row.
        /// </summary>
        /// <param name="columnOrder">The new column order.</param>
        public void ReorderColumns(List<string> columnOrder)
        {
            SelectColumns(columnOrder);
        }

        /// <summary>
        /// Select a subset of the columns for each row.
        /// </summary>
        /// <param name="columns"></param>
        public void SelectColumns(List<string> columns)
        {
            rows = rows.Select(row => Table.Row(columns, row as RowDict));

            // We produce new row objects, so no need to make a copy for
            // in-place operations
            copied = true;
        }

        /// <summary>
        /// Return the rows as row dictionaries.
        /// </summary>
        /// <returns>An enumerable that produces the rows as row dictionaries.</returns>
        public IEnumerable<RowDict> ToRows()
        {
            return rows.Cast<RowDict>();
        }

        /// <summary>
        /// Transform the values of a given column using a transformation function.
        /// </summary>
        /// <param name="column">The column to transform.</param>
        /// <param name="transform">The function used to calculate the new value for the column</param>
        public void TransformColumn(string column, Func<dynamic, dynamic> transform)
        {
            // As we modify rows in-place, make a copy of the source rows
            // (if not already made)
            if(!copied)
                Copy();

            dynamic ApplyTransformation(dynamic row)
            {
                (row as RowDict)[column] = transform(row);

                return row;
            }

            rows = rows.Select(ApplyTransformation);
        }

        /// <summary>
        /// Helper function to convert a row into an ordered list of values.
        /// </summary>
        /// <param name="row">The row whose values are to be extracted.</param>
        /// <returns>The values of the row as a list.</returns>
        private static List<dynamic> ValuesOf(RowDict row)
        {
            return row.Select(c => c.Value).ToList();
        }
    }
}
