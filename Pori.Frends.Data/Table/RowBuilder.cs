using System;
using System.Collections.Generic;
using System.Linq;

namespace Pori.Frends.Data
{
    using RowDict = IDictionary<string, dynamic>;


    /// <summary>
    /// A class for building table rows by applying operations to existing rows.
    /// </summary>
    public class RowBuilder
    {
        /// <summary>
        /// The enumerable that will produce the rows after applying the operations.
        /// </summary>
        private IEnumerable<dynamic> rows;

        /// <summary>
        /// Whether or not a copy of the source rows has been made at some point.
        /// Used to prevent making multiple copies of the rows when doing 
        /// multiple in-place operations on the rows
        /// </summary>
        private bool copied = false;

        /// <summary>
        /// Create a new row builder starting with the given rows.
        /// </summary>
        /// <param name="source"></param>
        public RowBuilder(IEnumerable<dynamic> source)
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
                RowDict valueToExpand = row[column] ?? Table.NullRow(nestedColumns);

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
        /// Perform a full outer join on the current rows with the rows of a table.
        /// </summary>
        /// <param name="leftKeyColumns">The columns to use as the key of for each row.</param>
        /// <param name="right">The table whose rows are joined to the current rows.</param>
        /// <param name="rightKeyColumns">The columns to use as the key for the other table.</param>
        /// <param name="resultColumns">The names of the columns for the result rows.</param>
        public void FullOuterJoin(IEnumerable<string> leftKeyColumns,
                                  Table right, IEnumerable<string> rightKeyColumns, List<string> resultColumns)
        {
            var eq = new RowEquality();

            // Convert rows from both sides to a lookup
            var leftLookup  = rows.ToLookup(ExtractKey(leftKeyColumns), eq);
            var rightLookup = right.Rows.ToLookup(ExtractKey(rightKeyColumns), eq);

            // Get all the keys
            var leftKeys  = leftLookup.Select(group => group.Key);
            var rightKeys = rightLookup
                                .Where(group => !leftLookup.Contains(group.Key))
                                .Select(group => group.Key);

            var keys = leftKeys.Concat(rightKeys);

            // Function for calculating all the result rows for a given key
            IEnumerable<dynamic> RowsForKey(List<dynamic> key)
            {
                return leftLookup[key]
                        .DefaultIfEmpty(null)
                        .SelectMany(
                            leftRow => rightLookup[key]
                                        .DefaultIfEmpty(null)
                                        .Select(rightRow => Table.Row(resultColumns, (new[] { leftRow, rightRow })))
                        );
            }

            // Calculate the rows
            rows = keys.SelectMany(RowsForKey);

            // Mark that the rows can be modified in-place.
            copied = true;
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
                             (leftRow, rightRow) => Table.Row(resultColumns, (new[] { leftRow, rightRow })),
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
                        .DefaultIfEmpty(null)
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
