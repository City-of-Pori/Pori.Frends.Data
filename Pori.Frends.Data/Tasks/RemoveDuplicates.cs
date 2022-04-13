using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    /// <summary>
    /// What to use as the key for removing duplicate rows from a table.
    /// </summary>
    public enum RemoveDuplicatesKey
    {
        /// <summary>
        /// Use the entire row (all columns) as the key for matching duplicate rows.
        /// </summary>
        EntireRows,

        /// <summary>
        /// Use specific columns as the key for matching duplicate rows.
        /// </summary>
        SelectedColumns
    }

    /// <summary>
    /// Parameters for the RemoveDuplicates task.
    /// </summary>
    public class RemoveDuplicatesParameters
    {
        /// <summary>
        /// The table to use as the source.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// What to use as the key for matching duplicate rows.
        /// </summary>
        [DefaultValue(RemoveDuplicatesKey.EntireRows)]
        public RemoveDuplicatesKey Key { get; set; }

        /// <summary>
        /// Names of columns to use determine duplicate rows.
        /// </summary>
        [UIHint(nameof(Key), "", RemoveDuplicatesKey.SelectedColumns)]
        public string[] KeyColumns { get; set; }
    }

    public static class RemoveDuplicatesTask
    {
        /// <summary>
        /// Remove duplicate rows from a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with duplicate rows removed.</returns>
        public static Table RemoveDuplicates([PropertyTab] RemoveDuplicatesParameters input, CancellationToken cancellationToken)
        {
            if(input.Key == RemoveDuplicatesKey.EntireRows)
                input.KeyColumns = input.Data.Columns.ToArray();

            // Check that the input table contains all the specified key columns
            if(input.KeyColumns.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column name specified");

            return TableBuilder
                    .From(input.Data)
                    .RemoveDuplicates(input.KeyColumns)
                    .CreateTable();
        }
    }
}
