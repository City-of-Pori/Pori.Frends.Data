﻿#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Pori.Frends.Data
{
    using FilterFunc = Func<dynamic, bool>;

    public enum LoadFormat
    {
        CSV
    }

    /// <summary>
    /// Parameters for loading data into a Pori.Frends.Data.Table.
    /// </summary>
    [DisplayName("Input")]
    public class LoadParameters
    {
        /// <summary>
        /// The format of the input data.
        /// </summary>
        [DefaultValue(LoadFormat.CSV)]
        public LoadFormat Format { get; set; }

        /// <summary>
        /// CSV data to be loaded into a table. Must be in the format returned by Frends.Csv.Parse.
        /// </summary>
        [DisplayName("CSV Data")]
        [DisplayFormat(DataFormatString = "Expression")]
        [UIHint(nameof(Format), "", LoadFormat.CSV)]
        public dynamic CsvData { get; set; }
    }

    /// <summary>
    /// The type of input to provide a function processing a table's rows.
    /// </summary>
    public enum ProcessingType
    {
        Row,
        Column
    }

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

    /// <summary>
    /// Parameters for the ReorderColumns task.
    /// </summary>
    public class ReorderColumnsParameters
    {
        /// <summary>
        /// The table whose columns are to be reordered.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }


        /// <summary>
        /// The new order for the columns of the table. Columns that are not 
        /// specified are not reodered (unless DiscardOtherColumns is true, 
        /// in which case unspecified columns are not included in the 
        /// resulting table).
        /// </summary>
        [DisplayName("New Column Order")]
        public string[] ColumnOrder { get; set; }

        /// <summary>
        /// Whether to discard columns that are not specified in the column 
        /// order from the resulting table.
        /// </summary>
        [DisplayName("Discard Other Columns?")]
        [DefaultValue(true)]
        public bool DiscardOtherColumns { get; set; }
    }

    public class ColumnRename
    {
        public string Column { get; set; }

        [DisplayName("New Column Name")]
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

    /// <summary>
    /// Specifies whether columns provided to the SelectColumns task should 
    /// be kept or discarded.
    /// </summary>
    public enum SelectColumnsAction
    {
        Keep,
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
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// Whether to keep or discard the specified columns.
        /// </summary>
        public SelectColumnsAction Action { get; set; }

        /// <summary>
        /// The columns to include in / discard from the result table.
        /// </summary>
        public string[] Columns { get; set; }

        /// <summary>
        /// Whether to preserve the original order of the columns.
        /// </summary>
        [DisplayName("Preserve Column Order")]
        [UIHint(nameof(Action), "", SelectColumnsAction.Keep)]
        [DefaultValue(false)]
        public bool PreserveOrder { get; set; }
    }

    /// <summary>
    /// How the values for a new column formed (AddColumns task)
    /// </summary>
    public enum NewColumnValueSource
    {
        Constant,
        Computed
    }

    /// <summary>
    /// A definition for a new table column for adding new columns to a table 
    /// using the AddColumns task.
    /// </summary>
    public class NewColumn
    {
        /// <summary>
        /// The name of the new column.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Name { get; set; }

        /// <summary>
        /// How the values for the new column are generated.
        /// </summary>
        [DefaultValue(NewColumnValueSource.Constant)]
        public NewColumnValueSource ValueSource { get; set; }

        /// <summary>
        /// The constant value for the column.
        /// </summary>
        [UIHint(nameof(ValueSource), "", NewColumnValueSource.Constant)]
        [DisplayFormat(DataFormatString = "Expression")]
        public dynamic Value { get; set; }


        /// <summary>
        /// A function for generating the value of the new column.
        /// Receives a single row as its argument and should return the value 
        /// for the new column.
        /// </summary>
        [UIHint(nameof(ValueSource), "", NewColumnValueSource.Computed)]
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, dynamic> ValueGenerator { get; set; }
    }

    /// <summary>
    /// Parameters for the AddColumns task.
    /// </summary>
    public class AddColumnsParameters
    {
        /// <summary>
        /// The table to use as the source
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// List of columns to add to the table
        /// </summary>
        public NewColumn[] Columns { get; set; }
    }

    public class ColumnTransform
    {
        /// <summary>
        /// The name of the column to transform.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Column { get; set; }

        /// <summary>
        /// Whether to provide the current value of the column or the whole 
        /// row as an argument to the transform function.
        /// </summary>
        [DefaultValue(ProcessingType.Column)]
        public ProcessingType TransformType { get; set; }

        /// <summary>
        /// The transform function. Receives either the entire row or the 
        /// current value of the column as its argument (based on the value 
        /// of TransformType). Should return a value for the column.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, dynamic> Transform { get; set; }
    }

    /// <summary>
    /// Parameters for the TransformColumns task.
    /// </summary>
    public class TransformColumnsParameters
    {
        /// <summary>
        /// The table to use as the source
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The transformations to apply to the source table.
        /// </summary>
        public ColumnTransform[] Transforms { get; set; }
    }
}
