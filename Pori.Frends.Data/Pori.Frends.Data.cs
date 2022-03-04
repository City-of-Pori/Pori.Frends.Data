using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks

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
    }
}
