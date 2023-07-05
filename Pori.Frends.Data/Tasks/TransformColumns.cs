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
    /// Specify a transformation to a table column.
    /// </summary>
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
        [UIHint(nameof(TransformType), "", ProcessingType.Column, ProcessingType.Row)]
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, dynamic> Transform { get; set; }

        /// <summary>
        /// The transform function. Receives either the entire row or the
        /// current value of the column as its argument (based on the value
        /// of TransformType) as well as the index of the row being processed.
        /// Should return a value for the column.
        /// </summary>
        [UIHint(nameof(TransformType), "", ProcessingType.ColumnWithIndex, ProcessingType.RowWithIndex)]
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, int, dynamic> IndexedTransform { get; set; }
    }

    /// <summary>
    /// Parameters for the TransformColumns task.
    /// </summary>
    public class TransformColumnsParameters
    {
        /// <summary>
        /// The table to use as the source
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The transformations to apply to the source table.
        /// </summary>
        public ColumnTransform[] Transforms { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Transform the values of one or more columns in a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with the specifed transforms applied to the rows.</returns>
        public static Table TransformColumns([PropertyTab] TransformColumnsParameters input, [PropertyTab] CommonOptions options, CancellationToken cancellationToken)
        {
            // Get the names of the columns to be transformed
            var columnNames = input.Transforms.Select(tr => tr.Column);

            // Check that the input table has all the specified columns
            if(columnNames.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified");

            // Start creating a new table using the input table as a source.
            TableBuilder builder = TableBuilder.From(input.Data);

            // Transform the columns one at a time
            foreach(var transform in input.Transforms)
            {
                Func<dynamic, int, dynamic> fn;

                switch(transform.TransformType)
                {
                    case ProcessingType.Row:
                        if(transform.Transform == null)
                            throw new ArgumentException("Transform function cannot be null");

                        fn = (row, i) => transform.Transform(row);
                        break;

                    case ProcessingType.Column:
                        if(transform.Transform == null)
                            throw new ArgumentException("Transform function cannot be null");

                        fn = TableBuilder.IndexedColumnFunction(transform.Column, transform.Transform);
                        break;

                    case ProcessingType.RowWithIndex:
                        if(transform.IndexedTransform == null)
                            throw new ArgumentException("Transform function cannot be null");

                        fn = transform.IndexedTransform;
                        break;

                    case ProcessingType.ColumnWithIndex:
                        if(transform.IndexedTransform == null)
                            throw new ArgumentException("Transform function cannot be null");

                        fn = TableBuilder.IndexedColumnFunction(transform.Column, transform.IndexedTransform);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                // Transform the values of the column using the transform
                // function.
                builder.TransformColumn(transform.Column, fn);
            }

            // Create and return the table with the transformed rows.
            return builder
                    .OnError(options.ErrorHandling)
                    .CreateTable();
        }
    }
}