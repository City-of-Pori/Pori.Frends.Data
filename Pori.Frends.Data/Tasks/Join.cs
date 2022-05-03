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
    /// How the original table should be included in the result of a join.
    /// </summary>
    public enum JoinResult
    {
        /// <summary>
        /// Include the entire matching row as a
        /// value of a single column in the result.
        /// </summary>
        Row,

        /// <summary>
        /// Include all columns of the original table
        /// as columns of the result table.
        /// </summary>
        AllColumns,

        /// <summary>
        /// Include one or more columns of the original
        /// table as columns of the result table.
        /// </summary>
        SelectColumns,

        /// <summary>
        /// Include all columns of the original table in the result, except
        /// for columns which were used as the key of the join.
        /// </summary>
        DiscardKey
    }

    /// <summary>
    /// Specifies the kind of join to perform between two tables.
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// Perform an inner join (only matching rows).
        /// </summary>
        Inner,

        /// <summary>
        /// Perform a left outer join (all rows from the left side table and
        /// only matching rows from the right side table).
        /// </summary>
        LeftOuter,

        /// <summary>
        /// Perform a full outer join (all rows from both tables).
        /// </summary>
        FullOuter
    }

    /// <summary>
    /// Define how a table should be treated in a join operation.
    /// </summary>
    public class JoinTable
    {
        /// <summary>
        /// The table to use as a source for the join operation.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// The names columns to use as the key for the join.
        /// </summary>
        public string[] KeyColumns { get; set; }

        /// <summary>
        /// How the matching rows should be included in the result.
        /// </summary>
        [DefaultValue(JoinResult.Row)]
        public JoinResult ResultType { get; set; }

        /// <summary>
        /// Name of the column in the result table to contain the matching
        /// rows from the original table.
        /// </summary>
        [UIHint(nameof(ResultType), "", JoinResult.Row)]
        public string ResultColumn { get; set; }

        /// <summary>
        /// List of the names of the columns from
        /// the original table to include in the result.
        /// </summary>
        [UIHint(nameof(ResultType), "", JoinResult.SelectColumns)]
        public string[] ResultColumns { get; set; }
    }

    /// <summary>
    /// Parameters for the Join task.
    /// </summary>
    public class JoinParameters
    {
        /// <summary>
        /// The type of join to perform.
        /// </summary>
        [DefaultValue(JoinType.Inner)]
        public JoinType JoinType { get; set; }

        /// <summary>
        /// The left side for the join.
        /// </summary>
        public JoinTable Left { get; set; }

        /// <summary>
        /// The right side for the join.
        /// </summary>
        public JoinTable Right { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Join the rows of two tables into a new table.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The result of the join as a new table.</returns>
        public static Table Join([PropertyTab] JoinParameters input, CancellationToken cancellationToken)
        {
            // Create shorthand names to parts of the input.
            var left  = input.Left;
            var right = input.Right;

            // Get the list of columns to include in the result
            var leftResultColumns  = JoinResultColumns(left);
            var rightResultColumns = JoinResultColumns(right);

            // When the result is a row, we use the provided names for the join columns,
            // otherwise we use temporary column names that aren't visible to the user
            // as they are replaced when expanding the join columns
            var leftJoinColumn  = left.ResultColumn ?? "$Pori.Frends.Data.Join.Left$";
            var rightJoinColumn = right.ResultColumn ?? "$Pori.Frends.Data.Join.Right$";

            // Validate parameters for each side of the join
            ValidateJoinParameters(left);
            ValidateJoinParameters(right);

            // Check that columns to be included in the result are distinct
            if(leftResultColumns.Intersect(rightResultColumns).Count() > 0)
                throw new ArgumentException("Cannot include multiple columns with the same name in the result of a join");

            // Start building the result table
            var result = TableBuilder.From(left.Data);

            // Perform the actual join
            switch(input.JoinType)
            {
                case JoinType.Inner:
                    result.InnerJoin(left.KeyColumns, leftJoinColumn,
                                     right.Data, right.KeyColumns, rightJoinColumn);
                    break;

                case JoinType.LeftOuter:
                    result.LeftOuterJoin(left.KeyColumns, leftJoinColumn,
                                         right.Data, right.KeyColumns, rightJoinColumn);
                    break;

                case JoinType.FullOuter:
                    result.FullOuterJoin(left.KeyColumns, leftJoinColumn,
                                         right.Data, right.KeyColumns, rightJoinColumn);
                    break;
            }

            // If the result type is other than JoinResult.Row,
            // expand the join columns
            if(left.ResultType != JoinResult.Row)
                result.ExpandColumn(leftJoinColumn, leftResultColumns);
            if(right.ResultType != JoinResult.Row)
                result.ExpandColumn(rightJoinColumn, rightResultColumns);

            // Make sure columns from the left table are always before
            // columns from the inner table
            if(left.ResultType != JoinResult.Row && right.ResultType == JoinResult.Row)
                result.ReorderColumns(leftResultColumns.Concat(new[] { rightJoinColumn }));

            // Create and return the resulting table
            return result.CreateTable();
        }

        /// <summary>
        /// Build the list of columns to include in the joined table.
        /// </summary>
        /// <param name="table">The information about one of the sides of the join.</param>
        /// <returns>The list of columns to include in the joined table.</returns>
        private static IEnumerable<string> JoinResultColumns(JoinTable table)
        {
            // Select the columns to include in the result of the join
            switch(table.ResultType)
            {
                // Result should have all the columns of the source table
                case JoinResult.AllColumns:
                    return table.Data.Columns;

                // Result should only specified columns of the source table
                case JoinResult.SelectColumns:
                    return table.ResultColumns;

                // Result should have all columns except the key columns
                case JoinResult.DiscardKey:
                    return table.Data.Columns.Where(c => !table.KeyColumns.Contains(c));

                // Result should have the matching rows as values of a new column
                case JoinResult.Row:
                    return new[] { table.ResultColumn };

                default: // Should not occur. Here to silence the compiler
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Validate the parameters for a table to be joined with another table.
        /// </summary>
        /// <param name="joinable">The information about one of the sides of the join.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateJoinParameters(JoinTable joinable)
        {
            // Check that a result column name is provided when the result type is Row
            if(joinable.ResultType == JoinResult.Row && string.IsNullOrEmpty(joinable.ResultColumn))
                throw new ArgumentException("Result column name must be specified for a join.");

            // Check that at least one column is specified when only
            // selected columns should be included in the result.
            if(joinable.ResultType == JoinResult.SelectColumns && joinable.ResultColumns.Count() == 0)
                throw new ArgumentException("At least one expanded column must be specified for a join.");

            // Check that all columns to be included in the result exist in the original table
            if(joinable.ResultType == JoinResult.SelectColumns && joinable.ResultColumns.Any(c => !joinable.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid column specified to be included in join result.");

            // Check that all key columns exist in the original table
            if(joinable.KeyColumns.Any(c => !joinable.Data.Columns.Contains(c)))
                throw new ArgumentException("Invalid key column specified for join");
        }
    }
}