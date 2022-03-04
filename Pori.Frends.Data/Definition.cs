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
}
