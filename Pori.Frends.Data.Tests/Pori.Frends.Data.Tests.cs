using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pori.Frends.Data.Tests
{
    public class CsvInputData
    {
        public List<string> Headers { get; set; }
        public List<List<object>> Data { get; set; }
    }

    [TestFixture]
    class TableTests
    {
        private static readonly List<string> columns = new List<string> { "firstName", "lastName" };
        private static readonly List<Dictionary<string, string>> rows = new List<Dictionary<string, string>> 
        {
            new Dictionary<string, string> { { "firstName", "Ted" },      { "lastName", "Mosby" }       },
            new Dictionary<string, string> { { "firstName", "Robin" },    { "lastName", "Scherbatsky" } },
            new Dictionary<string, string> { { "firstName", "Marshall" }, { "lastName", "Eriksen" }     },
            new Dictionary<string, string> { { "firstName", "Lily" },     { "lastName", "Aldrin" }      },
            new Dictionary<string, string> { { "firstName", "Barney" },   { "lastName", "Stinson" }     },
        };



        [Test]
        public void TableRowsAreEnumerable()
        {
            Table table = Table.From(columns, rows);

            // Check that each row is a collection of key-value pairs
            foreach(var row in table.Rows)
                Assert.That(row is IEnumerable<KeyValuePair<string, dynamic>>);
        }

        [Test]
        public void EnumeratingTableRowsProducesTheRowsInOrder()
        {
            Table table = Table.From(columns, rows);

            Assert.That(table.Rows, Is.EqualTo(rows));
        }

        [Test]
        public void TableRowsAreInColumnOrder()
        {
            Table table = Table.From(columns, rows);

            // Check that each row has the columns in the table's column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in table.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(columns));
            }
        }
    }

    [TestFixture]
    class LoadTaskTests
    {
        [Test]
        public void CsvDataIsLoadedCorrectly()
        {
            var CsvData = new CsvInputData
            {
                Headers = new List<string> { "letter", "index", "isOdd" },
                Data = new List<List<object>>
                {
                    new List<object> { "A", 1, true  },
                    new List<object> { "B", 2, false },
                    new List<object> { "C", 3, true  },
                    new List<object> { "D", 4, false },
                    new List<object> { "E", 5, true  },
                    new List<object> { "F", 6, false },
                }
            };

            var input = new LoadParameters
            {
                Format = LoadFormat.CSV,
                CsvData = CsvData
            };


            Table result = DataTasks.Load(input, new System.Threading.CancellationToken());

            var resultLetters = from row in result.Rows select row.letter;
            var resultIndices = from row in result.Rows select row.index;
            var resultOddity  = from row in result.Rows select row.isOdd;

            var expectedLetters = from row in CsvData.Data select row[0];
            var expectedIndices = from row in CsvData.Data select row[1];
            var expectedOddity  = from row in CsvData.Data select row[2];

            Assert.That(resultLetters, Is.EqualTo(expectedLetters));
            Assert.That(resultIndices, Is.EqualTo(expectedIndices));
            Assert.That(resultOddity, Is.EqualTo(expectedOddity));
        }
    }

    [TestFixture]
    class FilterTaskTests
    {
        private static readonly List<string> columns = new List<string> { "id", "name", "eol", "inProduction" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> {  1, "Veribet",  "2020-08-07T07:11:05Z", false },
            new List<object> {  2, "Lotlux",   "2021-10-08T15:56:39Z", false },
            new List<object> {  3, "Tempsoft", "2022-12-13T23:21:06Z", true  },
            new List<object> {  4, "Opela",    "2022-02-03T07:52:34Z", false },
            new List<object> {  5, "Span",     "2022-02-22T07:56:15Z", false },
            new List<object> {  6, "Sonair",   "2019-05-21T19:22:41Z", true  },
            new List<object> {  7, "Cardify",  "2022-10-10T17:37:46Z", false },
            new List<object> {  8, "Hatity",   "2020-11-06T10:04:53Z", true  },
            new List<object> {  9, "Duobam",   "2020-02-08T16:17:19Z", false },
            new List<object> { 10, "Tresom",   "2020-08-18T23:47:29Z", false }
        };


        [Test]
        public void FilterUsingRowFilterReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            FilterParameters input = new FilterParameters 
            { 
                Data       = original, 
                FilterType = ProcessingType.Row,
                Filter     = row => row.inProduction == true
            };
            
            Table filtered = DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(filtered is Table);
            Assert.That(filtered, Is.Not.SameAs(original));
        }

        [Test]
        public void FilterUsingColumnFilterReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "inProduction",
                Filter       = inProduction => inProduction == true
            };

            Table filtered = DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(filtered is Table);
            Assert.That(filtered, Is.Not.SameAs(original));
        }

        [Test]
        public void FilterUsingRowFilterProducesCorrectRows()
        {
            FilterParameters input = new FilterParameters
            {
                Data       = Table.From(columns, rows),
                FilterType = ProcessingType.Row,
                Filter     = row => row.inProduction == true
            };

            Table filtered = DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(filtered.Rows, Has.All.Matches<dynamic>(row => row.inProduction == true));
        }

        [Test]
        public void FilterUsingColumnFilterProducesCorrectRows()
        {
            FilterParameters input = new FilterParameters
            {
                Data         = Table.From(columns, rows),
                FilterType   = ProcessingType.Column,
                FilterColumn = "inProduction",
                Filter       = inProduction => inProduction == true
            };

            Table filtered = DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(filtered.Rows, Has.All.Matches<dynamic>(row => row.inProduction == true));
        }

        [Test]
        public void FilterUsingRowFilterDoesNotAffectRowOrder()
        {
            Table original = Table.From(columns, rows);

            FilterParameters input = new FilterParameters
            {
                Data       = original,
                FilterType = ProcessingType.Row,    // Filter based on entire rows
                Filter     = row => true            // Accept all rows
            };

            Table filtered = DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(filtered.Rows, Is.EqualTo(original.Rows));
        }

        [Test]
        public void FilterUsingColumnFilterDoesNotAffectRowOrder()
        {
            Table original = Table.From(columns, rows);

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "inProduction",
                Filter       = inProduction => true
            };

            Table filtered = DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(filtered.Rows, Is.EqualTo(original.Rows));
        }

        [Test]
        public void FilterFailsWhenFilterColumnDoesNotExist()
        {
            Table original = Table.From(columns, rows);

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "doesNotExist",
                Filter       = doesNotExist => true
            };

            Action executeTask = () => DataTasks.Filter(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }
}
