using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    /// <summary>
    /// How the values for a new column formed (AddColumns task)
    /// </summary>
    public enum NewColumnValueSource
    {
        /// <summary>
        /// Use a single constant value for every row.
        /// </summary>
        Constant,

        /// <summary>
        /// Compute a value for each row using a function that receives
        /// the row as its input.
        /// </summary>
        Computed,

        /// <summary>
        /// Compute a value for each row using a function that receives
        /// the row and its index as its input.
        /// </summary>
        ComputedWithIndex
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
        /// Receives a single row as its argument and should return
        /// the value for the new column.
        /// </summary>
        [UIHint(nameof(ValueSource), "", NewColumnValueSource.Computed)]
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, dynamic> ValueGenerator { get; set; }

        /// <summary>
        /// A function for generating the value of the new column.
        /// Receives a single row as its argument and should return
        /// the value for the new column.
        /// </summary>
        [UIHint(nameof(ValueSource), "", NewColumnValueSource.ComputedWithIndex)]
        [DisplayFormat(DataFormatString = "Expression")]
        public Func<dynamic, int, dynamic> IndexedValueGenerator { get; set; }
    }

    /// <summary>
    /// Parameters for the AddColumns task.
    /// </summary>
    public class AddColumnsParameters
    {
        /// <summary>
        /// The table to use as the source
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// List of columns to add to the table.
        /// </summary>
        public NewColumn[] Columns { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Add one or more columns to a table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with the added columns.</returns>
        public static Table AddColumns([PropertyTab] AddColumnsParameters input, [PropertyTab] CommonOptions options, CancellationToken cancellationToken)
        {
            var columnNames = input.Columns.Select(c => c.Name);

            // Check that the table doesn't contain columns with the same name
            if(columnNames.Intersect(input.Data.Columns).Count() > 0)
                throw new ArgumentException("Cannot add new column with the same name as an existing column in the table.");

            // Check the new column names are unique
            if(columnNames.Distinct().Count() != columnNames.Count())
                throw new ArgumentException("Multiple new columns with the same specified.");

            TableBuilder builder = TableBuilder.From(input.Data);

            // Add the new columns one by one
            foreach(var column in input.Columns)
            {
                // Add the column to the result based on the type of source
                // to use for the value
                switch(column.ValueSource)
                {
                    case NewColumnValueSource.Constant:
                        // Wrap the provided value as a function
                        builder.AddColumn(column.Name, row => column.Value);
                        break;

                    case NewColumnValueSource.Computed:
                        builder.AddColumn(column.Name, column.ValueGenerator);
                        break;

                    case NewColumnValueSource.ComputedWithIndex:
                        builder.AddColumn(column.Name, column.IndexedValueGenerator);
                        break;
                }
            }

            return builder
                    .OnError(options.ErrorHandling)
                    .CreateTable();
        }
    }
}