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
    using TableFunc = Func<dynamic, dynamic>;

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
        String   = 700,

        // Always have 'Custom' as last
        Custom   = 9999,
    }

    /// <summary>
    /// Type conversion specification for a table column.
    /// </summary>
    public class ColumnConversion
    {
        /// <summary>
        /// The name of the column to convert.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Column { get; set; }

        /// <summary>
        /// The data type to convert the column values to.
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
        /// Format to use when converting the column value to string.
        /// </summary>
        [UIHint(nameof(Type), "", ColumnType.String)]
        [DisplayFormat(DataFormatString = "Text")]
        public dynamic StringFormat { get; set; }

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
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The column type conversions to perform.
        /// </summary>
        public ColumnConversion[] Conversions { get; set; }
    }


    public class ConvertColumnsTask
    {
        /// <summary>
        /// Convert the values of one or more columns in a table to specific type.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with the specifed transforms applied to the rows.</returns>
        public static Table ConvertColumns([PropertyTab] ConvertColumnsParameters input, [PropertyTab] CommonOptions options, CancellationToken cancellationToken)
        {
            // Get the names of the columns to be transformed
            var columnNames = input.Conversions.Select(tr => tr.Column);

            // Check that the input table has all the specified columns
            if(columnNames.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified");

            // Start creating a new table using the input table as a source.
            TableBuilder builder = TableBuilder.From(input.Data);

            // Perform the conversions one column at a time
            foreach(var conv in input.Conversions)
                builder.TransformColumn(conv.Column, ConverterFor(conv));

            // Create and return the table with the transformed rows.
            return builder
                    .OnError(options.ErrorHandling)
                    .CreateTable();
        }

        /// <summary>
        /// Select a conversion function for a given column type.
        /// </summary>
        /// <param name="conv">
        /// The column conversion specification for which a converter is returned.
        /// </param>
        /// <returns>
        /// The conversion function matching the provided column conversion.
        /// </returns>
        private static TableFunc ConverterFor(ColumnConversion conv)
        {
            TableFunc converter;

            switch(conv.Type)
            {
                case ColumnType.Custom:
                    converter = conv.Converter;
                    break;

                case ColumnType.DateTime:
                    converter = x => DateTime.ParseExact(x as string, conv.DateTimeFormat, null);
                    break;

                case ColumnType.String:
                    var fmt = conv.StringFormat;

                    if(fmt == null || (fmt is string && String.IsNullOrEmpty(fmt)))
                        converter = x => x.ToString();
                    else
                        converter = x => x.ToString(conv.StringFormat);
                    break;

                default:
                    columnConverters.TryGetValue(conv.Type, out converter);
                    break;
            }

            return TableBuilder.ColumnFunction(conv.Column, converter);
        }

        /// <summary>
        /// Predefined conversion functions for different column types.
        /// </summary>
        private static readonly Dictionary<ColumnType, TableFunc> columnConverters = new Dictionary<ColumnType, TableFunc>
        {
            { ColumnType.Boolean,  x => Convert.ToBoolean(x) },
            { ColumnType.Decimal,  x => Convert.ToDecimal(x) },
            { ColumnType.Double,   x => Convert.ToDouble(x)  },
            { ColumnType.Float,    x => Convert.ToSingle(x)  },
            { ColumnType.Long,     x => Convert.ToInt64(x)   },
            { ColumnType.Int,      x => Convert.ToInt32(x)   },
        };
    }
}