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
    /// Specifies whether columns provided to the SelectColumns task should 
    /// be kept or discarded.
    /// </summary>
    public enum SelectColumnsAction
    {
        /// <summary>
        /// Keep the specified columns in the result.
        /// </summary>
        Keep,

        /// <summary>
        /// Discard the specified columns from the result.
        /// </summary>
        Discard
    }

    /// <summary>
    /// Parameters for the SelectColumns task.
    /// </summary>
    public class SelectColumnsParameters
    {
        /// <summary>
        /// The table whose columns are to be reordered.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        [DefaultValue("#result")]
        public Table Data { get; set; }

        /// <summary>
        /// Whether to keep or discard the specified columns.
        /// </summary>
        public SelectColumnsAction Action { get; set; }

        /// <summary>
        /// The columns to include in / discard from the result table.
        /// </summary>
        public IEnumerable<string> Columns { get; set; }

        /// <summary>
        /// Whether to preserve the original order of the columns or use
        /// the order the columns to keep are specified in.
        /// </summary>
        [UIHint(nameof(Action), "", SelectColumnsAction.Keep)]
        [DefaultValue(false)]
        public bool PreserveColumnOrder { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Select only a subset of columns from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with reordered columns as a Pori.Frends.Data.Table</returns>
        public static Table SelectColumns([PropertyTab] SelectColumnsParameters input, CancellationToken cancellationToken)
        {
            IEnumerable<string> columns;

            if(input.Columns.Distinct().Count() != input.Columns.Count())
                throw new ArgumentException("Same column specified more than once.");

            // Set column order based on input parameters
            switch(input.Action)
            {
                // Keep the specified columns in the result
                case SelectColumnsAction.Keep:
                    if(input.Columns.Any(c => !input.Data.Columns.Contains(c)))
                        throw new ArgumentException("Invalid columns specified.");

                    // Preserve the order of the columns from the input table
                    if(input.PreserveColumnOrder)
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