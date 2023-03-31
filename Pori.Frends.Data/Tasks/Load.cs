using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using System.Xml.XPath;
using Microsoft.CSharp; // For dynamic in .NET Standard Frends Tasks
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    using RowDict = IDictionary<string, dynamic>;

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
        JSON,

        /// <summary>
        /// Load XML data into a table.
        /// </summary>
        XML,

        /// <summary>
        /// Load table rows into a new table.
        /// </summary>
        Rows,

        /// <summary>
        /// Load table from data produced using the Serialize task.
        /// </summary>
        Serialized,

        /// <summary>
        /// Load custom data into a table.
        /// </summary>
        Custom = 9999
    }

    /// <summary>
    /// Parameters for loading data into a Pori.Frends.Data.Table.
    /// </summary>
    public class LoadParameters
    {
        /// <summary>
        /// The format of the input data.
        /// </summary>
        [DefaultValue(LoadFormat.CSV)]
        public LoadFormat Format { get; set; }

        [UIHint(nameof(Format), "", LoadFormat.CSV)]
        public LoadCsvParameters Csv { get; set; }

        [UIHint(nameof(Format), "", LoadFormat.JSON)]
        public LoadJsonParameters Json { get; set; }

        [UIHint(nameof(Format), "", LoadFormat.XML)]
        public LoadXmlParameters Xml { get; set; }

        [UIHint(nameof(Format), "", LoadFormat.Rows)]
        public LoadRowsParameters Rows { get; set; }

        [UIHint(nameof(Format), "", LoadFormat.Serialized)]
        public LoadSerializedParameters Serialized { get; set; }

        [UIHint(nameof(Format), "", LoadFormat.Custom)]
        public LoadCustomParameters Custom { get; set; }
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
        public IEnumerable<string> Columns { get; set; }
    }


    /// <summary>
    /// Whether a column source applies to a single column or multiple columns.
    /// </summary>
    public enum XmlColumnSourceType
    {
        SingleColumn,
        MultipleColumns
    }

    /// <summary>
    /// Whether a column has a single value or multiple values per row.
    /// </summary>
    public enum XmlColumnValueType
    {
        SingleValue,
        MultipleValues
    }

    /// <summary>
    /// A source definition for a table column.
    /// </summary>
    /// <typeparam name="TPath">Type for XPath expressions. Should be either 'string' or 'XPathExpression'. </typeparam>
    internal class XmlColumnSource<TPath>
        where TPath : XPathExpression
    {
        /// <summary>
        /// Whether to define a source for a single column or multiple columns.
        /// </summary>
        public XmlColumnSourceType Type { get; set; }

        /// <summary>
        /// Whether the column has a single value or multiple values.
        /// </summary>
        public XmlColumnValueType ValueType { get; set; }

        /// <summary>
        /// Name of the column to extract (when extracting a single column).
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// XPath expression to extract a column for a single row.
        /// The expression should be relative to a single row inside the data.
        /// The result of the expression should contain the column's name and value.
        /// </summary>
        public TPath ColumnPath { get; set; } = default;

        /// <summary>
        /// XPath expression to extract the name of the column.
        /// The expression should be relative to the element extracted using ColumnPath.
        /// </summary>
        public TPath ColumnNamePath { get; set; } = default;

        /// <summary>
        /// XPath expression for extracting the value(s) for the column.
        /// The expression should be relative to the element extracted using ColumnPath.
        /// </summary>
        public TPath ValuePath { get; set; } = default;
    }

    /// <summary>
    /// Column source definition for loading XML data as a table.
    /// </summary>
    /// <remarks>
    /// Basically a redefinition of the generic XmlColumnSource type above,
    /// but because the Frends UI doesn't handle the generic type properly,
    /// we have to do this.
    /// </remarks>
    public class XmlColumnSource
    {
        /// <summary>
        /// Whether to define a source for a single column or multiple columns.
        /// </summary>
        [DefaultValue(XmlColumnSourceType.SingleColumn)]
        public XmlColumnSourceType Type { get; set; }

        /// <summary>
        /// Whether the column has a single value or multiple values.
        /// </summary>
        [DefaultValue(XmlColumnValueType.SingleValue)]
        public XmlColumnValueType ValueType { get; set; }

        /// <summary>
        /// Name of the column to extract (when extracting a single column).
        /// </summary>
        [UIHint(nameof(Type), "", XmlColumnSourceType.SingleColumn)]
        public string ColumnName { get; set; }

        /// <summary>
        /// XPath expression to extract a column for a single row.
        /// The expression should be relative to a single row inside the data.
        /// The result of the expression should contain the column's name and value.
        /// </summary>
        [UIHint(nameof(Type), "", XmlColumnSourceType.MultipleColumns)]
        public string ColumnPath { get; set; } = default;

        /// <summary>
        /// XPath expression to extract the name of the column.
        /// The expression should be relative to the element extracted using ColumnPath.
        /// </summary>
        [UIHint(nameof(Type), "", XmlColumnSourceType.MultipleColumns)]
        public string ColumnNamePath { get; set; } = default;

        /// <summary>
        /// XPath expression for extracting the value(s) for the column.
        /// The expression should be relative to the element extracted using ColumnPath.
        /// </summary>
        public string ValuePath { get; set; } = default;
    }

    /// <summary>
    /// XML specific parameters for the Load task.
    /// </summary>
    public class LoadXmlParameters
    {
        /// <summary>
        /// A string containing the XML data to load into a table.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// XPath expression for selecting the rows from the data.
        /// </summary>
        public string RowsPath { get; set; }

        /// <summary>
        /// Names of columns to include in the resulting table.
        /// </summary>
        public IEnumerable<string> Columns { get; set; }

        /// <summary>
        /// Definitions for extracting column names and values from the data.
        /// </summary>
        public XmlColumnSource[] ColumnSources { get; set; }
    }


    public class LoadRowsParameters
    {
        /// <summary>
        /// The rows to load into a table. The input should contain only rows from other tables.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public IEnumerable<dynamic> Data { get; set; }

        /// <summary>
        /// Names of columns to include in the resulting table.
        /// </summary>
        public IEnumerable<string> Columns { get; set; }
    }

    /// <summary>
    /// Parameters for loading a table that was serialized using the Serialize task.
    /// </summary>
    public class LoadSerializedParameters
    {
        /// <summary>
        /// Whether to deserialize the table from a file or from a string value.
        /// </summary>
        [DefaultValue(SerializationType.File)]
        public SerializationType Source { get; set; }

        /// <summary>
        /// File path to deserialize a table from. The contents of the file
        /// should have been produced using the Serialize task.
        /// </summary>
        [UIHint(nameof(Source), "", SerializationType.File)]
        public string Path { get; set; }

        /// <summary>
        /// The table data to deserialize as a table. Should have been
        /// produced by the Serialize task.
        /// </summary>
        [UIHint(nameof(Source), "", SerializationType.String)]
        [DisplayFormat(DataFormatString = "Expression")]
        public string Data { get; set; }
    }

    public class LoadCustomParameters
    {
        /// <summary>
        /// The custom data to load into a table.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public IEnumerable<dynamic> Data { get; set; }

        /// <summary>
        /// Names of columns to include in the resulting table.
        /// </summary>
        public IEnumerable<string> Columns { get; set; }

        /// <summary>
        /// Function to extract a value for a given column of a row.
        /// </summary>
        public Func<dynamic, string, dynamic> ColumnLoader { get; set; }
    }


    /// <summary>
    /// Frends task for loading data into a table.
    /// </summary>
    public static partial class TableTasks
    {
        /// <summary>
        /// Load data into a table structure for further processing.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The data as a Pori.Frends.Data.Table</returns>
        public static Table Load([PropertyTab] LoadParameters input, [PropertyTab] CommonOptions options, CancellationToken cancellationToken)
        {
            // Load data based on the input format
            switch(input.Format)
            {
                case LoadFormat.CSV:
                    // Extract the headers and data from the input
                    var headers = input.Csv.Data.Headers as List<string>;
                    var data    = input.Csv.Data.Data as List<List<object>>;

                    // Create a table using the data
                    return TableBuilder
                            .Load(headers, data)
                            .OnError(options.ErrorHandling)
                            .CreateTable();


                case LoadFormat.JSON:
                    return LoadJson(input.Json, options);


                case LoadFormat.XML:
                    return LoadXml(input.Xml, options.ErrorHandling);


                case LoadFormat.Rows:
                    return TableBuilder
                                .Load(input.Rows.Columns, input.Rows.Data, row => row as RowDict)
                                .OnError(options.ErrorHandling)
                                .CreateTable();


                case LoadFormat.Serialized:
                    return LoadSerialized(input.Serialized, options);


                case LoadFormat.Custom:
                    return LoadCustom(input.Custom, options.ErrorHandling);


                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Load JSON data (must be a JArray of JObjects) as a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <returns>The new table with the loaded data.</returns>
        private static Table LoadJson(LoadJsonParameters input, CommonOptions options)
        {
            var rows = input.Data as JArray;

            return TableBuilder
                    .Load(input.Columns, rows, row => (row as JObject).ToObject<ExpandoObject>())
                    .OnError(options.ErrorHandling)
                    .CreateTable();
        }

        /// <summary>
        /// Load XML data into a table.
        /// </summary>
        /// <param name="input">The input data and options for loading the data.</param>
        /// <param name="errorHandling">How to handle errors encountered while loading the data.</param>
        /// <returns></returns>
        private static Table LoadXml(LoadXmlParameters input, Table.ErrorHandling errorHandling)
        {
            var reader  = new StringReader(input.Data);
            var doc     = new XPathDocument(reader);
            var nav     = doc.CreateNavigator();

            var rowExpr     = XPathExpression.Compile(input.RowsPath);
            var rowElements = nav.Select(rowExpr);

            // Local function for converting a column source definition to
            // one which has the XPath expressions compiled into XPathExpression objects.
            XmlColumnSource<XPathExpression> CompileColumnSource(XmlColumnSource src)
            {
                return new XmlColumnSource<XPathExpression>
                {
                    Type = src.Type,
                    ValueType = src.ValueType,
                    ColumnName = src.ColumnName,
                    ColumnPath = XPathExpression.Compile(
                                        string.IsNullOrEmpty(src.ColumnPath) ? "." : src.ColumnPath
                                     ),
                    ColumnNamePath = XPathExpression.Compile(
                                        string.IsNullOrEmpty(src.ColumnNamePath) ? "." : $"string({src.ColumnNamePath})"
                                     ),
                    ValuePath = XPathExpression.Compile(
                                        string.IsNullOrEmpty(src.ValuePath) ? "." : src.ValuePath
                                     )
                };
            }

            // Pre-compile the XPath expressions both to validate them before
            // attempting to use them and to avoid compiling them for each
            // loaded row.
            var columnSources = input.ColumnSources
                                    .Select(CompileColumnSource)
                                    .ToList();

            var rows = rowElements.Cast<XPathNavigator>();

            return TableBuilder
                    .Load(input.Columns, rows, XmlRowLoader(columnSources))
                    .OnError(errorHandling)
                    .CreateTable();
        }

        /// <summary>
        /// Create a function for loading table row data from an XML element
        /// using the provided column sources.
        /// </summary>
        /// <param name="columnSources">The column source definitions to use.</param>
        /// <returns>A function for loading table row data.</returns>
        private static Func<XPathNavigator, IDictionary<string, dynamic>> XmlRowLoader(IEnumerable<XmlColumnSource<XPathExpression>> columnSources)
        {
            return rowElement =>
            {
                IDictionary<string, dynamic> row = new Dictionary<string, dynamic>();

                foreach(var (column, value) in LoadXmlColumns(rowElement, columnSources))
                    row[column] = value;

                return row;
            };
        }


        /// <summary>
        /// Load the columns for a single table row from an XML element.
        /// </summary>
        /// <param name="rowElement">The XML element containing the column values.</param>
        /// <param name="columnSources">Column source definitions for the columns to be loaded.</param>
        /// <returns>The column names and associated column values as pairs.</returns>
        private static IEnumerable<(string, dynamic)> LoadXmlColumns(XPathNavigator rowElement, IEnumerable<XmlColumnSource<XPathExpression>> columnSources)
        {
            string  columnName;
            dynamic value;

            foreach(var source in columnSources)
            {
                if(source.Type == XmlColumnSourceType.MultipleColumns)
                {
                    var columns = rowElement.Select(source.ColumnPath);

                    foreach(XPathNavigator column in columns)
                    {
                        columnName = (string)column.Evaluate(source.ColumnNamePath);

                        if(String.IsNullOrEmpty(columnName))
                            throw new XPathException("The result of the column name path expression is empty.");
                        /*
                        columnName = column
                                        .Select(source.ColumnNamePath)
                                        .Cast<dynamic>()
                                        .First()
                                        .Value as string;
                        */
                        value = LoadXmlValue(column, source);

                        yield return (columnName, value);
                    }
                }
                else
                {
                    columnName = source.ColumnName;
                    value = LoadXmlValue(rowElement, source);

                    yield return (columnName, value);
                }
            }
        }

        /// <summary>
        /// Load a column value for a single table row from an XML element.
        /// </summary>
        /// <param name="element">The XML element that contains the column value(s).</param>
        /// <param name="source">The column source definition to use.</param>
        /// <returns>The value for the column.</returns>
        private static dynamic LoadXmlValue(XPathNavigator element, XmlColumnSource<XPathExpression> source)
        {
            var values = element
                            .Select(source.ValuePath)
                            .Cast<dynamic>()
                            .Select(x => x.Value as string);

            if(source.ValueType == XmlColumnValueType.MultipleValues)
                return values.ToList();
            else
                return values.DefaultIfEmpty(null).First();
        }

        /// <summary>
        /// Load serialized table data as a new Table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <returns>The new table with the loaded data.</returns>
        private static Table LoadSerialized(LoadSerializedParameters input, CommonOptions options)
        {
            JToken data;

            using(var source = DeserializationSourceFor(input))
            {
                JsonReader reader = new JsonTextReader(source)
                {
                    DateParseHandling = DateParseHandling.None
                };

                data = JToken.ReadFrom(reader);
            }

            string[] columns = data["Columns"].ToObject<string[]>();
            JArray   rows    = data["Rows"] as JArray;

            var loadJsonParameters = new LoadJsonParameters
            {
                Columns = columns,
                Data    = rows
            };

            return LoadJson(loadJsonParameters, options);
        }

        /// <summary>
        /// Initialize an appropriate TextReader for deserializing a table.
        /// </summary>
        /// <param name="input">Deserialization parameters.</param>
        /// <returns>An instance of TextReader.</returns>
        private static TextReader DeserializationSourceFor(LoadSerializedParameters input)
        {
            if(input.Source == SerializationType.File)
                return new StreamReader(input.Path, encoding: Encoding.UTF8);
            else
                return new StringReader(input.Data);
        }

        /// <summary>
        /// Load custom data into a table.
        /// </summary>
        /// <param name="input">Input data and options for loading the data.</param>
        /// <param name="errorHandling">How to handle errors encountered while loading the data.</param>
        /// <returns>The resulting table.</returns>
        private static Table LoadCustom(LoadCustomParameters input, Table.ErrorHandling errorHandling)
        {
            var loader = CustomRowLoader(input.Columns.ToArray(), input.ColumnLoader);

            return TableBuilder
                    .Load(input.Columns, input.Data, loader)
                    .OnError(errorHandling)
                    .CreateTable();
        }

        /// <summary>
        /// Produce row data from custom data using the specified column loader.
        /// </summary>
        /// <param name="columns">The columns to load.</param>
        /// <param name="columnLoader">Function for extracting a value for a single column of a row.</param>
        /// <returns>A function for loading data for a single table row.</returns>
        private static Func<dynamic, IDictionary<string, dynamic>> CustomRowLoader(string[] columns, Func<dynamic, string, dynamic> columnLoader)
        {
            return source =>
            {
                IDictionary<string, dynamic> row = new Dictionary<string, dynamic>();

                foreach(var column in columns)
                    row[column] = columnLoader(source, column);

                return row;
            };
        }
    }
}
