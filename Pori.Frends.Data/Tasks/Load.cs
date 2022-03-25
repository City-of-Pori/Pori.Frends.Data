using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Microsoft.CSharp; // For dynamic in .NET Standard Frends Tasks
using Newtonsoft.Json.Linq;

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    /// <summary>
    /// The format of the data to be loaded as a table.
    /// </summary>
    public enum LoadFormat
    {
        /// <summary>
        /// Load data from the result of the Frends.Csv.Parse task.
        /// </summary>
        CSV,

        /// <summary>
        /// Load JSON data into a table.
        /// </summary>
        JSON
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

        [DisplayName("CSV")]
        [UIHint(nameof(Format), "", LoadFormat.CSV)]
        public LoadCsvParameters Csv { get; set; }

        [DisplayName("JSON")]
        [UIHint(nameof(Format), "", LoadFormat.JSON)]
        public LoadJsonParameters Json { get; set; }
    }

    /// <summary>
    /// Parameters for loading CSV data using the Load task.
    /// </summary>
    public class LoadCsvParameters
    {
        /// <summary>
        /// CSV data to be loaded into a table. Must be in the format returned by Frends.Csv.Parse.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public dynamic Data { get; set; }
    }

    /// <summary>
    /// Parameters for loading JSON data using the Load task.
    /// </summary>
    public class LoadJsonParameters
    {
        /// <summary>
        /// The JSON data to load into a table. Must be a JArray of JObjects.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public dynamic Data { get; set; }

        /// <summary>
        /// The names of properties to include as columns in the resulting
        /// table.
        /// </summary>
        public string[] Columns { get; set; }
    }
    

    /// <summary>
    /// Frends task for loading data into a table.
    /// </summary>
    public static class LoadTask
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
                    var headers = input.Csv.Data.Headers as List<string>;
                    var data    = input.Csv.Data.Data as List<List<object>>;

                    // Create a table using the data
                    return Table.From(headers, data);


                case LoadFormat.JSON:
                    var rows = (input.Json.Data as JArray).Cast<JObject>();

                    return Table.From(input.Json.Columns, rows);


                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}