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
    using FilterFunc = Func<dynamic, bool>;

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
        /// Add one or more columns to a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with the added columns.</returns>
        public static Table AddColumns([PropertyTab] AddColumnsParameters input, CancellationToken cancellationToken)
        {
            var columnNames = input.Columns.Select(c => c.Name);

            // Check that the table doesn't contain columns with the same name
            if(columnNames.Intersect(input.Data.Columns).Count() > 0)
                throw new ArgumentException("Cannot add new column with the same name as an existing column in the table.");

            // Check the new column names are unique
            if(columnNames.Distinct().Count() != columnNames.Count())
                throw new ArgumentException("Multiple new columns with the same specified.");

            TableBuilder builder = TableBuilder.From(input.Data);

            // Add the new columns one by one
            foreach(var column in input.Columns)
            {
                Func<dynamic, dynamic> generator;

                // If a constant value was specified as the new value for the
                // column, wrap it as a function returning the constant value.
                if(column.ValueSource == NewColumnValueSource.Constant)
                    generator = row => column.Value;
                else
                    generator = column.ValueGenerator;

                // Add the column to the result
                builder.AddColumn(column.Name, generator);
            }

            return builder.CreateTable();
        }

        /// <summary>
        /// Filter rows from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Filtered data as a Pori.Frends.Data.Table</returns>
        public static Table Filter([PropertyTab] FilterParameters input, CancellationToken cancellationToken)
        {
            FilterFunc filter;

            // Wrap the filter function if the filter is to be applied to
            // values of a single column.
            if(input.FilterType == ProcessingType.Column)
                filter = TableBuilder.ColumnFunction(input.FilterColumn, input.Filter);
            else
                filter = input.Filter;

            return TableBuilder
                    .From(input.Data)   // Use the input table as the source
                    .Filter(filter)     // Filter the rows
                    .CreateTable();     // Create the resulting table
        }

        

        /// <summary>
        /// Rename the columns of a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with reordered columns as a Pori.Frends.Data.Table</returns>
        public static Table RenameColumns([PropertyTab] RenameColumnsParameters input, CancellationToken cancellationToken)
        {
            // Mapping from old column names to new ones
            Dictionary<string, string> newNameOf = input.Renamings.ToDictionary(r => r.Column, r => r.NewName);

            // List of columns to rename (in order)
            IEnumerable<string> columnsToRename = input.Renamings.Select(r => r.Column);

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

            return TableBuilder
                    .From(input.Data)
                    .SelectColumns(columnsBeforeRename)
                    .RenameColumns(newNameOf)
                    .CreateTable();
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

            // Reorder the columns
            var builder = TableBuilder
                            .From(input.Data)
                            .ReorderColumns(input.ColumnOrder);

            // Discard other columns if so requested
            if(input.DiscardOtherColumns)
                builder.SelectColumns(input.ColumnOrder);

            // Create and return the resulting table.
            return builder.CreateTable();
        }

        /// <summary>
        /// Select only a subset of columns from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with reordered columns as a Pori.Frends.Data.Table</returns>
        public static Table SelectColumns([PropertyTab] SelectColumnsParameters input, CancellationToken cancellationToken)
        {
            IEnumerable<string> columns;

            // Set column order based on input parameters
            switch(input.Action)
            {
                // Keep the specified columns in the result
                case SelectColumnsAction.Keep:
                    // Preserve the order of the columns from the input table
                    if(input.PreserveOrder)
                        columns = input.Data.Columns.Where(c => input.Columns.Contains(c));
                    // Use the specified order of the columns
                    else
                        columns = input.Columns;
                    break;

                // Discard the specified columns from the result
                case SelectColumnsAction.Discard:
                    columns = input.Data.Columns.Where(c => !input.Columns.Contains(c));
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            // Create the new table
            return TableBuilder
                    .From(input.Data)
                    .SelectColumns(columns)
                    .CreateTable();
        }
    }
}
