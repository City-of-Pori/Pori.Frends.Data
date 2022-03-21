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

    /// <summary>
    /// Parameters for filtering rows from a Pori.Frends.Data.Table
    /// </summary>
    [DisplayName("Input")]
    public class FilterParameters
    {
        /// <summary>
        /// The table to filter.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// Whether to filter rows based on the entire row or a single column.
        /// </summary>
        [DisplayName("Filter Type")]
        [DefaultValue(ProcessingType.Row)]
        public ProcessingType FilterType { get; set; }

        /// <summary>
        /// Column to use as the input for the filter function
        /// </summary>
        [DisplayName("Filter Column")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(FilterType), "", ProcessingType.Column)]
        public string FilterColumn { get; set; }

        /// <summary>
        /// Filter function to select the rows to include in the result. 
        /// Only rows for which the function returns 'true' are included in the result.
        /// </summary>
        [DisplayName("Filter")]
        [DisplayFormat(DataFormatString = "Expression")]
        public FilterFunc Filter { get; set; }
    }


    public class FilterTask
    {
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
    }
}