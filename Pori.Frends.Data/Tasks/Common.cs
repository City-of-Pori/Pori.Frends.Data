using System.ComponentModel;

namespace Pori.Frends.Data
{
    /// <summary>
    /// The type of input to provide a function processing a table's rows.
    /// </summary>
    public enum ProcessingType
    {
        /// <summary>
        /// Process rows using the entire row.
        /// </summary>
        Row,

        /// <summary>
        /// Process rows using the value of single column.
        /// </summary>
        Column,

        /// <summary>
        /// Process rows using the entire row and its index in the table.
        /// </summary>
        RowWithIndex,

        /// <summary>
        /// Process rows using the value of a single column and the index of
        /// the row in the table.
        /// </summary>
        ColumnWithIndex
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
    /// Whether to (de)serialize a table from/to a file or a string value.
    /// </summary>
    public enum SerializationType
    {
        /// <summary>
        /// (De)serialize the table to/from a file.
        /// </summary>
        File,

        /// <summary>
        /// (De)serialize the table to/from a string.
        /// </summary>
        String
    }

    /// <summary>
    /// Common options for multiple tasks.
    /// </summary>
    [DisplayName("Options")]
    public class CommonOptions
    {
        /// <summary>
        /// How to handle errors encountered when executing the task.
        /// </summary>
        [DefaultValue(Table.ErrorHandling.Fail)]
        public Table.ErrorHandling ErrorHandling { get; set; }

        /// <summary>
        /// Default values for the options.
        /// </summary>
        public static readonly CommonOptions Defaults = new CommonOptions
        {
            ErrorHandling = Table.ErrorHandling.Fail
        };
    }
}
