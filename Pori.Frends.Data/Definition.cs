#pragma warning disable 1591

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

    /// <summary>
    /// Possible column types for the ConvertColumns task.
    /// </summary>
    public enum ColumnType
    {
        // Use explicit values to allow adding new ones while keeping the
        // list of values in alphabetical order (for UI purposes).
        Boolean  = 0,
        DateTime = 100,
        Decimal  = 200,
        Double   = 300,
        Float    = 400,
        Int      = 500,
        Long     = 600,

        // Always have 'Custom' as last
        Custom   = 9999,
    }

    public class ColumnConversion
    {
        /// <summary>
        /// The name of the column to convert.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Column { get; set; }

        /// <summary>
        /// The data type to convert the column values to
        /// </summary>
        public ColumnType Type { get; set; }

        /// <summary>
        /// The format to use when converting values to DateTime.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings 
        /// for possible format specifiers.
        /// </summary>
        [UIHint(nameof(Type), "", ColumnType.DateTime)]
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("yyyy-MM-ddThh:mm:ss.fffZ")]
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// The function to use as a custom converter.
        /// </summary>
        [UIHint(nameof(Type), "", ColumnType.Custom)]
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, dynamic> Converter { get; set; } 
    }

    /// <summary>
    /// Parameters for the ConvertColumns task.
    /// </summary>
    public class ConvertColumnsParameters
    {
        /// <summary>
        /// The table to use as the source.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The column type conversions to perform.
        /// </summary>
        public ColumnConversion[] Conversions { get; set; }
    }

    /// <summary>
    /// Specifies how the grouped rows are included in the result of a 
    /// GroupBy task.
    /// </summary>
    public enum GroupingType
    {
        /// <summary>
        /// Produce grouped rows as is in a table.
        /// </summary>
        EntireRows,

        /// <summary>
        /// Produce selected columns of grouped rows in a table.
        /// </summary>
        SelectedColumns,

        /// <summary>
        /// Produce the values of a single column of grouped rows as an 
        /// enumerable collection.
        /// </summary>
        SingleColumn,

        /// <summary>
        /// Produce computed values for grouped rows as an enumerable
        /// collection.
        /// </summary>
        Computed
    }

    public class GroupByParameters
    {
        /// <summary>
        /// The table to use as the source.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// Names of columns to group rows by.
        /// </summary>
        [DefaultValue(new string[] {""})]
        public string[] KeyColumns { get; set; }

        /// <summary>
        /// Name of the column for the the grouped rows.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string ResultColumn { get; set; }

        /// <summary>
        /// How the grouped rows should be returned in the resulting table.
        /// </summary>
        [DefaultValue(GroupingType.EntireRows)]
        public GroupingType Grouping { get; set; }

        /// <summary>
        /// The single column to include for grouped rows.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(Grouping), "", GroupingType.SingleColumn)]
        public string Column { get; set; }

        /// <summary>
        /// The columns to include in the grouped rows.
        /// </summary>
        [UIHint(nameof(Grouping), "", GroupingType.SelectedColumns)]
        [DefaultValue(new string[] {""})]
        public string[] Columns { get; set; }

        /// <summary>
        /// Function to compute a value for each grouped row.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        [UIHint(nameof(Grouping), "", GroupingType.Computed)]
        public Func<dynamic, dynamic> ComputeValue { get; set; }
    }

    /// <summary>
    /// Whether to sort table rows in an ascending or descending order.
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// Sort table rows in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sort table rows in descending order.
        /// </summary>
        Descending
    }

    /// <summary>
    /// A single sorting criterion.
    /// </summary>
    public class SortingCriterion
    {
        /// <summary>
        /// The name of the column to use for sorting rows.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Column { get; set; }

        /// <summary>
        /// Whether to sort the rows ascending or descending.
        /// </summary>
        [DefaultValue(Order.Ascending)]
        public Order Order { get; set; }
    }

    /// <summary>
    /// Parameters for the Sort task.
    /// </summary>
    public class SortParameters
    {
        /// <summary>
        /// The table to use as the source.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The sorting criteria (column and order) to use.
        /// </summary>
        [DefaultValue(new[] {""})]
        public SortingCriterion[] SortingCriteria { get; set; }
    }

    /// <summary>
    /// Parameters for the Concatenate task.
    /// </summary>
    public class ConcatenateParameters
    {
        /// <summary>
        /// The table to use as the source.
        /// </summary>
        public Table[] Tables { get; set; }
    }

    /// <summary>
    /// How the original table should be included in the result of a join.
    /// </summary>
    public enum JoinResult
    {
        /// <summary>
        /// Include the entire matching row as a
        /// value of a single column in the result.
        /// </summary>
        Row,

        /// <summary>
        /// Include all columns of the original table
        /// as columns of the result table.
        /// </summary>
        AllColumns,

        /// <summary>
        /// Include one or more columns of the original
        /// table as columns of the result table.
        /// </summary>
        SelectColumns,

        /// <summary>
        /// Include all columns of the original table in the result, except
        /// for columns which were used as the key of the join.
        /// </summary>
        DiscardKey
    }

    public enum JoinType
    {
        /// <summary>
        /// Perform an inner join (only matching rows).
        /// </summary>
        Inner,

        /// <summary>
        /// Perform a left outer join (all rows from the left side table and
        /// only matching rows from the right side table).
        /// </summary>
        LeftOuter
    }


    public class JoinTable
    {
        /// <summary>
        /// The table to use as a source for the join operation.
        /// </summary>
        [DisplayName("Table")]
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The names columns to use as the key for the join.
        /// </summary>
        public string[] KeyColumns { get; set; }

        /// <summary>
        /// How the matching rows should be included in the result.
        /// </summary>
        public JoinResult ResultType { get; set; }

        /// <summary>
        /// Name of the column in the result table to contain the matching
        /// rows from the original table.
        /// </summary>
        [UIHint(nameof(ResultType),"", JoinResult.Row)]
        public string ResultColumn { get; set; }

        /// <summary>
        /// List of the names of the columns from
        /// the original table to include in the result.
        /// </summary>
        [UIHint(nameof(ResultType),"", JoinResult.SelectColumns)]
        public string[] ResultColumns { get; set; }
    }

    /// <summary>
    /// Parameters for the Join task.
    /// </summary>
    public class JoinParameters
    {
        /// <summary>
        /// The type of join to perform.
        /// </summary>
        public JoinType JoinType { get; set; }

        /// <summary>
        /// The left side for the join.
        /// </summary>
        public JoinTable Left { get; set; }

        /// <summary>
        /// The right side for the join.
        /// </summary>
        public JoinTable Right { get; set; }
    }
}
