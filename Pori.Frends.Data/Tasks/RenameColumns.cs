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
    /// Specify a column rename.
    /// </summary>
    public class ColumnRename
    {
        /// <summary>
        /// Name of the column to rename.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Column { get; set; }

        /// <summary>
        /// The new name for the column.
        /// </summary>
        [DisplayName("New Column Name")]
        [DisplayFormat(DataFormatString = "Text")]
        public string NewName { get; set; }
    }

    /// <summary>
    /// Parameters for the RenameColumns task.
    /// </summary>
    public class RenameColumnsParameters
    {
        /// <summary>
        /// The table whose columns are to be renamed.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The columns to rename.
        /// </summary>
        [DisplayName("Column Names")]
        public ColumnRename[] Renamings { get; set; }

        /// <summary>
        /// Whether to preserve the original order of the columns.
        /// </summary>
        [DisplayName("Preserve Column Order")]
        [DefaultValue(true)]
        public bool PreserveOrder { get; set; }

        /// <summary>
        /// Whether to discard columns that are not specified in the column 
        /// name mapping.
        /// </summary>
        [DisplayName("Discard Other Columns?")]
        [DefaultValue(true)]
        public bool DiscardOtherColumns { get; set; }
    }


    public class RenameColumnsTask
    {
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
    }
}