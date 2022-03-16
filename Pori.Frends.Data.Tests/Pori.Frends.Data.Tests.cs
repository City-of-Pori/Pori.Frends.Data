using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pori.Frends.Data.Tests
{
    using TableFunc  = Func<dynamic, dynamic>;
    using FilterFunc = Func<dynamic, bool>;
    using RowDict    = IDictionary<string, dynamic>;

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
    class TableBuilderTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<string> reversedColumns = columns.Reverse<string>().ToList();
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> { 1,  2,  3,  4,  5, 1 },
            new List<object> { 2,  4,  6,  8, 10, 0 },
            new List<object> { 3,  6,  9, 12, 15, 1 },
            new List<object> { 4,  8, 12, 16, 20, 0 },
            new List<object> { 5, 10, 15, 20, 25, 1 },
        };
        private static readonly ColumnRename[] renamings = new ColumnRename[]
        {
            new ColumnRename { Column = "F", NewName = "W" },
            new ColumnRename { Column = "D", NewName = "Z" },
            new ColumnRename { Column = "C", NewName = "Y" },
            new ColumnRename { Column = "A", NewName = "X" },
        };

        [Test]
        public void TheResultIsANewTable()
        {
            Table original = Table.From(columns, rows);

            Table result = TableBuilder
                            .From(original)
                            .CreateTable();

            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void ConcatenateWorks()
        {
            Table fullTable = Table.From(columns, rows);

            Table[] tables =
            {
                Table.From(columns, rows.Skip(0).Take(2).ToList()),
                Table.From(columns, rows.Skip(2).Take(2).ToList()),
                Table.From(columns, rows.Skip(4).Take(2).ToList())
            };

            Table result = TableBuilder
                            .From(tables[0])
                            .Concatenate(tables.Skip(1))
                            .CreateTable();

            Assert.That(result.Rows, Is.EqualTo(fullTable.Rows));
        }

        [Test]
        public void FilterProducesCorrectRows()
        {
            Table      original = Table.From(columns, rows);
            FilterFunc filter   = (row) => row.A <= 3;

            Table filtered = TableBuilder
                                .From(original)
                                .Filter(filter)
                                .CreateTable();

            Assert.That(filtered.Rows, Has.All.Matches<dynamic>(row => row.A <= 3));
        }

        [Test]
        public void FilterDoesNotAffectRowOrder()
        {
            Table      original = Table.From(columns, rows);
            FilterFunc filter   = (row) => true;

            Table filtered = TableBuilder
                                .From(original)
                                .Filter(filter)
                                .CreateTable();

            Assert.That(filtered.Rows, Is.EqualTo(original.Rows));
        }

        [Test]
        public void GroupByCorrectlyGroupsRows()
        {
            Table    original        = Table.From(columns, rows);
            string[] keyColumns      = { "F" };
            string[] expectedColumns = { "F", "G" };
            var      keys            = original.Rows
                                        .Select(row => row.F)
                                        .Distinct();

            Table grouped = TableBuilder
                                .From(original)
                                .GroupBy(keyColumns, x => x, "G", rows => rows)
                                .CreateTable();

            // Check that the result has only the key column and the group column
            Assert.That(grouped.Columns, Is.EqualTo(expectedColumns));

            // Check that the result has the correct number of rows
            Assert.That(grouped.Count, Is.EqualTo(keys.Count()));

            // Check that the result contains all the keys
            // (not necessarily in the same order)
            Assert.That(
                grouped.Rows.Select(row => row.F), 
                Is.EquivalentTo(keys)
            );

            // Check that each group's elements has the correct key
            foreach(var row in grouped.Rows)
                Assert.That(row.G, Has.All.Matches<dynamic>(elem => elem.F == row.F));

            // Check that the result contains all the rows of the original table
            Assert.That(
                grouped.Rows.SelectMany(row => row.G as IEnumerable<dynamic>),
                Is.EquivalentTo(original.Rows)
            );
        }

        [Test]
        public void RenameColumnsDoesTheRename()
        {
            Table original = Table.From(columns, rows);

            Table result = TableBuilder
                            .From(original)
                            .RenameColumns(renamings.ToDictionary(r => r.Column, r => r.NewName))
                            .CreateTable();

            string[] expectedColumns = { "X", "B", "Y", "Z", "E", "W" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void ReorderColumnsResultsInCorrectColumnOrder()
        {
            Table original = Table.From(columns, rows);

            Table reordered = TableBuilder
                                .From(original)
                                .ReorderColumns(reversedColumns.ToArray())
                                .CreateTable();

            Assert.That(reordered.Columns, Is.EqualTo(reversedColumns));
        }

        [Test]
        public void ReorderColumnsReordersRowColumnOrder()
        {
            Table original = Table.From(columns, rows);

            Table reordered = TableBuilder
                                .From(original)
                                .ReorderColumns(reversedColumns.ToArray())
                                .CreateTable();

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in reordered.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(reversedColumns));
            }
        }

        [Test]
        public void ReorderColumnsDoesNotAffectOrderOfUnspecifiedColumns()
        {
            Table original = Table.From(columns, rows);

            Table reordered = TableBuilder
                                .From(original)
                                .ReorderColumns(new [] { "C", "E", "B" })
                                .CreateTable();

            string[] expectedColumnOrder = { "A", "C", "E", "D", "B", "F" };

            Assert.That(reordered.Columns, Is.EqualTo(expectedColumnOrder));

            // Check that the columns are in the correct order for each row in the result
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in reordered.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumnOrder));
            }
        }

        [Test]
        public void SelectColumnsProducesColumnsInTheSpecifiedOrder()
        {
            Table original = Table.From(columns, rows);

            string[] selectedColumns =  new string[] { "B", "A" };

            Table result = TableBuilder
                            .From(original)
                            .SelectColumns(selectedColumns)
                            .CreateTable();

            Assert.That(result.Columns, Is.EqualTo(selectedColumns));

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(selectedColumns));
            }
        }

        [Test]
        public void AddColumnAddsTheColumnAsTheLastColumn()
        {
            Table original = Table.From(columns, rows);

            Table result = TableBuilder
                            .From(original)
                            .AddColumn("G", row => "foo")
                            .CreateTable();

            string[] expectedColumns = columns.Concat(new [] {"G"}).ToArray();

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));

            // Check that each row has the correct columns
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumns));
            }
        }

        [Test]
        public void SortProducesCorrectResultsWithASingleCriterion()
        {
            Table original = Table.From(columns, rows);

            TableBuilder.SortingCriterion[] criteria = new TableBuilder.SortingCriterion[]
            {
                new TableBuilder.SortingCriterion { KeySelector = row => row.B, Order = Order.Descending }
            };

            Table result = TableBuilder
                            .From(original)
                            .Sort(criteria)
                            .CreateTable();

            // Check the values in the result are correct.
            // Also ends up making sure that the original rows have not been modified.
            Assert.That(
                result.Rows.Select(row => row.B),
                Is.Ordered.Descending
            );
        }

        [Test]
        public void SortProducesCorrectResultsWithMultipleCriteria()
        {
            Table original = Table.From(columns, rows);

            TableBuilder.SortingCriterion[] criteria = new TableBuilder.SortingCriterion[]
            {
                new TableBuilder.SortingCriterion { KeySelector = row => row.B, Order = Order.Descending },
                new TableBuilder.SortingCriterion { KeySelector = row => row.F, Order = Order.Ascending }
            };

            Table result = TableBuilder
                            .From(original)
                            .Sort(criteria)
                            .CreateTable();

            // Check the values in the result are correct.
            // Also ends up making sure that the original rows have not been modified.
            Assert.That(
                result.Rows.Select(row => new { row.B, row.F }),
                Is.Ordered.Descending.By("B").Then.Ascending.By("F")
            );
        }

        [Test]
        public void TransformColumnProducesCorrectValues()
        {
            Table original = Table.From(columns, rows);

            Table result = TableBuilder
                            .From(original)
                            .TransformColumn("A", row => row.A * 10)
                            .CreateTable();

            // Check the values in the result are correct.
            // Also ends up making sure that the original rows have not been modified.
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.A == orig.A * 10),
                Has.All.EqualTo(true)
            );
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
    class AddColumnsTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> { 1,  2,  3,  4,  5,  6 },
            new List<object> { 2,  4,  6,  8, 10, 12 },
            new List<object> { 3,  6,  9, 12, 15, 18 },
            new List<object> { 4,  8, 12, 16, 20, 24 },
            new List<object> { 5, 10, 15, 20, 25, 30 },
        };

        [Test]
        public void AddColumnsReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "G", ValueSource = NewColumnValueSource.Constant, Value = 0 }
                }
            };

            Table result = DataTasks.AddColumns(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void AddColumnsActuallyAddsTheColumns()
        {
            Table original = Table.From(columns, rows);

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "G", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                    new NewColumn { Name = "H", ValueSource = NewColumnValueSource.Constant, Value = 1 },
                }
            };

            IEnumerable<string> expectedColumns = original.Columns.Concat(input.Columns.Select(c => c.Name));

            
            Table result = DataTasks.AddColumns(input, new System.Threading.CancellationToken());


            Assert.That(result.Columns, Is.EqualTo(expectedColumns));

            // Check each row contains the correct columns
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumns));
            }
        }

        [Test]
        public void AddColumnsWorksWithConstantValuesForTheColumns()
        {
            Table original = Table.From(columns, rows);

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "G", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                    new NewColumn { Name = "H", ValueSource = NewColumnValueSource.Constant, Value = 1 },
                }
            };


            Table result = DataTasks.AddColumns(input, new System.Threading.CancellationToken());


            // Check each row contains the correct columns
            foreach(var row in result.Rows)
            {
                Assert.That(row.G, Is.EqualTo(0));
                Assert.That(row.H, Is.EqualTo(1));
            }
        }

        [Test]
        public void AddColumnsWorksWithComputedValuesForTheColumns()
        {
            Table original = Table.From(columns, rows);

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "copyOfA", ValueSource = NewColumnValueSource.Computed, ValueGenerator = row => row.A },
                    new NewColumn { Name = "copyOfB", ValueSource = NewColumnValueSource.Computed, ValueGenerator = row => row.B },
                }
            };


            Table result = DataTasks.AddColumns(input, new System.Threading.CancellationToken());


            // Check each row contains the correct values in the new columns
            foreach(var row in result.Rows)
            {
                Assert.That(row.copyOfA, Is.EqualTo(row.A));
                Assert.That(row.copyOfB, Is.EqualTo(row.B));
            }
        }

        [Test]
        public void AddColumnsThrowsWhenAddingMultipleColumnsWithTheSameName()
        {
            Table original = Table.From(columns, rows);

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "X", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                    new NewColumn { Name = "X", ValueSource = NewColumnValueSource.Constant, Value = 1 },
                }
            };

            Action executeTask = () => DataTasks.AddColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void AddColumnsThrowsWhenAddingAnExistingColumn()
        {
            Table original = Table.From(columns, rows);

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "A", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                }
            };

            Action executeTask = () => DataTasks.AddColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class ConcatenateTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A","B","C","D","E","F","I","M","N","U" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            //                  A    B     C         D           E           F       I    M        N                  U
            new List<object> {  0,  true, "T", "04.08.2015", "Foxtrot",     -8.6,   541,  0,       "Puce", "2027-11-15T06:56:47Z" },
            new List<object> {  1,  true, "W", "07.12.2004",   "Tango",    -43.5,   244,  1,       "Teal", "2004-11-20T03:02:28Z" },
            new List<object> {  2,  true, "L", "19.07.2023",    "Echo",   -10.11,  -869,  0,         null, "2015-01-14T00:51:16Z" },
            new List<object> {  3, false, "S", "27.05.2027",    "Alfa",   -66.06,  -761,  1,         null, "2028-03-25T21:49:37Z" },
            new List<object> {  4, false, "Z", "13.10.2014", "Uniform",   -14.72,  -275,  0,  "Goldenrod", "2028-03-16T08:08:43Z" },
            new List<object> {  5,  true, "Y", "05.09.2013",   "Oscar",   -29.71,  -896,  1,      "Green", "2027-08-11T12:32:56Z" },
            new List<object> {  6,  true, "T", "21.07.2003",   "Bravo",     7.05,  -706,  0,      "Khaki", "2013-12-19T14:24:42Z" },
            new List<object> {  7, false, "X", "23.12.2004",   "Bravo",    74.45,   424,  1,       "Mauv", "2013-10-20T18:21:19Z" },
            new List<object> {  8,  true, "P", "23.09.2023","November",    49.35,  -417,  0,         null, "1999-07-27T23:16:03Z" },
            new List<object> {  9,  true, "G", "06.04.2007",   "Tango",    -54.5,    -8,  1,         null, "2017-05-23T19:01:35Z" },
            new List<object> { 10,  true, "Q", "13.03.2025",    "Papa",   -87.98,   594,  0,         null, "2015-07-17T18:30:11Z" },
            new List<object> { 11,  true, "T", "26.02.2017", "Foxtrot",       75,   745,  1,     "Fuscia", "2013-09-27T23:27:52Z" },
            new List<object> { 12, false, "U", "24.06.2002",    "Kilo",   -97.89,  -678,  0,         null, "2028-05-17T04:10:04Z" },
            new List<object> { 13,  true, "X", "10.02.2020",    "Mike",    63.58,   363,  1,     "Maroon", "2024-04-17T07:16:37Z" },
            new List<object> { 14, false, "S", "03.05.2023",   "Delta",   -60.48,   979,  0,  "Goldenrod", "2000-06-10T03:15:18Z" },
            new List<object> { 15, false, "I", "14.09.2029", "Whiskey",    72.45,  -406,  1,       "Pink", "1999-01-19T00:29:17Z" },
            new List<object> { 16,  true, "Q", "24.02.2009",    "Papa",   -80.44,     9,  0,         null, "2013-10-27T06:43:15Z" },
            new List<object> { 17,  true, "R", "11.08.2015", "Uniform",    -26.4,  -293,  1, "Aquamarine", "2022-02-03T08:57:37Z" },
            new List<object> { 18,  true, "T", "07.06.2026",   "Oscar",     27.6,  -592,  0,         null, "2007-10-25T23:44:31Z" },
            new List<object> { 19,  true, "W", "18.03.2000", "Uniform",   -60.79,  -130,  1,         null, "2001-03-09T11:05:58Z" },
        };

        [Test]
        public void ConcatenateReturnsANewTable()
        {
            Table fullTable = Table.From(columns, rows);

            int chunkSize  = 6;
            int chunkCount = (int) Math.Ceiling((double) fullTable.Count / chunkSize);

            IEnumerable<Table> chunks = Enumerable
                                            .Range(0, chunkCount)
                                            .Select(i => rows.Skip(i * chunkSize).Take(chunkSize))
                                            .Select(chunkRows => Table.From(columns, chunkRows.ToList()));

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = chunks.ToArray()
            };

            Table result = DataTasks.Concatenate(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(fullTable));
        }

        [Test]
        public void ConcatenateReturnsTheCorrectResultWithASingleTable()
        {
            Table fullTable = Table.From(columns, rows);

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = new Table[] { fullTable }
            };

            Table result = DataTasks.Concatenate(input, new System.Threading.CancellationToken());

            Assert.That(result, Is.Not.SameAs(fullTable));
            Assert.That(result.Columns, Is.EqualTo(fullTable.Columns));
            Assert.That(result.Rows, Is.EqualTo(fullTable.Rows));
        }

        [Test]
        public void ConcatenateReturnsTheCorrectResultWithMultipleTables()
        {
            Table fullTable = Table.From(columns, rows);

            int chunkSize  = 6;
            int chunkCount = (int) Math.Ceiling((double) fullTable.Count / chunkSize);

            IEnumerable<Table> chunks = Enumerable
                                            .Range(0, chunkCount)
                                            .Select(i => rows.Skip(i * chunkSize).Take(chunkSize))
                                            .Select(chunkRows => Table.From(columns, chunkRows.ToList()));

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = chunks.ToArray()
            };

            Table result = DataTasks.Concatenate(input, new System.Threading.CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(fullTable.Columns));
            Assert.That(result.Rows, Is.EqualTo(fullTable.Rows));
        }

        [Test]
        public void ConcatenateThrowsWhenTableColumnsDoNotMatch()
        {
            Table fullTable = Table.From(columns, rows);

            Table first  = Table.From(columns, rows.Skip(0).Take(10).ToList());
            Table second = Table.From(columns, rows.Skip(1).Take(10).ToList());

            Table incompatible = TableBuilder
                                    .From(second)
                                    .RenameColumns(new Dictionary<string, string> { { "A", "X" } })
                                    .CreateTable();

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = new [] { first, incompatible }
            };

            Action executeTask = () => DataTasks.Concatenate(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class ConvertColumnsTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            //                  A     B       C              D               E          F
            new List<object> {  0, "false",  682, "12.03.2027 09:32:28", "4821503",  "63.34" },
            new List<object> {  1,  "true", -974, "12.12.2023 07:25:27", "7116123",   "69.6" },
            new List<object> {  2, "false", -626, "05.09.2025 09:23:42", "2360915",   "3.68" },
            new List<object> {  3, "false", -635, "28.02.2020 02:07:36", "4804015",   "7.44" },
            new List<object> {  4, "false", -532, "08.10.2009 05:36:31", "9959168",  "39.21" },
            new List<object> {  5, "false", -874, "11.03.2003 04:07:12", "8845181", "-51.42" },
            new List<object> {  6, "false",    0, "09.04.2002 07:21:49", "4003331", "-91.24" },
            new List<object> {  7,  "true", -251, "12.12.2012 08:49:06", "9437750",  "28.63" },
            new List<object> {  8,  "true",  719, "19.03.2002 06:28:28", "3619804", "-69.55" },
            new List<object> {  9, "false", -103, "01.08.2004 11:42:11", "3605879", "-81.51" },
            new List<object> { 10,  "true",    0, "09.08.2015 08:00:10", "9825135",     "13" },
            new List<object> { 11,  "true", -678, "12.06.2022 01:28:56", "6606703",  "-12.8" },
            new List<object> { 12, "false",    0, "03.12.2018 01:19:55", "6037166",  "56.05" },
            new List<object> { 13, "false",  526, "08.04.2011 09:20:46", "8132910", "-51.63" },
            new List<object> { 14, "false", -630, "25.02.2009 06:43:52", "8239276", "-34.54" },
            new List<object> { 15, "false",  802, "26.11.2027 05:00:20", "1828554", "-43.64" },
        };

        [Test]
        public void ConvertColumnsReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "B", Type = ColumnType.Boolean },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void ConvertColumnsToBooleanWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "B", Type = ColumnType.Boolean },
                    new ColumnConversion { Column = "C", Type = ColumnType.Boolean },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.B is bool));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.C == (orig.C != 0)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsUsingCustomConverterWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "C", Type = ColumnType.Custom, Converter = x => x > 0 },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.C is bool));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.C == (orig.C > 0)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToDateTimeWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "D", Type = ColumnType.DateTime, DateTimeFormat = "dd.MM.yyyy hh:mm:ss" },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.D is DateTime));
            Assert.That(
                original.Rows.Zip(
                    result.Rows, 
                    (orig, res) => res.D == DateTime.ParseExact(orig.D, "dd.MM.yyyy hh:mm:ss", null)
                ),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToDecimalWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "F", Type = ColumnType.Decimal },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.F is decimal));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.F == decimal.Parse(orig.F)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToDoubleWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "F", Type = ColumnType.Double },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.F is double));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.F == double.Parse(orig.F)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToFloatWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "F", Type = ColumnType.Float },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.F is float));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.F == float.Parse(orig.F)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToIntWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "E", Type = ColumnType.Int },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.E is int));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.E == int.Parse(orig.E)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToLongWorks()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "E", Type = ColumnType.Long },
                }
            };

            Table result = DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.E is long));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.E == long.Parse(orig.E)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsThrowsWhenSpecifyingAnInvalidColumnName()
        {
            Table original = Table.From(columns, rows);

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "X", Type = ColumnType.Int }
                }
            };

            Action executeTask = () => DataTasks.ConvertColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class GroupByTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A","B","C","D","E","F","I","M","N","U" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            //                  A    B     C         D           E           F       I    M        N                  U
            new List<object> {  0,  true, "T", "04.08.2015", "Foxtrot",     -8.6,   541,  0,       "Puce", "2027-11-15T06:56:47Z" },
            new List<object> {  1,  true, "W", "07.12.2004",   "Tango",    -43.5,   244,  1,       "Teal", "2004-11-20T03:02:28Z" },
            new List<object> {  2,  true, "L", "19.07.2023",    "Echo",   -10.11,  -869,  0,         null, "2015-01-14T00:51:16Z" },
            new List<object> {  3, false, "S", "27.05.2027",    "Alfa",   -66.06,  -761,  1,         null, "2028-03-25T21:49:37Z" },
            new List<object> {  4, false, "Z", "13.10.2014", "Uniform",   -14.72,  -275,  0,  "Goldenrod", "2028-03-16T08:08:43Z" },
            new List<object> {  5,  true, "Y", "05.09.2013",   "Oscar",   -29.71,  -896,  1,      "Green", "2027-08-11T12:32:56Z" },
            new List<object> {  6,  true, "T", "21.07.2003",   "Bravo",     7.05,  -706,  0,      "Khaki", "2013-12-19T14:24:42Z" },
            new List<object> {  7, false, "X", "23.12.2004",   "Bravo",    74.45,   424,  1,       "Mauv", "2013-10-20T18:21:19Z" },
            new List<object> {  8,  true, "P", "23.09.2023","November",    49.35,  -417,  0,         null, "1999-07-27T23:16:03Z" },
            new List<object> {  9,  true, "G", "06.04.2007",   "Tango",    -54.5,    -8,  1,         null, "2017-05-23T19:01:35Z" },
            new List<object> { 10,  true, "Q", "13.03.2025",    "Papa",   -87.98,   594,  0,         null, "2015-07-17T18:30:11Z" },
            new List<object> { 11,  true, "T", "26.02.2017", "Foxtrot",       75,   745,  1,     "Fuscia", "2013-09-27T23:27:52Z" },
            new List<object> { 12, false, "U", "24.06.2002",    "Kilo",   -97.89,  -678,  0,         null, "2028-05-17T04:10:04Z" },
            new List<object> { 13,  true, "X", "10.02.2020",    "Mike",    63.58,   363,  1,     "Maroon", "2024-04-17T07:16:37Z" },
            new List<object> { 14, false, "S", "03.05.2023",   "Delta",   -60.48,   979,  0,  "Goldenrod", "2000-06-10T03:15:18Z" },
            new List<object> { 15, false, "I", "14.09.2029", "Whiskey",    72.45,  -406,  1,       "Pink", "1999-01-19T00:29:17Z" },
            new List<object> { 16,  true, "Q", "24.02.2009",    "Papa",   -80.44,     9,  0,         null, "2013-10-27T06:43:15Z" },
            new List<object> { 17,  true, "R", "11.08.2015", "Uniform",    -26.4,  -293,  1, "Aquamarine", "2022-02-03T08:57:37Z" },
            new List<object> { 18,  true, "T", "07.06.2026",   "Oscar",     27.6,  -592,  0,         null, "2007-10-25T23:44:31Z" },
            new List<object> { 19,  true, "W", "18.03.2000", "Uniform",   -60.79,  -130,  1,         null, "2001-03-09T11:05:58Z" },
        };

        [Test]
        public void GroupByReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = new string[] { "B" },
                ResultColumn = "G",
                Grouping     = GroupingType.EntireRows
            };

            Table result = DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void GroupByIncludesAllKeyColumnsInTheResult()
        {
            Table original = Table.From(columns, rows);
            string[] keyColumns = { "B", "M" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.EntireRows
            };

            Table result = DataTasks.GroupBy(input, new System.Threading.CancellationToken());


            string[] expectedColumns = { "B", "M", "G" };

            // Check that the table has correct columns
            Assert.That(result.Columns, Is.EqualTo(expectedColumns));

            // Check that each row has the right columns
            foreach(RowDict row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumns));
            }
        }

        [Test]
        public void GroupByCanProduceEntireGroupedRowsAsATable()
        {
            Table original = Table.From(columns, rows);
            string[] keyColumns = { "B" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.EntireRows
            };

            Table result = DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            // Check that each group is a table
            foreach(var row in result.Rows)
                Assert.That(row.G is Table);

            // Check that each group's elements has the correct key
            foreach(var row in result.Rows)
                Assert.That((row.G as Table).Rows, Has.All.Matches<dynamic>(elem => elem.B == row.B));

            // Check that the result contains all the rows of the original table
            Assert.That(
                result.Rows.SelectMany(row => row.G.Rows as IEnumerable<dynamic>),
                Is.EquivalentTo(original.Rows)
            );
        }

        [Test]
        public void GroupByCanProduceSelectedColumnsOfGroupedRowsAsATable()
        {
            Table original = Table.From(columns, rows);
            string[] keyColumns = { "B" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.SelectedColumns,
                Columns      = new[] { "A", "B", "C" }
            };

            Table result = DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            string[] expectedColumns = { "A", "B", "C" };

            // Check that each group's elements has the correct key
            foreach(var row in result.Rows)
                Assert.That((row.G as Table).Rows, Has.All.Matches<dynamic>(elem => elem.B == row.B));

            Table expectedGroupedRows = TableBuilder
                                            .From(original)
                                            .SelectColumns(expectedColumns)
                                            .CreateTable();

            // Check that the result contains all the rows of the original table
            Assert.That(
                result.Rows.SelectMany(row => row.G.Rows as IEnumerable<dynamic>),
                Is.EquivalentTo(expectedGroupedRows.Rows)
            );
        }

        [Test]
        public void GroupByCanProduceValuesOfASingleColumnAsAnEnumerable()
        {
            Table original = Table.From(columns, rows);
            string[] keyColumns = { "B" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.SingleColumn,
                Column       = "A"
            };

            Table result = DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            // Check that each group consists of the values of the selected
            // column.
            foreach(var row in result.Rows)
            {
                Table expectedGroupedRows = TableBuilder
                                                .From(original)
                                                .Filter(origRow => origRow.B == row.B)
                                                .CreateTable();

                Assert.That(
                    row.G,
                    Is.EquivalentTo(expectedGroupedRows.Rows.Select(r => r.A))
                );
            }

            // Check that the result contains values matching all the rows
            // of the original table.
            Assert.That(
                result.Rows.SelectMany(row => row.G as IEnumerable<dynamic>),
                Is.EquivalentTo(original.Rows.Select(r => r.A))
            );
        }

        [Test]
        public void GroupByCanProduceComputedValuesAsAnEnumerable()
        {
            Table original = Table.From(columns, rows);
            string[] keyColumns = { "B" };
            TableFunc selectElement = row => row.A * 2;

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = keyColumns,
                ResultColumn    = "G",
                Grouping        = GroupingType.Computed,
                ComputeValue    = selectElement
            };

            Table result = DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            // Check that each group consists of the values of the selected
            // column.
            foreach(var row in result.Rows)
            {
                Table expectedGroupedRows = TableBuilder
                                                .From(original)
                                                .Filter(origRow => origRow.B == row.B)
                                                .CreateTable();

                Assert.That(
                    row.G,
                    Is.EquivalentTo(expectedGroupedRows.Rows.Select(selectElement))
                );
            }

            // Check that the result contains all values matching the rows
            // of the original table.
            Assert.That(
                result.Rows.SelectMany(row => row.G as IEnumerable<dynamic>),
                Is.EquivalentTo(original.Rows.Select(selectElement))
            );
        }

        [Test]
        public void GroupByThrowsWhenKeyColumnDoesNotExistInTheTable()
        {
            Table original = Table.From(columns, rows);

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = new[] { "X" },  // <---
                ResultColumn    = "G",
                Grouping        = GroupingType.EntireRows
            };

            Action executeTask = () => DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void GroupByThrowsWhenASelectedColumnDoesNotExistInTheTable()
        {
            Table original = Table.From(columns, rows);

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = new[] { "B" },
                ResultColumn    = "G",
                Grouping        = GroupingType.SelectedColumns,
                Columns         = new[] { "A", "X" }  // <---
            };

            Action executeTask = () => DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void GroupByThrowsWhenTheSelectedSingleColumnDoesNotExistInTheTable()
        {
            Table original = Table.From(columns, rows);

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = new[] { "B" },
                ResultColumn    = "G",
                Grouping        = GroupingType.SingleColumn,
                Column          = "X" // <---
            };

            Action executeTask = () => DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void GroupByThrowsWhenResultColumnIsOneOfTheKeyColums()
        {
            Table original = Table.From(columns, rows);
            string[] keyColumns = { "B", "M" };

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = keyColumns,
                ResultColumn    = "M",  // <---
                Grouping        = GroupingType.EntireRows
            };

            Action executeTask = () => DataTasks.GroupBy(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
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

    [TestFixture]
    public class JoinTaskTests
    {
        private class JoinRows
        {
            public List<string>       columns;
            public string[]           key;
            public List<List<object>> matched;
            public List<List<object>> duplicateMatches;
            public List<List<object>> unmatched;
        };

        private static readonly JoinRows left = new JoinRows
        {
            columns = new List<string> { "A", "B", "V1", "V2", "V3" },
            key     = new [] { "A", "B" },
            matched = new List<List<object>>
            {
                new List<object> {    "India",   "Delta", false, 85,    "Yellow" },
                new List<object> {   "Quebec",    "Lima", false,  5, "Turquoise" },
                new List<object> {  "Foxtrot",    "Zulu", false, 16,    "Fuscia" },
                new List<object> {  "Foxtrot", "Juliett", false, 99, "Turquoise" },
                new List<object> {    "India",    "Mike",  true, 72,      "Blue" },
                new List<object> {  "Charlie", "Juliett", false, 83,     "Khaki" },
                new List<object> {    "Romeo",    "Golf",  true, 93,    "Yellow" },
                new List<object> {     "Mike",   "Delta", false, 55,    "Fuscia" },
                new List<object> {    "Delta",   "Bravo",  true, 31,    "Violet" },
                new List<object> { "November",  "Victor",  true,  3,    "Maroon" },
            },
            unmatched = new List<List<object>> 
            {
                new List<object> {    "Kilo", "Oscar",  true,  7, "Mauv" },
                new List<object> { "Juliett",  "Echo",  true, 57,  "Red" },
                new List<object> {   "Hotel", "Bravo", false, 16, "Mauv" },
            }
        };

        private static readonly JoinRows right = new JoinRows
        {
            columns = new List<string> { "X", "Y", "V4", "V5" },
            key     = new [] { "X", "Y" },
            matched = new List<List<object>>
            {
                new List<object> {    "India",   "Delta", 0.85, "#ff8387" },
                new List<object> {   "Quebec",    "Lima", 0.35, "#d1e48e" },
                new List<object> {  "Foxtrot",    "Zulu", 0.14, "#23d265" },
                new List<object> {  "Foxtrot", "Juliett", 0.87, "#83870b" },
                new List<object> {    "India",    "Mike", 0.23, "#25c28f" },
                new List<object> {  "Charlie", "Juliett",  0.2, "#88f146" },
                new List<object> {    "Romeo",    "Golf", 0.24, "#5dc8c6" },
                new List<object> {     "Mike",   "Delta", 0.65, "#7caca6" },
                new List<object> {    "Delta",   "Bravo", 0.34, "#781c91" },
                new List<object> { "November",  "Victor", 0.12, "#2b0113" }
            },
            duplicateMatches = new List<List<object>>
            {
                new List<object> {    "Mike",   "Delta", 0.48, "#b93587" },
                new List<object> {  "Quebec",    "Lima",  0.3, "#4bf785" },
                new List<object> { "Foxtrot", "Juliett", 0.18, "#ae3c3c" }
            },
            unmatched = new List<List<object>>
            {
                new List<object> { "Hotel", "Victor", 0.23, "#f1a51e" },
                new List<object> { "Romeo",   "Alfa", 0.47, "#808a8f" },
                new List<object> {  "Zulu",   "Papa", 0.42, "#5b528f" }
            }
        };

        [Test]
        public void JoinReturnsANewTable()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data         = leftTable,
                    KeyColumns   = left.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "left"
                },
                Right = new JoinTable
                {
                    Data         = rightTable,
                    KeyColumns   = right.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "right"
                }
            };

            Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(leftTable));
            Assert.That(result, Is.Not.SameAs(rightTable));
        }

        [Test]
        public void JoinWorksWhenAllLeftRowsHaveASingleMatch()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinType[] joins = { JoinType.Inner, JoinType.LeftOuter };

            foreach(var joinType in joins)
            {
                JoinParameters input = new JoinParameters
                {
                    JoinType = joinType,
                    Left = new JoinTable
                    {
                        Data         = leftTable,
                        KeyColumns   = left.key,
                        ResultType   = JoinResult.Row,
                        ResultColumn = "left"
                    },
                    Right = new JoinTable
                    {
                        Data         = rightTable,
                        KeyColumns   = right.key,
                        ResultType   = JoinResult.Row,
                        ResultColumn = "right"
                    }
                };

                Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

                string[] expectedColumns = { "left", "right" };

                Assert.That(result.Columns, Is.EqualTo(expectedColumns));

                // Check that each row has the columns in the new column order
                foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
                {
                    var keys = row.Select(x => x.Key);

                    Assert.That(keys, Is.EqualTo(expectedColumns));
                }

                Assert.That(result.Rows.Select(row => row.left), Is.EquivalentTo(leftTable.Rows));
                Assert.That(result.Rows.Select(row => row.right), Is.EquivalentTo(rightTable.Rows));
            }
        }

        [Test]
        public void JoinWorksWhenAllLeftRowsHaveMatches()
        {
            var rightRows = right.matched.Concat(right.duplicateMatches).ToList();

            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, rightRows);

            JoinType[] joins = { JoinType.Inner, JoinType.LeftOuter };

            foreach(var joinType in joins)
            {
                JoinParameters input = new JoinParameters
                {
                    JoinType = joinType,
                    Left = new JoinTable
                    {
                        Data         = leftTable,
                        KeyColumns   = left.key,
                        ResultType   = JoinResult.Row,
                        ResultColumn = "left"
                    },
                    Right = new JoinTable
                    {
                        Data         = rightTable,
                        KeyColumns   = right.key,
                        ResultType   = JoinResult.Row,
                        ResultColumn = "right"
                    }
                };

                Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

                string[] expectedColumns = { "left", "right" };

                Assert.That(result.Columns, Is.EqualTo(expectedColumns));

                // Check that each row has the columns in the new column order
                foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
                {
                    var keys = row.Select(x => x.Key);

                    Assert.That(keys, Is.EqualTo(expectedColumns));
                }

                var resultLeftRows = result.Rows.Select(row => row.left);
                var resultRightRows = result.Rows.Select(row => row.right);

                foreach(var row in leftTable.Rows)
                    Assert.That(resultLeftRows, Contains.Item(row));

                foreach(var row in rightTable.Rows)
                    Assert.That(resultRightRows, Contains.Item(row));
            }
        }

        [Test]
        public void InnerJoinDoesNotProduceLeftRowsThatHaveNoMatch()
        {
            Table leftMatched    = Table.From(left.columns, left.matched);
            Table rightUnmatched = Table.From(left.columns, left.unmatched);

            Table leftTable = TableBuilder
                                .From(leftMatched)
                                .Concatenate(new []{ rightUnmatched })
                                .CreateTable();

            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data         = leftTable,
                    KeyColumns   = left.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "left"
                },
                Right = new JoinTable
                {
                    Data         = rightTable,
                    KeyColumns   = right.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "right"
                }
            };

            Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows.Select(row => row.left), Is.EquivalentTo(leftMatched.Rows));
        }

        [Test]
        public void OuterJoinHasAllTheRowsFromTheLeftTable()
        {
            Table leftMatched   = Table.From(left.columns, left.matched);
            Table leftUnmatched = Table.From(left.columns, left.unmatched);

            Table leftTable = TableBuilder
                                .From(leftMatched)
                                .Concatenate(new []{ leftUnmatched })
                                .CreateTable();

            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.LeftOuter,
                Left = new JoinTable
                {
                    Data         = leftTable,
                    KeyColumns   = left.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "left"
                },
                Right = new JoinTable
                {
                    Data         = rightTable,
                    KeyColumns   = right.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "right"
                }
            };

            Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

            string[] expectedColumns = { "left", "right" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumns));
            }

            var resultMatchedRows = result.Rows.Where(row => Enumerable.Contains(rightTable.Rows, row.right));

            Assert.That(result.Rows.Select(row => row.left), Is.EquivalentTo(leftTable.Rows));
            Assert.That(resultMatchedRows.Select(row => row.right), Is.EquivalentTo(rightTable.Rows));
        }

        [Test]
        public void OuterJoinHasOnlyMatchingRowsFromTheRightTable()
        {
            Table leftMatched   = Table.From(left.columns, left.matched);
            Table leftUnmatched = Table.From(left.columns, left.unmatched);

            Table leftTable = TableBuilder
                                .From(leftMatched)
                                .Concatenate(new []{ leftUnmatched })
                                .CreateTable();

            Table rightMatched   = Table.From(right.columns, right.matched);
            Table rightUnmatched = Table.From(right.columns, right.unmatched);

            Table rightTable = TableBuilder
                                .From(rightMatched)
                                .Concatenate(new []{ rightUnmatched })
                                .CreateTable();

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.LeftOuter,
                Left = new JoinTable
                {
                    Data         = leftTable,
                    KeyColumns   = left.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "left"
                },
                Right = new JoinTable
                {
                    Data         = rightTable,
                    KeyColumns   = right.key,
                    ResultType   = JoinResult.Row,
                    ResultColumn = "right"
                }
            };

            Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

            var resultMatchedRows = result.Rows.Where(row => row.right.X != null);

            Assert.That(resultMatchedRows.Select(row => row.right), Is.EquivalentTo(rightMatched.Rows));
        }

        [Test]
        public void LeftRowColumnsCanBeExpanded()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            var testCases = new [] {
                new 
                {
                    ResultType      = JoinResult.AllColumns,
                    ResultColumns   = new string[] {},
                    ExpectedColumns = new [] { "A", "B", "V1", "V2", "V3", "right" }
                },
                new
                {
                    ResultType      = JoinResult.DiscardKey,
                    ResultColumns   = new string[] {},
                    ExpectedColumns = new [] { "V1", "V2", "V3", "right" }
                },
                new
                {
                    ResultType      = JoinResult.SelectColumns,
                    ResultColumns   = new [] { "A", "B", "V2" },
                    ExpectedColumns = new [] { "A", "B", "V2", "right" }
                },
            };

            foreach(var testCase in testCases)
            {
                JoinParameters input = new JoinParameters
                {
                    JoinType = JoinType.Inner,
                    Left = new JoinTable
                    {
                        Data          = leftTable,
                        KeyColumns    = left.key,
                        ResultType    = testCase.ResultType,
                        ResultColumns = testCase.ResultColumns
                    },
                    Right = new JoinTable
                    {
                        Data         = rightTable,
                        KeyColumns   = right.key,
                        ResultType   = JoinResult.Row,
                        ResultColumn = "right"
                    }
                };

                Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

                Assert.That(result.Columns, Is.EqualTo(testCase.ExpectedColumns));

                // Check that each row has the columns in the new column order
                foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
                {
                    var keys = row.Select(x => x.Key);

                    Assert.That(keys, Is.EqualTo(testCase.ExpectedColumns));
                }
            }
        }

        [Test]
        public void RightRowColumnsCanBeExpanded()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            var testCases = new [] {
                new
                {
                    ResultType      = JoinResult.AllColumns,
                    ResultColumns   = new string[] {},
                    ExpectedColumns = new [] { "left", "X", "Y", "V4", "V5" }
                },
                new
                {
                    ResultType      = JoinResult.DiscardKey,
                    ResultColumns   = new string[] {},
                    ExpectedColumns = new [] { "left", "V4", "V5" }
                },
                new
                {
                    ResultType      = JoinResult.SelectColumns,
                    ResultColumns   = new [] { "X", "Y", "V5" },
                    ExpectedColumns = new [] { "left", "X", "Y", "V5" }
                },
            };

            foreach(var testCase in testCases)
            {
                JoinParameters input = new JoinParameters
                {
                    JoinType = JoinType.Inner,
                    Left = new JoinTable
                    {
                        Data          = leftTable,
                        KeyColumns    = left.key,
                        ResultType    = JoinResult.Row,
                        ResultColumn  = "left"
                    },
                    Right = new JoinTable
                    {
                        Data          = rightTable,
                        KeyColumns    = right.key,
                        ResultType    = testCase.ResultType,
                        ResultColumns = testCase.ResultColumns
                    }
                };

                Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

                Assert.That(result.Columns, Is.EqualTo(testCase.ExpectedColumns));

                // Check that each row has the columns in the new column order
                foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
                {
                    var keys = row.Select(x => x.Key);

                    Assert.That(keys, Is.EqualTo(testCase.ExpectedColumns));
                }
            }
        }

        [Test]
        public void BothLeftAndRightRowsCanBeExpandedAtTheSameTime()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data          = leftTable,
                    KeyColumns    = left.key,
                    ResultType    = JoinResult.AllColumns
                },
                Right = new JoinTable
                {
                    Data          = rightTable,
                    KeyColumns    = right.key,
                    ResultType    = JoinResult.DiscardKey
                }
            };

            Table result = DataTasks.Join(input, new System.Threading.CancellationToken());

            string[] expectedColumns = { "A", "B", "V1", "V2", "V3", "V4", "V5" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumns));
            }
        }

        [Test]
        public void JoinThrowsWhenResultColumnIsMissing()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data          = leftTable,
                    KeyColumns    = left.key,
                    ResultType    = JoinResult.Row
                },
                Right = new JoinTable
                {
                    Data          = rightTable,
                    KeyColumns    = right.key,
                    ResultType    = JoinResult.DiscardKey
                }
            };

            Action executeTask = () => DataTasks.Join(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void JoinThrowsWhenResultColumnsIsEmpty()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data          = leftTable,
                    KeyColumns    = left.key,
                    ResultType    = JoinResult.SelectColumns,
                    ResultColumns = new string[] { }
                },
                Right = new JoinTable
                {
                    Data          = rightTable,
                    KeyColumns    = right.key,
                    ResultType    = JoinResult.DiscardKey
                }
            };

            Action executeTask = () => DataTasks.Join(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void JoinThrowsWhenResultColumnsContainsAnInvalidColumnName()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data          = leftTable,
                    KeyColumns    = left.key,
                    ResultType    = JoinResult.SelectColumns,
                    ResultColumns = new string[] { "A", "B", "I" }
                },
                Right = new JoinTable
                {
                    Data          = rightTable,
                    KeyColumns    = right.key,
                    ResultType    = JoinResult.DiscardKey
                }
            };

            Action executeTask = () => DataTasks.Join(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void JoinThrowsWhenExpandingAColumnWithTheSameNameFromBothTables()
        {
            Table leftTable = Table.From(left.columns, left.matched);

            Table rightTable = TableBuilder
                                .From(Table.From(right.columns, right.matched))
                                .RenameColumns(new Dictionary<string, string> { { "V4", "V1" } })
                                .CreateTable();

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.Inner,
                Left = new JoinTable
                {
                    Data          = leftTable,
                    KeyColumns    = left.key,
                    ResultType    = JoinResult.AllColumns,
                },
                Right = new JoinTable
                {
                    Data          = rightTable,
                    KeyColumns    = right.key,
                    ResultType    = JoinResult.DiscardKey
                }
            };

            Action executeTask = () => DataTasks.Join(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class RenameColumnsTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> { 1,  2,  3,  4,  5,  6 },
            new List<object> { 2,  4,  6,  8, 10, 12 },
            new List<object> { 3,  6,  9, 12, 15, 18 },
            new List<object> { 4,  8, 12, 16, 20, 24 },
        };
        private static readonly ColumnRename[] renamings = new ColumnRename[]
        {
            new ColumnRename { Column = "F", NewName = "W" },
            new ColumnRename { Column = "D", NewName = "Z" },
            new ColumnRename { Column = "C", NewName = "Y" },
            new ColumnRename { Column = "A", NewName = "X" },
        };


        [Test]
        public void RenameColumnsReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                = original,
                Renamings           = renamings,
                PreserveOrder       = true,
                DiscardOtherColumns = false
            };

            Table result = DataTasks.RenameColumns(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void ColumnsAreRenamed()
        {
            Table original = Table.From(columns, rows);

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                = original,
                Renamings           = renamings,
                PreserveOrder       = true,
                DiscardOtherColumns = false
            };

            Table result = DataTasks.RenameColumns(input, new System.Threading.CancellationToken());

            string[] expectedColumns = { "X", "B", "Y", "Z", "E", "W" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void ColumnsCanBeOrderedAccoringToTheColumnMapping()
        {
            Table original = Table.From(columns, rows);

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                = original,
                Renamings           = renamings,
                PreserveOrder       = false,
                DiscardOtherColumns = false
            };

            Table result = DataTasks.RenameColumns(input, new System.Threading.CancellationToken());

            string[] expectedColumns = { "W", "B", "Z", "Y", "E", "X" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void OtherColumnsCanBeDiscardedWhilePreservingColumnOrder()
        {
            Table original = Table.From(columns, rows);

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                = original,
                Renamings           = renamings,
                PreserveOrder       = true,
                DiscardOtherColumns = true // <---
            };

            Table result = DataTasks.RenameColumns(input, new System.Threading.CancellationToken());

            string[] expectedColumns = { "X", "Y", "Z", "W" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void OtherColumnsCanBeDiscardedWhileNotPreservingColumnOrder()
        {
            Table original = Table.From(columns, rows);

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                = original,
                Renamings           = renamings,
                PreserveOrder       = false, // <---
                DiscardOtherColumns = true   // <---
            };

            Table result = DataTasks.RenameColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(renamings.Select(m => m.NewName)));
        }
    }

    [TestFixture]
    class ReorderColumnsTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<string> reversedColumns = columns.Reverse<string>().ToList();
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> { 1,  2,  3,  4,  5,  6 },
            new List<object> { 2,  4,  6,  8, 10, 12 },
            new List<object> { 3,  6,  9, 12, 15, 18 },
            new List<object> { 4,  8, 12, 16, 20, 24 },
        };


        [Test]
        public void ReorderColumnsReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = reversedColumns.ToArray(),
                DiscardOtherColumns = false
            };

            Table reordered = DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            Assert.That(reordered is Table);
            Assert.That(reordered, Is.Not.SameAs(original));
        }

        [Test]
        public void ResultHasColumnsInTheSpecifiedOrder()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = reversedColumns.ToArray(),
                DiscardOtherColumns = false
            };

            Table reordered = DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            Assert.That(reordered.Columns, Is.EqualTo(reversedColumns));
        }

        [Test]
        public void ResultRowsAreInColumnOrder()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = reversedColumns.ToArray(),
                DiscardOtherColumns = false
            };

            Table reordered = DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in reordered.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(reversedColumns));
            }
        }

        [Test]
        public void OrderOfUnspecifiedColumnsDoesNotChange()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "C", "E", "B" },
                DiscardOtherColumns = false
            };

            string[] expectedColumnOrder = { "A", "C", "E", "D", "B", "F" };

            Table reordered = DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            Assert.That(reordered.Columns, Is.EqualTo(expectedColumnOrder));

            // Check that the columns are in the correct order for each row in the result
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in reordered.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumnOrder));
            }
        }

        [Test]
        public void UnspecifiedColumnsCanBeDiscarded()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "C", "E", "B" },
                DiscardOtherColumns = true
            };

            string[] expectedColumns = { "C", "E", "B" };

            Table reordered = DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            Assert.That(reordered.Columns, Is.EqualTo(expectedColumns));

            // Check that each row in the result has only the specified columns (in order)
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in reordered.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(expectedColumns));
            }
        }

        public void ReorderColumnsThrowsWhenColumnOrderHasDuplicates()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "B", "A", "B" },
                DiscardOtherColumns = false
            };

            Action executeTask = () => DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        public void ReorderColumnsThrowsWhenColumnOrderContainsAnInvalidColumn()
        {
            Table original = Table.From(columns, rows);

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "A", "B", "X" },
                DiscardOtherColumns = false
            };

            Action executeTask = () => DataTasks.ReorderColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class SelectColumnsTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> { 1,  2,  3,  4,  5,  6 },
            new List<object> { 2,  4,  6,  8, 10, 12 },
            new List<object> { 3,  6,  9, 12, 15, 18 },
            new List<object> { 4,  8, 12, 16, 20, 24 },
        };


        [Test]
        public void SelectColumnsReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Keep,
                Columns       = new string[] { "A", "B" },
                PreserveOrder = false
            };

            Table result = DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void SpecificColumnsCanBeSelectedInTheSpecifiedOrder()
        {
            Table original = Table.From(columns, rows);

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Keep,
                Columns       = selectedColumns,
                PreserveOrder = false
            };

            Table result = DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(selectedColumns));
        }

        [Test]
        public void SpecificColumnsCanBeSelectedInTheirOriginalOrder()
        {
            Table original = Table.From(columns, rows);

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Keep,
                Columns       = selectedColumns,
                PreserveOrder = true
            };

            Table result = DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(original.Columns.Where(c => selectedColumns.Contains(c))));
        }

        [Test]
        public void SpecificColumnsCanBeDiscarded()
        {
            Table original = Table.From(columns, rows);

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Discard,
                Columns       = selectedColumns
            };

            Table result = DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(original.Columns.Where(c => !selectedColumns.Contains(c))));
        }

        [Test]
        public void ResultRowsAreInColumnOrder()
        {
            Table original = Table.From(columns, rows);

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Keep,
                Columns       = selectedColumns,
                PreserveOrder = false
            };

            Table result = DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(selectedColumns));
            }
        }

        public void SelectColumnsThrowsWhenColumnOrderHasDuplicates()
        {
            Table original = Table.From(columns, rows);

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Keep,
                Columns       = new string[] { "B", "A", "B" },
                PreserveOrder = false
            };

            Action executeTask = () => DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        public void SelectColumnsThrowsWhenColumnOrderContainsAnInvalidColumn()
        {
            Table original = Table.From(columns, rows);

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data          = original,
                Action        = SelectColumnsAction.Keep,
                Columns       = new string[] { "A", "B", "X" },
                PreserveOrder = false
            };

            Action executeTask = () => DataTasks.SelectColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class SortTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A","B","C","D","E","F","I","M","N","U" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            //                  A    B     C         D           E           F       I    M        N                  U
            new List<object> {  0,  true, "T", "04.08.2015", "Foxtrot",     -8.6,   541,  0,       "Puce", "2027-11-15T06:56:47Z" },
            new List<object> {  1,  true, "W", "07.12.2004",   "Tango",    -43.5,   244,  1,       "Teal", "2004-11-20T03:02:28Z" },
            new List<object> {  2,  true, "L", "19.07.2023",    "Echo",   -10.11,  -869,  0,         null, "2015-01-14T00:51:16Z" },
            new List<object> {  3, false, "S", "27.05.2027",    "Alfa",   -66.06,  -761,  1,         null, "2028-03-25T21:49:37Z" },
            new List<object> {  4, false, "Z", "13.10.2014", "Uniform",   -14.72,  -275,  0,  "Goldenrod", "2028-03-16T08:08:43Z" },
            new List<object> {  5,  true, "Y", "05.09.2013",   "Oscar",   -29.71,  -896,  1,      "Green", "2027-08-11T12:32:56Z" },
            new List<object> {  6,  true, "T", "21.07.2003",   "Bravo",     7.05,  -706,  0,      "Khaki", "2013-12-19T14:24:42Z" },
            new List<object> {  7, false, "X", "23.12.2004",   "Bravo",    74.45,   424,  1,       "Mauv", "2013-10-20T18:21:19Z" },
            new List<object> {  8,  true, "P", "23.09.2023","November",    49.35,  -417,  0,         null, "1999-07-27T23:16:03Z" },
            new List<object> {  9,  true, "G", "06.04.2007",   "Tango",    -54.5,    -8,  1,         null, "2017-05-23T19:01:35Z" },
            new List<object> { 10,  true, "Q", "13.03.2025",    "Papa",   -87.98,   594,  0,         null, "2015-07-17T18:30:11Z" },
            new List<object> { 11,  true, "T", "26.02.2017", "Foxtrot",       75,   745,  1,     "Fuscia", "2013-09-27T23:27:52Z" },
            new List<object> { 12, false, "U", "24.06.2002",    "Kilo",   -97.89,  -678,  0,         null, "2028-05-17T04:10:04Z" },
            new List<object> { 13,  true, "X", "10.02.2020",    "Mike",    63.58,   363,  1,     "Maroon", "2024-04-17T07:16:37Z" },
            new List<object> { 14, false, "S", "03.05.2023",   "Delta",   -60.48,   979,  0,  "Goldenrod", "2000-06-10T03:15:18Z" },
            new List<object> { 15, false, "I", "14.09.2029", "Whiskey",    72.45,  -406,  1,       "Pink", "1999-01-19T00:29:17Z" },
            new List<object> { 16,  true, "Q", "24.02.2009",    "Papa",   -80.44,     9,  0,         null, "2013-10-27T06:43:15Z" },
            new List<object> { 17,  true, "R", "11.08.2015", "Uniform",    -26.4,  -293,  1, "Aquamarine", "2022-02-03T08:57:37Z" },
            new List<object> { 18,  true, "T", "07.06.2026",   "Oscar",     27.6,  -592,  0,         null, "2007-10-25T23:44:31Z" },
            new List<object> { 19,  true, "W", "18.03.2000", "Uniform",   -60.79,  -130,  1,         null, "2001-03-09T11:05:58Z" },
        };

        [Test]
        public void SortReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Ascending }
                }
            };

            Table result = DataTasks.Sort(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void SortingASingleColumnAscendingWorks()
        {
            Table original = Table.From(columns, rows);

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Ascending }
                }
            };

            Table result = DataTasks.Sort(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows.Select(row => row.E), Is.Ordered.Ascending);
        }

        [Test]
        public void SortingASingleColumnDescendingWorks()
        {
            Table original = Table.From(columns, rows);

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Descending }
                }
            };

            Table result = DataTasks.Sort(input, new System.Threading.CancellationToken());

            Assert.That(result.Rows.Select(row => row.E), Is.Ordered.Descending);
        }

        [Test]
        public void SortingMultipleColumnsWorks()
        {
            Table original = Table.From(columns, rows);

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Descending },
                    new SortingCriterion { Column = "A", Order = Order.Ascending  }
                }
            };

            Table result = DataTasks.Sort(input, new System.Threading.CancellationToken());

            Assert.That(
                result.Rows.Select(row => new { row.E, row.A }),
                Is.Ordered.Descending.By("E").Then.Ascending.By("A")
            );
        }

        [Test]
        public void SortThrowsWhenAnInvalidColumnIsSpecified()
        {
            Table original = Table.From(columns, rows);

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Descending },
                    new SortingCriterion { Column = "X", Order = Order.Descending }, // <---
                    new SortingCriterion { Column = "A", Order = Order.Ascending  }
                }
            };

            Action executeTask = () => DataTasks.Sort(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class TransformColumnsTaskTests
    {
        private static readonly List<string> columns = new List<string> { "A", "B", "C", "D", "E", "F" };
        private static readonly List<List<object>> rows = new List<List<object>>
        {
            new List<object> { 1,  2,  3,  4,  5,  6 },
            new List<object> { 2,  4,  6,  8, 10, 12 },
            new List<object> { 3,  6,  9, 12, 15, 18 },
            new List<object> { 4,  8, 12, 16, 20, 24 },
            new List<object> { 5, 10, 15, 20, 25, 30 },
        };

        [Test]
        public void TransformColumnsReturnsANewTable()
        {
            Table original = Table.From(columns, rows);

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "A", TransformType = ProcessingType.Row, Transform = row => row.A * 10 },
                }
            };

            Table result = DataTasks.TransformColumns(input, new System.Threading.CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void TransformCanBeDoneUsingRows()
        {
            Table original = Table.From(columns, rows);

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "A", TransformType = ProcessingType.Row, Transform = row => row.A * 10 },
                }
            };

            Table result = DataTasks.TransformColumns(input, new System.Threading.CancellationToken());

            // Check the values in the result are correct.
            // Also ends up making sure that the original rows have not been modified.
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.A == orig.A * 10),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void TransformCanBeDoneUsingColumnValues()
        {
            Table original = Table.From(columns, rows);

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "A", TransformType = ProcessingType.Column, Transform = A => A * 10 },
                }
            };

            Table result = DataTasks.TransformColumns(input, new System.Threading.CancellationToken());

            // Check the values in the result are correct.
            // Also ends up making sure that the original rows have not been modified.
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.A == orig.A * 10),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void TransformColumnsThrowsWhenSpecifyingAnInvalidColumnName()
        {
            Table original = Table.From(columns, rows);

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "X", TransformType = ProcessingType.Row, Transform = row => null },
                }
            };

            Action executeTask = () => DataTasks.TransformColumns(input, new System.Threading.CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }
}
