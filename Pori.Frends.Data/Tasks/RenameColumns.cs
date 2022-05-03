using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks
using Newtonsoft.Json.Linq;
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
        [DisplayFormat(DataFormatString = "Text")]
        public string NewName { get; set; }
    }

    /// <summary>
    /// How the column renamings are provided to the RenameColumns task.
    /// </summary>
    public enum RenameFormat
    {
        /// <summary>
        /// Provide the column renamings as "inline" parameters to the
        /// RenameColumns task.
        /// </summary>
        Manual,

        /// <summary>
        /// Provide the column renamings as a JSON object.
        /// </summary>
        JSON
    }

    /// <summary>
    /// Parameters for the RenameColumns task.
    /// </summary>
    public class RenameColumnsParameters
    {
        /// <summary>
        /// The table whose columns are to be renamed.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// How the renamings are provided.
        /// </summary>
        [DefaultValue(RenameFormat.Manual)]
        public RenameFormat Format { get; set; }

        /// <summary>
        /// The columns to rename.
        /// </summary>
        [UIHint(nameof(Format), "", RenameFormat.Manual)]
        public ColumnRename[] Renamings { get; set; }

        /// <summary>
        /// The columns to rename as a JSON object. The object's property
        /// names are used as the current column name and the property
        /// values as the new column name.
        /// </summary>
        [UIHint(nameof(Format), "", RenameFormat.JSON)]
        [DisplayFormat(DataFormatString = "Json")]
        public string JsonRenamings { get; set; }

        /// <summary>
        /// Whether to preserve the original order of the columns.
        /// </summary>
        [DefaultValue(true)]
        public bool PreserveColumnOrder { get; set; }

        /// <summary>
        /// Whether to discard columns that are not specified in the column 
        /// name mapping.
        /// </summary>
        [DefaultValue(false)]
        public bool DiscardOtherColumns { get; set; }

        /// <summary>
        /// Whether to ignore any invalid column names specified to be
        /// renamed. If set to true, renamings referencing invalid column
        /// names will not affect the result table in any way.
        /// </summary>
        [DefaultValue(false)]
        public bool IgnoreInvalidColumnNames { get; set; }
    }


    public static partial class TableTasks
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
            Dictionary<string, string> newNameOf;

            // List of columns to rename (in order)
            IEnumerable<string> columnsToRename;

            // Get the renamings based on the input format
            if(input.Format == RenameFormat.JSON)
            {
                // Convert the renamings to JObject and get the properties.
                var renamings = JObject.Parse(input.JsonRenamings).Properties();

                newNameOf       = renamings.ToDictionary(prop => prop.Name, prop => prop.Value.ToString());
                columnsToRename = renamings.Select(prop => prop.Name);
            }
            else
            {
                newNameOf       = input.Renamings.ToDictionary(r => r.Column, r => r.NewName);
                columnsToRename = input.Renamings.Select(r => r.Column);
            }

            // Remove invalid column names from the list if we wish to ignore them
            if(input.IgnoreInvalidColumnNames)
                columnsToRename = columnsToRename.Where(c => input.Data.Columns.Contains(c));

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
            if(!input.PreserveColumnOrder)
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