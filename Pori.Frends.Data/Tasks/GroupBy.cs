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
    using RowDict   = IDictionary<string, dynamic>;

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

    /// <summary>
    /// Parameters for the task GroupBy.
    /// </summary>
    public class GroupByParameters
    {
        /// <summary>
        /// The table to use as the source.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// Names of columns to group rows by.
        /// </summary>
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
        public string[] Columns { get; set; }

        /// <summary>
        /// Function to compute a value for each grouped row.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        [UIHint(nameof(Grouping), "", GroupingType.Computed)]
        public Func<dynamic, dynamic> ComputeValue { get; set; }
    }


    public class GroupByTask
    {
        /// <summary>
        /// Group rows of a table based on the values of one or more columns.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A new table with the rows of the input table grouped by the 
        /// specified columns.
        /// </returns>
        public static Table GroupBy([PropertyTab] GroupByParameters input, CancellationToken cancellationToken)
        {
            TableFunc elementSelector;
            Func<IEnumerable<dynamic>, dynamic> resultSelector;

            // Check that the input table has all the specified columns
            if(input.KeyColumns.Contains(input.ResultColumn))
                throw new ArgumentException("The result column name cannot be one of the key columns");

            // Check that the input table has all the specified columns
            if(input.KeyColumns.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid key column specified");

            // Check that all columns to include in the grouped rows exist
            // in the table.
            if(input.Grouping == GroupingType.SelectedColumns
               && input.Columns.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified to be selected into grouped rows");

            // Check that the column specified to be used as the values for
            // each group exists in the table.
            if(input.Grouping == GroupingType.SingleColumn
               && !input.Data.Columns.Contains(input.Column))
                throw new ArgumentException("Invalid column specified to be selected as the group element");

            // Select the format of the grouped rows
            switch(input.Grouping)
            {
                // Take grouped rows as is and create a table from them.
                case GroupingType.EntireRows:
                    elementSelector = x => x;
                    resultSelector = rows => Table.From(input.Data.Columns,
                                                        rows.Cast<RowDict>());
                    break;

                // Take grouped rows as is, and create a table from them but
                // only select certain columns.
                case GroupingType.SelectedColumns:
                    elementSelector = x => x;
                    resultSelector = rows => Table.From(input.Columns,
                                                        rows.Cast<RowDict>());
                    break;

                // Take only the value of the specified column and create
                // a list from them.
                case GroupingType.SingleColumn:
                    elementSelector = TableBuilder.ColumnFunction(input.Column, x => x);
                    resultSelector = rows => rows;
                    break;

                // Create the result elements from the grouped rows using
                // the provided function and a create a list from them.
                case GroupingType.Computed:
                    elementSelector = input.ComputeValue;
                    resultSelector = rows => rows;
                    break;

                // Should not happen. This is here to silence the compiler.
                default:
                    throw new InvalidEnumArgumentException();
            }

            // By default use the provided result column
            string resultColumn = input.ResultColumn;

            // Create and return the result table.
            return TableBuilder
                    .From(input.Data)
                    .GroupBy(input.KeyColumns, elementSelector, resultColumn, resultSelector)
                    .CreateTable();
        }
    }
}