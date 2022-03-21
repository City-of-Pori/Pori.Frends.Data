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
        /// Concatenate the rows of one or more tables to the current rows.
        /// </summary>
        /// <param name="tables">
        /// The table whose rows are to be added to the current rows.
        /// </param>
        public void Concatenate(IEnumerable<Table> tables)
        {
            // Concatenate the rows of each table in turn.
            foreach(var table in tables)
                rows = rows.Concat(table.Rows);
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
        /// Expand the values of a given column as new columns.
        /// </summary>
        /// <param name="column">The column whose values are expanded.</param>
        /// <param name="nestedColumns">The nested columns to select as new columns.</param>
        public void ExpandColumn(string column, IEnumerable<string> nestedColumns)
        {
            // Make sure we can modify rows in-place
            if(!copied)
                Copy();

            // Function to expand the column for a single row
            dynamic DoExpandColumn(RowDict row)
            {
                // Get the value of the column
                RowDict valueToExpand = row[column];

                // Remove the expanded column from the row
                row.Remove(column);

                // Store all values from the expanded column to the row
                foreach(var nested in nestedColumns)
                    row[nested] = valueToExpand[nested];

                // Return the modified row
                return row;
            }

            // Apply the expansion to all the rows
            rows = rows
                    .Cast<RowDict>()
                    .Select(DoExpandColumn);
        }

        /// <summary>
        /// Returns a function for extracting from a row the values of the
        /// specified columns (to be used as a key for the row).
        /// </summary>
        /// <param name="keyColumns">The names of the columns to use as the key.</param>
        /// <returns></returns>
        private static Func<dynamic, List<dynamic>> ExtractKey(IEnumerable<string> keyColumns)
        {
            // Function to extract the values of the key columns as a list.
            List<dynamic> ExtractKey(dynamic row)
            {
                return keyColumns
                        .Select(column => (row as RowDict)[column])
                        .ToList();
            }

            return ExtractKey;
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
        /// Group rows by one or more columns.
        /// </summary>
        /// <param name="keyColumns">The columns to group the rows by.</param>
        /// <param name="elementSelector">Function to select a value based on each grouped row.</param>
        /// <param name="resultColumn">Name of the column to store the grouped rows in.</param>
        /// <param name="resultSelector">Function to produce a value for each group.</param>
        public void GroupBy(IEnumerable<string> keyColumns, Func<dynamic, dynamic> elementSelector,
                            string resultColumn, Func<IEnumerable<dynamic>, dynamic> resultSelector)
        {
            // Calculate the list of columns to include in the result
            List<string> columns = keyColumns.Concat(new string[] { resultColumn }).ToList();

            // Function to extract the values of the key columns as a list.
            List<dynamic> ExtractKey(dynamic row)
            {
                return keyColumns
                        .Select(column => (row as RowDict)[column])
                        .ToList();
            }

            // Function to create a single row in the result
            dynamic BuildResultRow(List<dynamic> key, IEnumerable<dynamic> groupedRows)
            {
                // Create the group value using the provided function
                dynamic group  = resultSelector(groupedRows);

                // The row's values are the values of the key columns plus
                // the group column.
                var values = key.Concat(new dynamic[] { group });

                // Return the new table row
                return Table.Row(columns, values);
            };

            // Perform the grouping on the rows
            rows = rows.GroupBy(ExtractKey, elementSelector, BuildResultRow, new RowEquality());

            // As we have create new rows, mark that rows can be modified
            // in-place.
            copied = true;
        }

        /// <summary>
        /// Perform an inner join on the current rows with the rows of a table.
        /// </summary>
        /// <param name="leftKeyColumns">The columns to use as the key of for each row.</param>
        /// <param name="right">The table whose rows are joined to the current rows.</param>
        /// <param name="rightKeyColumns">The columns to use as the key for the other table.</param>
        /// <param name="resultColumns">The names of the columns for the result rows.</param>
        public void InnerJoin(IEnumerable<string> leftKeyColumns,
                              Table right, IEnumerable<string> rightKeyColumns, List<string> resultColumns)
        {
            // Perform the join and get the result rows.
            rows = rows.Join(right.Rows, ExtractKey(leftKeyColumns), ExtractKey(rightKeyColumns),
                             (leftRow, rightRow) => Table.Row(resultColumns, (new[] {leftRow, rightRow})),
                             new RowEquality());

            // Mark that the rows can be modified in-place.
            copied = true;
        }

        /// <summary>
        /// Perform an outer join on the current rows with the rows of a table.
        /// </summary>
        /// <param name="leftKeyColumns">The columns to use as the key of for each row.</param>
        /// <param name="right">The table whose rows are joined to the current rows.</param>
        /// <param name="rightKeyColumns">The columns to use as the key for the other table.</param>
        /// <param name="resultColumns">The names of the columns for the result rows.</param>
        public void LeftOuterJoin(IEnumerable<string> leftKeyColumns,
                              Table right, IEnumerable<string> rightKeyColumns, List<string> resultColumns)
        {
            // Function for creating a table row for each pair of joined rows.
            IEnumerable<dynamic> SelectResult(dynamic leftRow, IEnumerable<dynamic> rightRows)
            {
                return rightRows
                        .DefaultIfEmpty(Table.NullRow(right.Columns) as RowDict)
                        .Select(rightRow => Table.Row(resultColumns, (new[] { leftRow, rightRow })));
            }

            // Perform the join and get the result rows.
            rows = rows
                    .GroupJoin(right.Rows,
                               ExtractKey(leftKeyColumns), ExtractKey(rightKeyColumns),
                               SelectResult, new RowEquality())
                    .SelectMany(rows => rows);

            // Mark that the rows can be modified in-place.
            copied = true;
        }


        /// <summary>
        /// Rename all columns of the rows
        /// </summary>
        /// <param name="columns">The new column names, in order.</param>
        public void RenameColumns(IEnumerable<string> columns)
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
        public void ReorderColumns(IEnumerable<string> columnOrder)
        {
            SelectColumns(columnOrder);
        }

        /// <summary>
        /// Select a subset of the columns for each row.
        /// </summary>
        /// <param name="columns"></param>
        public void SelectColumns(IEnumerable<string> columns)
        {
            rows = rows.Select(row => Table.Row(columns, row as RowDict));

            // We produce new row objects, so no need to make a copy for
            // in-place operations
            copied = true;
        }

        /// <summary>
        /// Sort the rows according to provided sorting criteria.
        /// </summary>
        /// <param name="criteria">List of sorting criteria to apply.</param>
        public void Sort(IEnumerable<TableBuilder.SortingCriterion> criteria)
        {
            IOrderedEnumerable<dynamic> sorted;

            // We need to separate the first criterion from the rest
            // because how multi-step sorting works with LINQ
            var initial = criteria.First();
            var rest    = criteria.Skip(1);
            
            // Sort the rows according to the first criterion
            if(initial.Order == Order.Ascending)
                sorted = rows.OrderBy(initial.KeySelector);
            else
                sorted = rows.OrderByDescending(initial.KeySelector);

            // Sort the rows according to the rest of the criteria
            foreach(var criterion in rest)
            {
                if(criterion.Order == Order.Ascending)
                    sorted = sorted.ThenBy(criterion.KeySelector);
                else
                    sorted = sorted.ThenByDescending(criterion.KeySelector);
            }

            // Store the sorted rows
            rows = sorted;
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

        /// <summary>
        /// A class comparing to lists of (row) values for equality.
        /// Used by Rows.GroupBy.
        /// </summary>
        internal class RowEquality : EqualityComparer<List<dynamic>>
        {
            public override bool Equals(List<dynamic> Xs, List<dynamic> Ys)
            {
                // Compare elements of each list in order
                return Xs.SequenceEqual(Ys);
            }

            public override int GetHashCode(List<dynamic> Xs)
            {
                // If only we had HashCode.Add available... :(
                // DJB2 hash variant
                // Algorithm source: http://www.cse.yorku.ca/~oz/hash.html
                return Xs
                        .Select(x => x.GetHashCode())
                        .Aggregate(5381, (hash, xhash) => ((hash << 5) + hash) ^ xhash);

            }
        }
    }
}
