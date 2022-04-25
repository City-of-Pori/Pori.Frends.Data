using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.XPath;
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
        JSON,

        /// <summary>
        /// Load XML data into a table.
        /// </summary>
        XML
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
    public class XmlColumnSource<TPath>
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
        public TPath ColumnPath { get; set; } = default;

        /// <summary>
        /// XPath expression to extract the name of the column.
        /// The expression should be relative to the element extracted using ColumnPath.
        /// </summary>
        [UIHint(nameof(Type), "", XmlColumnSourceType.MultipleColumns)]
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
    public class XmlColumnSource : XmlColumnSource<string> { };

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
        public string[] Columns { get; set; }

        /// <summary>
        /// Definitions for extracting column names and values from the data.
        /// </summary>
        public XmlColumnSource[] ColumnSources { get; set; }
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
                    var rows = input.Json.Data as JArray;

                    return TableBuilder
                            .Load(input.Json.Columns, rows, row => (row as JObject).ToObject<ExpandoObject>())
                            .OnError(options.ErrorHandling)
                            .CreateTable();

                case LoadFormat.XML:
                    return LoadXml(input.Xml, options.ErrorHandling);


                default:
                    throw new InvalidEnumArgumentException();
            }
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
                    Type           = src.Type,
                    ValueType      = src.ValueType,
                    ColumnName     = src.ColumnName,
                    ColumnPath     = XPathExpression.Compile(src.ColumnPath ?? "."),
                    ColumnNamePath = XPathExpression.Compile(src.ColumnNamePath ?? "."),
                    ValuePath      = XPathExpression.Compile(src.ValuePath ?? ".")
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
                        columnName = column
                                        .Select(source.ColumnNamePath)
                                        .Cast<dynamic>()
                                        .First()
                                        .Value as string;

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
    }
}