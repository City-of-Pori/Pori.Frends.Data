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
    /// Parameters for the Concatenate task.
    /// </summary>
    public class ConcatenateParameters
    {
        /// <summary>
        /// The tables to concatenate.
        /// </summary>
        public IEnumerable<dynamic> Tables { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Concatenate one or more tables together. All tables must have
        /// exactly the same columns in the same order.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new table with all the input tables' rows concatenated.</returns>
        public static Table Concatenate([PropertyTab] ConcatenateParameters input, CancellationToken cancellationToken)
        {
            // Separate the first table from the input tables
            var first = input.Tables.Cast<Table>().First();
            var rest  = input.Tables.Cast<Table>().Skip(1);

            // Check that all tables have the same columns in the same order.
            if(rest.Any(table => !table.Columns.SequenceEqual(first.Columns)))
                throw new ArgumentException("All tables have to have exactly the same columns");

            // Create the result table
            return TableBuilder
                    .From(first)
                    .Concatenate(rest)
                    .CreateTable();
        }
    }
}