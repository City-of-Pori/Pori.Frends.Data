using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks
using Pori.Frends.Data.Linq;

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    /// <summary>
    /// Frends task for data related tasks.
    /// </summary>
    public static class DataTasks
    {
        /// <summary>
        /// Load data into a table structure for further processing.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The data as a Pori.Frends.Data.Table</returns>
        public static Table Load([PropertyTab] LoadParameters input, CancellationToken cancellationToken)
        {
            // Load data based on the input format
            switch(input.Format)
            {
                case LoadFormat.CSV:
                    // Extract the headers and data from the input
                    var headers = input.CsvData.Headers as List<string>;
                    var data    = input.CsvData.Data as List<List<object>>;

                    // Create a table using the data
                    return Table.From(headers, data);

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Filter rows from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Filtered data as a Pori.Frends.Data.Table</returns>
        public static Table Filter([PropertyTab] FilterParameters input, CancellationToken cancellationToken)
        {
            IEnumerable<dynamic> rows;

            // Check whether to apply the filter to a specific column or the whole row
            if(input.FilterType == ProcessingType.Column)
            {
                // Check that the input table has the column to be used for filtering
                if(!input.Data.Columns.Contains(input.FilterColumn))
                    throw new ArgumentException($"The input table does not contain column '{input.FilterColumn}'");

                // Filter the rows using the specified filter function,
                // but give it only the column value as input.
                rows = from   row in input.Data.Rows
                       let    column = (row as IDictionary<string, object>)[input.FilterColumn]
                       where  input.Filter(column)
                       select row;
            }
            else
                rows = input.Data.Rows.Where(input.Filter);

            // Return the a new table with the filtered rows
            return new Table(input.Data.Columns, rows);
        }

        

        /// <summary>
        /// Rename the columns of a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with reordered columns as a Pori.Frends.Data.Table</returns>
        public static Table RenameColumns([PropertyTab] RenameColumnsParameters input, CancellationToken cancellationToken)
        {
            // The list of column renamings to do
            IEnumerable<ColumnRename> renamings = input.Renamings;

            // Mapping from old column names to new ones
            Dictionary<string, string> newNameOf = renamings.ToDictionary(r => r.Column, r => r.NewName);

            // List of columns to rename
            IEnumerable<string> columnsToRename = renamings.Select(r => r.Column);

            // Check that a column isn't specified more than once
            if(columnsToRename.Distinct().Count() != columnsToRename.Count())
                throw new ArgumentException("The same column cannot be specified more than once");

            // Check that the input table contains all specified columns
            if(!columnsToRename.All(c => input.Data.Columns.Contains(c)))
                throw new ArgumentException("Column list contains columns that are not in the input table.");

            // List of columns before renaming (but after possible reordering
            // and/or discarding). Initially the list of columns for the input
            // table
            IEnumerable<string> columnsBeforeRename = input.Data.Columns;

            // If we don't want to preserve the original order of the columns
            // to be renamed, reorder the list of columns.
            if(!input.PreserveOrder)
                columnsBeforeRename = columnsBeforeRename.Reorder(columnsToRename);

            // If we only wish to include the renamed columns in the result,
            // filter the column list accordingly.
            if(input.DiscardOtherColumns)
                columnsBeforeRename = columnsBeforeRename.Where(c => columnsToRename.Contains(c));

            // Map old column names to new ones and leave others as is.
            IEnumerable<string> columnsAfterRename = columnsBeforeRename
                                                       .Select(c => newNameOf.ContainsKey(c) ? newNameOf[c] : c);

            // Local function to select specific columns from a row and
            // return the column values as a list.
            List<dynamic> SelectRowColumnsAsValues(dynamic row)
            {
                IDictionary<string, dynamic> data = row;

                return data
                        .Where(c => columnsBeforeRename.Contains(c.Key))
                        .Select(c => c.Value)
                        .ToList();
            }

            // Create new rows as a list of values so that Table.From ends up
            // doing the renaming implicitly.
            var rows = input.Data.Rows.Select(SelectRowColumnsAsValues);

            // Return a new table from the rows and columns
            return Table.From(columnsAfterRename.ToList(), rows.ToList());
        }

        /// <summary>
        /// Reorder the columns of a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with reordered columns as a Pori.Frends.Data.Table</returns>
        public static Table ReorderColumns([PropertyTab] ReorderColumnsParameters input, CancellationToken cancellationToken)
        {
            // Check that the specified column order does not contain duplicates
            if(input.ColumnOrder.Distinct().Count() != input.ColumnOrder.Count())
                throw new ArgumentException("The same column cannot be specified more than once", "input.ColumnOrder");

            // Check that the input table contains all specified columns
            if(!input.ColumnOrder.All(c => input.Data.Columns.Contains(c)))
                throw new ArgumentException("Column list contains columns that are not in the input table.", "input.ColumnOrder");

            var columns = input.Data.Columns.Reorder(input.ColumnOrder);

            if(input.DiscardOtherColumns)
                columns = columns.Where(c => input.ColumnOrder.Contains(c));

            // Cast the input table rows to IDictionary
            var rows = input.Data.Rows.Cast<IDictionary<string, dynamic>>();

            // Return the new table
            return Table.From(columns.ToList(), rows);
        }

        /// <summary>
        /// Select only a subset of columns from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with reordered columns as a Pori.Frends.Data.Table</returns>
        public static Table SelectColumns([PropertyTab] SelectColumnsParameters input, CancellationToken cancellationToken)
        {
            // We use ReorderColumns to do the actual work.
            // Prepare input for it.
            var reorderParams = new ReorderColumnsParameters 
            { 
                Data                = input.Data,
                DiscardOtherColumns = true
            };

            // Set column order based on input parameters
            switch(input.Action)
            {
                // Keep the specified columns in the result
                case SelectColumnsAction.Keep:
                    // Preserve the order of the columns from the input table
                    if(input.PreserveOrder)
                        reorderParams.ColumnOrder = input.Data.Columns.Where(c => input.Columns.Contains(c)).ToArray();
                    // Use the specified order of the columns
                    else
                        reorderParams.ColumnOrder = input.Columns;
                    break;

                // Discard the specified columns from the result
                case SelectColumnsAction.Discard:
                    reorderParams.ColumnOrder = input.Data.Columns.Where(c => !input.Columns.Contains(c)).ToArray();
                    break;
            }

            // Let ReorderColumns do the work and return the result
            return ReorderColumns(reorderParams, cancellationToken);
        }
    }
}
