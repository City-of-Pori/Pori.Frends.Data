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
    /// <summary>
    /// Parameters for the ReorderColumns task.
    /// </summary>
    public class ReorderColumnsParameters
    {
        /// <summary>
        /// The table whose columns are to be reordered.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }


        /// <summary>
        /// The new order for the columns of the table. Columns that are not 
        /// specified are not reodered (unless DiscardOtherColumns is true, 
        /// in which case unspecified columns are not included in the 
        /// resulting table).
        /// </summary>
        public string[] ColumnOrder { get; set; }

        /// <summary>
        /// Whether to discard columns that are not specified in the column 
        /// order from the resulting table.
        /// </summary>
        [DefaultValue(false)]
        public bool DiscardOtherColumns { get; set; }
    }
    

    public static partial class TableTasks
    {
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
    }
}