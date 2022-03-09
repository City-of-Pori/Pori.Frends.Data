using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pori.Frends.Data.Tests
{
    using FilterFunc = Func<dynamic, bool>;

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
            new List<object> { 1,  2,  3,  4,  5,  6 },
            new List<object> { 2,  4,  6,  8, 10, 12 },
            new List<object> { 3,  6,  9, 12, 15, 18 },
            new List<object> { 4,  8, 12, 16, 20, 24 },
            new List<object> { 5, 10, 15, 20, 25, 30 },
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
