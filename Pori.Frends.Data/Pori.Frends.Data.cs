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
    using RowDict = IDictionary<string, dynamic>;
    using FilterFunc = Func<dynamic, bool>;
    using TableFunc = Func<dynamic, dynamic>;

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
        /// Concatenate one or more tables together. All tables must have
        /// exactly the same columns in the same order.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with all the input tables' rows concatenated.</returns>
        public static Table Concatenate([PropertyTab] ConcatenateParameters input, CancellationToken cancellationToken)
        {
            // Separate the first table from the input tables
            var first = input.Tables.First();
            var rest  = input.Tables.Skip(1);

            // Check that all tables have the same columns in the same order.
            if(rest.Any(table => !table.Columns.SequenceEqual(first.Columns)))
                throw new ArgumentException("All tables have to have exactly the same columns");

            // Create the result table
            return TableBuilder
                    .From(first)
                    .Concatenate(rest)
                    .CreateTable();
        }

        /// <summary>
        /// Convert the values of one or more columns in a table to specific type.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with the specifed transforms applied to the rows.</returns>
        public static Table ConvertColumns([PropertyTab] ConvertColumnsParameters input, CancellationToken cancellationToken)
        {
            // Get the names of the columns to be transformed
            var columnNames = input.Conversions.Select(tr => tr.Column);

            // Check that the input table has all the specified columns
            if(columnNames.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified");

            // Start creating a new table using the input table as a source.
            TableBuilder builder = TableBuilder.From(input.Data);

            // Perform the conversions one column at a time
            foreach(var conv in input.Conversions)
                builder.TransformColumn(conv.Column, ConverterFor(conv));

            // Create and return the table with the transformed rows.
            return builder.CreateTable();
        }

        /// <summary>
        /// Select a conversion function for a given column type.
        /// </summary>
        /// <param name="conv">
        /// The column conversion specification for which a converter is returned.
        /// </param>
        /// <returns>
        /// The conversion function matching the provided column conversion.
        /// </returns>
        private static TableFunc ConverterFor(ColumnConversion conv)
        {
            TableFunc converter;

            switch(conv.Type)
            {
                case ColumnType.Custom:
                    converter = conv.Converter;
                    break;

                case ColumnType.DateTime:
                    converter = x => DateTime.ParseExact(x as string, conv.DateTimeFormat, null);
                    break;

                default:
                    columnConverters.TryGetValue(conv.Type, out converter);
                    break;
            }

            return TableBuilder.ColumnFunction(conv.Column, converter);
        }

        /// <summary>
        /// Predefined conversion functions for different column types.
        /// </summary>
        private static readonly Dictionary<ColumnType, TableFunc> columnConverters = new Dictionary<ColumnType, TableFunc>
        {
            { ColumnType.Boolean,  x => Convert.ToBoolean(x) },
            { ColumnType.Decimal,  x => Convert.ToDecimal(x) },
            { ColumnType.Double,   x => Convert.ToDouble(x)  },
            { ColumnType.Float,    x => Convert.ToSingle(x)  },
            { ColumnType.Long,     x => Convert.ToInt64(x)   },
            { ColumnType.Int,      x => Convert.ToInt32(x)   },
        };

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
        /// Group rows of a table based on the values of one or more columns.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A new table with the rows of the input table grouped by the 
        /// specified columns.
        /// </returns>
        public static Table GroupBy([PropertyTab] GroupByParameters input, CancellationToken cancellationToken)
        {
            TableFunc elementSelector;
            Func<IEnumerable<dynamic>, dynamic> resultSelector;

            // Check that the input table has all the specified columns
            if(input.KeyColumns.Contains(input.ResultColumn))
                throw new ArgumentException("The result column name cannot be one of the key columns");

            // Check that the input table has all the specified columns
            if(input.KeyColumns.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid key column specified");

            // Check that all columns to include in the grouped rows exist
            // in the table.
            if(input.Grouping == GroupingType.SelectedColumns
               && input.Columns.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified to be selected into grouped rows");

            // Check that the column specified to be used as the values for
            // each group exists in the table.
            if(input.Grouping == GroupingType.SingleColumn
               && !input.Data.Columns.Contains(input.Column))
                throw new ArgumentException("Invalid column specified to be selected as the group element");

            // Select the format of the grouped rows
            switch(input.Grouping)
            {
                // Take grouped rows as is and create a table from them.
                case GroupingType.EntireRows:
                    elementSelector = x => x;
                    resultSelector  = rows => Table.From(input.Data.Columns,
                                                         rows.Cast<RowDict>());
                    break;

                // Take grouped rows as is, and create a table from them but
                // only select certain columns.
                case GroupingType.SelectedColumns:
                    elementSelector = x => x;
                    resultSelector  = rows => Table.From(input.Columns.ToList(),
                                                         rows.Cast<RowDict>());
                    break;

                // Take only the value of the specified column and create
                // a list from them.
                case GroupingType.SingleColumn:
                    elementSelector = TableBuilder.ColumnFunction(input.Column, x => x);
                    resultSelector  = rows => rows;
                    break;

                // Create the result elements from the grouped rows using
                // the provided function and a create a list from them.
                case GroupingType.Computed:
                    elementSelector = input.ComputeValue;
                    resultSelector  = rows => rows;
                    break;

                // Should not happen. This is here to silence the compiler.
                default:
                    throw new InvalidEnumArgumentException();
            }

            // By default use the provided result column
            string resultColumn = input.ResultColumn;

            // Create and return the result table.
            return TableBuilder
                    .From(input.Data)
                    .GroupBy(input.KeyColumns, elementSelector, resultColumn, resultSelector)
                    .CreateTable();
        }

        /// <summary>
        /// Join the rows of two tables into a new table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The result of the join as a new table.</returns>
        public static Table Join([PropertyTab] JoinParameters input, CancellationToken cancellationToken)
        {
            // Create shorthand names to parts of the input.
            var left  = input.Left;
            var right = input.Right;

            // Get the list of columns to include in the result
            var leftResultColumns  = JoinResultColumns(left);
            var rightResultColumns = JoinResultColumns(right);

            // When the result is a row, we use the provided names for the join columns,
            // otherwise we use temporary column names that aren't visible to the user
            // as they are replaced when expanding the join columns
            var leftJoinColumn  = left.ResultColumn ?? "$Pori.Frends.Data.Join.Left$";
            var rightJoinColumn = right.ResultColumn ?? "$Pori.Frends.Data.Join.Right$";

            // Validate parameters for each side of the join
            ValidateJoinParameters(left);
            ValidateJoinParameters(right);

            // Check that columns to be included in the result are distinct
            if(leftResultColumns.Intersect(rightResultColumns).Count() > 0)
                throw new ArgumentException("Cannot include multiple columns with the same name in the result of a join");

            // Start building the result table
            var result = TableBuilder.From(left.Data);

            // Based on the parameters, perform either an inner or a left outer join
            if(input.JoinType == JoinType.Inner)
                result.InnerJoin(left.KeyColumns, leftJoinColumn,
                                 right.Data, right.KeyColumns, rightJoinColumn);
            else if(input.JoinType == JoinType.LeftOuter)
                result.LeftOuterJoin(left.KeyColumns, leftJoinColumn,
                                     right.Data, right.KeyColumns, rightJoinColumn);

            // If the result type is other than JoinResult.Row,
            // expand the join columns
            if(left.ResultType != JoinResult.Row)
                result.ExpandColumn(leftJoinColumn, leftResultColumns);
            if(right.ResultType != JoinResult.Row)
                result.ExpandColumn(rightJoinColumn, rightResultColumns);

            // Make sure columns from the left table are always before
            // columns from the inner table
            if(left.ResultType != JoinResult.Row && right.ResultType == JoinResult.Row)
                result.ReorderColumns(leftResultColumns.Concat(new[] { rightJoinColumn }));

            // Create and return the resulting table
                return result.CreateTable();
        }

        /// <summary>
        /// Build the list of columns to include in the joined table.
        /// </summary>
        /// <param name="table">The information about one of the sides of the join.</param>
        /// <returns>The list of columns to include in the joined table.</returns>
        private static IEnumerable<string> JoinResultColumns(JoinTable table)
        {
            // Select the columns to include in the result of the join
            switch(table.ResultType)
            {
                // Result should have all the columns of the source table
                case JoinResult.AllColumns:
                    return table.Data.Columns;

                // Result should only specified columns of the source table
                case JoinResult.SelectColumns:
                    return table.ResultColumns;

                // Result should have all columns except the key columns
                case JoinResult.DiscardKey:
                    return table.Data.Columns.Where(c => !table.KeyColumns.Contains(c));

                // Result should have the matching rows as values of a new column
                case JoinResult.Row:
                    return new[] { table.ResultColumn };

                default: // Should not occur. Here to silence the compiler
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Validate the parameters for a table to be joined with another table.
        /// </summary>
        /// <param name="joinable">The information about one of the sides of the join.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateJoinParameters(JoinTable joinable)
        {
            // Check that a result column name is provided when the result type is Row
            if(joinable.ResultType == JoinResult.Row && string.IsNullOrEmpty(joinable.ResultColumn))
                throw new ArgumentException("Result column name must be specified for a join.");

            // Check that at least one column is specified when only
            // selected columns should be included in the result.
            if(joinable.ResultType == JoinResult.SelectColumns && joinable.ResultColumns.Count() == 0)
                throw new ArgumentException("At least one expanded column must be specified for a join.");

            // Check that all columns to be included in the result exist in the original table
            if(joinable.ResultType == JoinResult.SelectColumns && joinable.ResultColumns.Any(c => !joinable.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified to be included in join result.");

            // Check that all key columns exist in the original table
            if(joinable.KeyColumns.Any(c => !joinable.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid key column specified for join");
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

        /// <summary>
        /// Sort the rows of a table by one or more columns.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with sorted rows.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Table Sort([PropertyTab] SortParameters input, CancellationToken cancellationToken)
        {
            // Get the names of the columns to be transformed
            var columnNames = input.SortingCriteria.Select(column => column.Column);

            // Check that the input table has all the specified columns
            if(columnNames.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified");

            // Function for converting the input criterion (column name and order) to
            // the TableBuilder equivalent (key selector function and order)
            TableBuilder.SortingCriterion ConvertSortingCriterion(SortingCriterion inputCriterion)
            {
                return new TableBuilder.SortingCriterion
                {
                    // Key is a function that extracts the value of the specific column
                    KeySelector = TableBuilder.ColumnFunction(inputCriterion.Column, x => x),
                    // Order or the sort for this criterion
                    Order = inputCriterion.Order
                };
            }

            // Convert the input sorting criteria to the ones accepted by TableBuilder
            var criteria = input.SortingCriteria.Select(ConvertSortingCriterion);

            return TableBuilder
                    .From(input.Data)   // Create the table from the input table
                    .Sort(criteria)     // Sort using the criteria
                    .CreateTable();
        }

        /// <summary>
        /// Transform the values of one or more columns in a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with the specifed transforms applied to the rows.</returns>
        public static Table TransformColumns([PropertyTab] TransformColumnsParameters input, CancellationToken cancellationToken)
        {
            // Get the names of the columns to be transformed
            var columnNames = input.Transforms.Select(tr => tr.Column);

            // Check that the input table has all the specified columns
            if(columnNames.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified");

            // Start creating a new table using the input table as a source.
            TableBuilder builder = TableBuilder.From(input.Data);

            // Transform the columns one at a time
            foreach(var transform in input.Transforms)
            {
                Func<dynamic, dynamic> fn = transform.Transform;

                // If a constant value was specified as the new value for the
                // column, wrap it as a function returning the constant value.
                if(transform.TransformType == ProcessingType.Column)
                    fn = TableBuilder.ColumnFunction(transform.Column, fn);

                // Transform the values of the column using the transform
                // function.
                builder.TransformColumn(transform.Column, fn);
            }

            // Create and return the table with the transformed rows.
            return builder.CreateTable();
        }
    }
}
