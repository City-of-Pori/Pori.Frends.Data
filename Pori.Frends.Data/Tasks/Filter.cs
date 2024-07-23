using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks
using Pori.Frends.Data.Linq;

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    using FilterFunc = Func<dynamic, bool>;
    using IndexedFilterFunc = Func<dynamic, int, bool>;

    /// <summary>
    /// Parameters for filtering rows from a Pori.Frends.Data.Table
    /// </summary>
    public class FilterParameters
    {
        /// <summary>
        /// The table to filter.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        [DefaultValue("#result")]
        public Table Data { get; set; }

        /// <summary>
        /// Whether to filter rows based on the entire row or a single column.
        /// </summary>
        [DefaultValue(ProcessingType.Row)]
        public ProcessingType FilterType { get; set; }

        /// <summary>
        /// Column to use as the input for the filter function
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(FilterType), "", ProcessingType.Column)]
        public string FilterColumn { get; set; }

        /// <summary>
        /// Filter function to select the rows to include in the result.
        /// Only rows for which the function returns 'true' are included in the result.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        [UIHint(nameof(FilterType), "", ProcessingType.Column, ProcessingType.Row)]
        public FilterFunc Filter { get; set; }

        /// <summary>
        /// Filter function to select the rows to include in the result.
        /// Only rows for which the function returns 'true' are included in the result.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        [UIHint(nameof(FilterType), "", ProcessingType.ColumnWithIndex, ProcessingType.RowWithIndex)]
        public IndexedFilterFunc IndexedFilter { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Filter rows from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Filtered data as a Pori.Frends.Data.Table</returns>
        public static Table Filter([PropertyTab] FilterParameters input, [PropertyTab] CommonOptions options, CancellationToken cancellationToken)
        {
            IndexedFilterFunc filter;

            // Select / create an appropriate filter function
            switch(input.FilterType)
            {
                case ProcessingType.Row:
                    filter = (row, index) => input.Filter(row);
                    break;

                case ProcessingType.Column:
                    filter = TableBuilder.IndexedColumnFunction(input.FilterColumn, input.Filter);
                    break;

                case ProcessingType.RowWithIndex:
                    filter = input.IndexedFilter;
                    break;

                case ProcessingType.ColumnWithIndex:
                    filter = TableBuilder.IndexedColumnFunction(input.FilterColumn, input.IndexedFilter);
                    break;

                default:
                    filter = default;
                    break;
            }

            return TableBuilder
                    .From(input.Data)   // Use the input table as the source
                    .Filter(filter)     // Filter the rows
                    .OnError(options.ErrorHandling)
                    .CreateTable();     // Create the resulting table
        }
    }
}