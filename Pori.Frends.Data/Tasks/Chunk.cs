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
    using FilterFunc = Func<dynamic, bool>;

    /// <summary>
    /// Parameters for filtering rows from a Pori.Frends.Data.Table
    /// </summary>
    public class ChunkParameters
    {
        /// <summary>
        /// The table to filter.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// Whether to filter rows based on the entire row or a single column.
        /// </summary>
        [DefaultValue(10)]
        public int Size { get; set; }
    }


    public class ChunkTask
    {
        /// <summary>
        /// Split a table into multiple tables of a given (maximum) size.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of tables.</returns>
        public static List<Table> Chunk([PropertyTab] ChunkParameters input, CancellationToken cancellationToken)
        {
            if(input.Size <= 0)
                throw new ArgumentException("The chunk size must be at least 1");

            return TableBuilder
                    .From(input.Data)   // Use the input table as the source
                    .Chunk(input.Size)     // Filter the rows
                    .ToList();
        }
    }
}
