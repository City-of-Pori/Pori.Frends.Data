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
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The sorting criteria (column and order) to use.
        /// </summary>
        public SortingCriterion[] SortingCriteria { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Sort the rows of a table by one or more columns.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with sorted rows.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Table Sort([PropertyTab] SortParameters input, CancellationToken cancellationToken)
        {
            // Get the names of the columns to be transformed
            var columnNames = input.SortingCriteria.Select(column => column.Column);

            // Check that the input table has all the specified columns
            if(columnNames.Any(c => !input.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified");

            // Function for converting the input criterion (column name and order) to
            // the TableBuilder equivalent (key selector function and order)
            TableBuilder.SortingCriterion ConvertSortingCriterion(SortingCriterion inputCriterion)
            {
                return new TableBuilder.SortingCriterion
                {
                    // Key is a function that extracts the value of the specific column
                    KeySelector = TableBuilder.ColumnFunction(inputCriterion.Column, x => x),
                    // Order or the sort for this criterion
                    Order = inputCriterion.Order
                };
            }

            // Convert the input sorting criteria to the ones accepted by TableBuilder
            var criteria = input.SortingCriteria.Select(ConvertSortingCriterion);

            return TableBuilder
                    .From(input.Data)   // Create the table from the input table
                    .Sort(criteria)     // Sort using the criteria
                    .CreateTable();
        }
    }
}