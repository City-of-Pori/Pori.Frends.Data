using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Pori.Frends.Data.Tests
{
    using TableFunc  = Func<dynamic, dynamic>;
    using FilterFunc = Func<dynamic, bool>;
    using RowDict    = IDictionary<string, dynamic>;

    public class TestData
    {
        public static readonly InlineTable Typed = new InlineTable
        {
            { "A",  "B", "C",     "D",         "E",        "F",    "I", "M",     "N",               "U"          },
            /****************************************************************************************************/
            {  0,  true, "T", "04.08.2015", "Foxtrot",     -8.6,   541,  0,       "Puce", "2027-11-15T06:56:47Z" },
            {  1,  true, "W", "07.12.2004",   "Tango",    -43.5,   244,  1,       "Teal", "2004-11-20T03:02:28Z" },
            {  2,  true, "L", "19.07.2023",    "Echo",   -10.11,  -869,  0,         null, "2015-01-14T00:51:16Z" },
            {  3, false, "S", "27.05.2027",    "Alfa",   -66.06,  -761,  1,         null, "2028-03-25T21:49:37Z" },
            {  4, false, "Z", "13.10.2014", "Uniform",   -14.72,  -275,  0,  "Goldenrod", "2028-03-16T08:08:43Z" },
            {  5,  true, "Y", "05.09.2013",   "Oscar",   -29.71,  -896,  1,      "Green", "2027-08-11T12:32:56Z" },
            {  6,  true, "T", "21.07.2003",   "Bravo",     7.05,  -706,  0,      "Khaki", "2013-12-19T14:24:42Z" },
            {  7, false, "X", "23.12.2004",   "Bravo",    74.45,   424,  1,       "Mauv", "2013-10-20T18:21:19Z" },
            {  8,  true, "P", "23.09.2023","November",    49.35,  -417,  0,         null, "1999-07-27T23:16:03Z" },
            {  9,  true, "G", "06.04.2007",   "Tango",    -54.5,    -8,  1,         null, "2017-05-23T19:01:35Z" },
            { 10,  true, "Q", "13.03.2025",    "Papa",   -87.98,   594,  0,         null, "2015-07-17T18:30:11Z" },
            { 11,  true, "T", "26.02.2017", "Foxtrot",     75.0,   745,  1,     "Fuscia", "2013-09-27T23:27:52Z" },
            { 12, false, "U", "24.06.2002",    "Kilo",   -97.89,  -678,  0,         null, "2028-05-17T04:10:04Z" },
            { 13,  true, "X", "10.02.2020",    "Mike",    63.58,   363,  1,     "Maroon", "2024-04-17T07:16:37Z" },
            { 14, false, "S", "03.05.2023",   "Delta",   -60.48,   979,  0,  "Goldenrod", "2000-06-10T03:15:18Z" },
            { 15, false, "I", "14.09.2029", "Whiskey",    72.45,  -406,  1,       "Pink", "1999-01-19T00:29:17Z" },
            { 16,  true, "Q", "24.02.2009",    "Papa",   -80.44,     9,  0,         null, "2013-10-27T06:43:15Z" },
            { 17,  true, "R", "11.08.2015", "Uniform",    -26.4,  -293,  1, "Aquamarine", "2022-02-03T08:57:37Z" },
            { 18,  true, "T", "07.06.2026",   "Oscar",     27.6,  -592,  0,         null, "2007-10-25T23:44:31Z" },
            { 19,  true, "W", "18.03.2000", "Uniform",   -60.79,  -130,  1,         null, "2001-03-09T11:05:58Z" },
        };

        public static readonly InlineTable Untyped = new InlineTable
        {
            {  "A",   "B",   "C",      "D",        "E",        "F",       "I",   "M",     "N",                "U"          },
            /***************************************************************************************************************/
            {  "0",  "true", "T", "04.08.2015", "Foxtrot",     "-8.6",   "541",  "0",       "Puce", "2027-11-15T06:56:47Z" },
            {  "1",  "true", "W", "07.12.2004",   "Tango",    "-43.5",   "244",  "1",       "Teal", "2004-11-20T03:02:28Z" },
            {  "2",  "true", "L", "19.07.2023",    "Echo",   "-10.11",  "-869",  "0",         null, "2015-01-14T00:51:16Z" },
            {  "3", "false", "S", "27.05.2027",    "Alfa",   "-66.06",  "-761",  "1",         null, "2028-03-25T21:49:37Z" },
            {  "4", "false", "Z", "13.10.2014", "Uniform",   "-14.72",  "-275",  "0",  "Goldenrod", "2028-03-16T08:08:43Z" },
            {  "5",  "true", "Y", "05.09.2013",   "Oscar",   "-29.71",  "-896",  "1",      "Green", "2027-08-11T12:32:56Z" },
            {  "6",  "true", "T", "21.07.2003",   "Bravo",     "7.05",  "-706",  "0",      "Khaki", "2013-12-19T14:24:42Z" },
            {  "7", "false", "X", "23.12.2004",   "Bravo",    "74.45",   "424",  "1",       "Mauv", "2013-10-20T18:21:19Z" },
            {  "8",  "true", "P", "23.09.2023","November",    "49.35",  "-417",  "0",         null, "1999-07-27T23:16:03Z" },
            {  "9",  "true", "G", "06.04.2007",   "Tango",    "-54.5",    "-8",  "1",         null, "2017-05-23T19:01:35Z" },
            { "10",  "true", "Q", "13.03.2025",    "Papa",   "-87.98",   "594",  "0",         null, "2015-07-17T18:30:11Z" },
            { "11",  "true", "T", "26.02.2017", "Foxtrot",       "75",   "745",  "1",     "Fuscia", "2013-09-27T23:27:52Z" },
            { "12", "false", "U", "24.06.2002",    "Kilo",   "-97.89",  "-678",  "0",         null, "2028-05-17T04:10:04Z" },
            { "13",  "true", "X", "10.02.2020",    "Mike",    "63.58",   "363",  "1",     "Maroon", "2024-04-17T07:16:37Z" },
            { "14", "false", "S", "03.05.2023",   "Delta",   "-60.48",   "979",  "0",  "Goldenrod", "2000-06-10T03:15:18Z" },
            { "15", "false", "I", "14.09.2029", "Whiskey",    "72.45",  "-406",  "1",       "Pink", "1999-01-19T00:29:17Z" },
            { "16",  "true", "Q", "24.02.2009",    "Papa",   "-80.44",     "9",  "0",         null, "2013-10-27T06:43:15Z" },
            { "17",  "true", "R", "11.08.2015", "Uniform",    "-26.4",  "-293",  "1", "Aquamarine", "2022-02-03T08:57:37Z" },
            { "18",  "true", "T", "07.06.2026",   "Oscar",     "27.6",  "-592",  "0",         null, "2007-10-25T23:44:31Z" },
            { "19",  "true", "W", "18.03.2000", "Uniform",   "-60.79",  "-130",  "1",         null, "2001-03-09T11:05:58Z" },
        };
    }

    public class CsvInputData
    {
        public List<string> Headers { get; set; }
        public List<List<object>> Data { get; set; }
    }

    [TestFixture]
    class TableTests
    {
        [Test]
        public void TableRowsAreEnumerable()
        {
            Table table = TestData.Typed;

            // Check that each row is a collection of key-value pairs
            foreach(var row in table.Rows)
                Assert.That(row is IEnumerable<KeyValuePair<string, dynamic>>);
        }

        [Test]
        public void EnumeratingTableRowsProducesTheRowsInOrder()
        {
            Table table = TestData.Typed;

            Assert.That(table.Rows.Cast<RowDict>().Select(row => row.Values), Is.EqualTo(TestData.Typed.Rows));
        }

        [Test]
        public void TableRowsAreInColumnOrder()
        {
            Table table = TestData.Typed;

            // Check that each row has the columns in the table's column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in table.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(TestData.Typed.Columns));
            }
        }

        [Test]
        public void TableCanBeConvertedToCsvRows()
        {
            Table original = TestData.Typed;

            var csvRows = original.ToCsvRows();

            Table result = Table.From(original.Columns, csvRows);

            Assert.That(result.Rows, Is.EqualTo(original.Rows));
        }

        [Test]
        public void TableCanBeConvertedToJson()
        {
            var rows = new []
            {
                new { A = 1, B = "foo" },
                new { A = 2, B = "bar" },
                new { A = 3, B = "baz" },
            };

            var data = JArray.FromObject(rows);

            var input = new LoadParameters
            {
                Format = LoadFormat.JSON,
                Json   = new LoadJsonParameters
                {
                    Columns = new [] { "A", "B" },
                    Data    = data
                }
            };

            Table table = TableTasks.Load(input, CommonOptions.Defaults, new CancellationToken());

            JToken result = table.ToJson();

            Assert.That(JToken.DeepEquals(result, data));
        }

        [Test]
        public void TableCanBeConvertedToXml()
        {
            Table table = TestData.Typed;

            string expectedDecl = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            string expectedXml = @"
                <table>
                    <row><A>0</A><B>true</B><C>T</C><D>04.08.2015</D><E>Foxtrot</E><F>-8.6</F><I>541</I><M>0</M><N>Puce</N><U>2027-11-15T06:56:47Z</U></row>
                    <row><A>1</A><B>true</B><C>W</C><D>07.12.2004</D><E>Tango</E><F>-43.5</F><I>244</I><M>1</M><N>Teal</N><U>2004-11-20T03:02:28Z</U></row>
                    <row><A>2</A><B>true</B><C>L</C><D>19.07.2023</D><E>Echo</E><F>-10.11</F><I>-869</I><M>0</M><N /><U>2015-01-14T00:51:16Z</U></row>
                    <row><A>3</A><B>false</B><C>S</C><D>27.05.2027</D><E>Alfa</E><F>-66.06</F><I>-761</I><M>1</M><N /><U>2028-03-25T21:49:37Z</U></row>
                    <row><A>4</A><B>false</B><C>Z</C><D>13.10.2014</D><E>Uniform</E><F>-14.72</F><I>-275</I><M>0</M><N>Goldenrod</N><U>2028-03-16T08:08:43Z</U></row>
                    <row><A>5</A><B>true</B><C>Y</C><D>05.09.2013</D><E>Oscar</E><F>-29.71</F><I>-896</I><M>1</M><N>Green</N><U>2027-08-11T12:32:56Z</U></row>
                    <row><A>6</A><B>true</B><C>T</C><D>21.07.2003</D><E>Bravo</E><F>7.05</F><I>-706</I><M>0</M><N>Khaki</N><U>2013-12-19T14:24:42Z</U></row>
                    <row><A>7</A><B>false</B><C>X</C><D>23.12.2004</D><E>Bravo</E><F>74.45</F><I>424</I><M>1</M><N>Mauv</N><U>2013-10-20T18:21:19Z</U></row>
                    <row><A>8</A><B>true</B><C>P</C><D>23.09.2023</D><E>November</E><F>49.35</F><I>-417</I><M>0</M><N /><U>1999-07-27T23:16:03Z</U></row>
                    <row><A>9</A><B>true</B><C>G</C><D>06.04.2007</D><E>Tango</E><F>-54.5</F><I>-8</I><M>1</M><N /><U>2017-05-23T19:01:35Z</U></row>
                    <row><A>10</A><B>true</B><C>Q</C><D>13.03.2025</D><E>Papa</E><F>-87.98</F><I>594</I><M>0</M><N /><U>2015-07-17T18:30:11Z</U></row>
                    <row><A>11</A><B>true</B><C>T</C><D>26.02.2017</D><E>Foxtrot</E><F>75</F><I>745</I><M>1</M><N>Fuscia</N><U>2013-09-27T23:27:52Z</U></row>
                    <row><A>12</A><B>false</B><C>U</C><D>24.06.2002</D><E>Kilo</E><F>-97.89</F><I>-678</I><M>0</M><N /><U>2028-05-17T04:10:04Z</U></row>
                    <row><A>13</A><B>true</B><C>X</C><D>10.02.2020</D><E>Mike</E><F>63.58</F><I>363</I><M>1</M><N>Maroon</N><U>2024-04-17T07:16:37Z</U></row>
                    <row><A>14</A><B>false</B><C>S</C><D>03.05.2023</D><E>Delta</E><F>-60.48</F><I>979</I><M>0</M><N>Goldenrod</N><U>2000-06-10T03:15:18Z</U></row>
                    <row><A>15</A><B>false</B><C>I</C><D>14.09.2029</D><E>Whiskey</E><F>72.45</F><I>-406</I><M>1</M><N>Pink</N><U>1999-01-19T00:29:17Z</U></row>
                    <row><A>16</A><B>true</B><C>Q</C><D>24.02.2009</D><E>Papa</E><F>-80.44</F><I>9</I><M>0</M><N /><U>2013-10-27T06:43:15Z</U></row>
                    <row><A>17</A><B>true</B><C>R</C><D>11.08.2015</D><E>Uniform</E><F>-26.4</F><I>-293</I><M>1</M><N>Aquamarine</N><U>2022-02-03T08:57:37Z</U></row>
                    <row><A>18</A><B>true</B><C>T</C><D>07.06.2026</D><E>Oscar</E><F>27.6</F><I>-592</I><M>0</M><N /><U>2007-10-25T23:44:31Z</U></row>
                    <row><A>19</A><B>true</B><C>W</C><D>18.03.2000</D><E>Uniform</E><F>-60.79</F><I>-130</I><M>1</M><N /><U>2001-03-09T11:05:58Z</U></row>
                </table>";

            string RemoveWhitespace(string text)
            {
                var nonWhitespace = text.ToCharArray()
                                        .Where(c => !char.IsWhiteSpace(c))
                                        .ToArray();

                return new string(nonWhitespace);
            }

            Assert.That(RemoveWhitespace(table.ToXml()), Is.EqualTo(RemoveWhitespace(expectedDecl + expectedXml)));
            Assert.That(RemoveWhitespace(table.ToXml(declaration: false)), Is.EqualTo(RemoveWhitespace(expectedXml)));
        }
    }

    [TestFixture]
    class TableBuilderTests
    {
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
            Table original = TestData.Typed;

            Table result = TableBuilder
                            .From(original)
                            .CreateTable();

            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void ConcatenateWorks()
        {
            Table fullTable = TestData.Typed;
            var rows        = TestData.Typed.Rows;
            var columns     = TestData.Typed.Columns;

            Table[] tables =
            {
                Table.From(columns, rows.Skip(0).Take(8)),
                Table.From(columns, rows.Skip(8).Take(8)),
                Table.From(columns, rows.Skip(16).Take(8))
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
            Table      original = TestData.Typed;
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
            Table      original = TestData.Typed;
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
            Table    original        = TestData.Typed;
            string[] keyColumns      = { "E" };
            string[] expectedColumns = { "E", "G" };
            var      keys            = original.Rows
                                        .Select(row => row.E)
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
                grouped.Rows.Select(row => row.E),
                Is.EquivalentTo(keys)
            );

            // Check that each group's elements has the correct key
            foreach(var row in grouped.Rows)
                Assert.That(row.G, Has.All.Matches<dynamic>(elem => elem.E == row.E));

            // Check that the result contains all the rows of the original table
            Assert.That(
                grouped.Rows.SelectMany(row => row.G as IEnumerable<dynamic>),
                Is.EquivalentTo(original.Rows)
            );
        }

        [Test]
        public void RenameColumnsDoesTheRename()
        {
            Table original = TestData.Typed;

            Table result = TableBuilder
                            .From(original)
                            .RenameColumns(renamings.ToDictionary(r => r.Column, r => r.NewName))
                            .CreateTable();

            string[] expectedColumns = { "X", "B", "Y", "Z", "E", "W", "I", "M", "N", "U" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void ReorderColumnsResultsInCorrectColumnOrder()
        {
            Table original = TestData.Typed;
            var   reversedColumns = TestData.Typed.Columns.Reverse();

            Table reordered = TableBuilder
                                .From(original)
                                .ReorderColumns(reversedColumns.ToArray())
                                .CreateTable();

            Assert.That(reordered.Columns, Is.EqualTo(reversedColumns));
        }

        [Test]
        public void ReorderColumnsReordersRowColumnOrder()
        {
            Table original = TestData.Typed;
            var   reversedColumns = TestData.Typed.Columns.Reverse();

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
            Table original = TestData.Typed;

            Table reordered = TableBuilder
                                .From(original)
                                .ReorderColumns(new [] { "C", "E", "B" })
                                .CreateTable();

            string[] expectedColumnOrder = { "A", "C", "E", "D", "B", "F", "I", "M", "N", "U" };

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
            Table original = TestData.Typed;

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
            Table original = TestData.Typed;

            Table result = TableBuilder
                            .From(original)
                            .AddColumn("G", row => "foo")
                            .CreateTable();

            string[] expectedColumns = TestData.Typed.Columns.Concat(new [] {"G"}).ToArray();

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
            Table original = TestData.Typed;

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
            Table original = TestData.Typed;

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
            Table original = TestData.Typed;

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
                Csv    = new LoadCsvParameters
                {
                    Data = CsvData
                }
            };


            Table result = TableTasks.Load(input, CommonOptions.Defaults, new CancellationToken());

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

        [Test]
        public void CustomDataCanBeLoaded()
        {
            Table source = TestData.Typed;

            var input = new LoadParameters
            {
                Format = LoadFormat.Custom,
                Custom = new LoadCustomParameters
                {
                    Data         = source.Rows,
                    Columns      = new [] { "A", "B", "I" },
                    ColumnLoader = (row, column) => (row as RowDict)[column]
                }
            };

            Table result = TableTasks.Load(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Columns, Is.EqualTo(new[] { "A", "B", "I" }));
            Assert.That(result.Count, Is.EqualTo(source.Count));

            foreach(var (resultRow, sourceRow) in result.Rows.Zip(source.Rows, (r, s) => (r, s)))
            {
                Assert.That(resultRow.A, Is.EqualTo(sourceRow.A));
                Assert.That(resultRow.B, Is.EqualTo(sourceRow.B));
                Assert.That(resultRow.I, Is.EqualTo(sourceRow.I));
            }
        }

        [Test]
        public void CustomDataLoadingCanDiscardErrors()
        {
            Table source = TestData.Typed;

            var input = new LoadParameters
            {
                Format = LoadFormat.Custom,
                Custom = new LoadCustomParameters
                {
                    Data         = source.Rows,
                    Columns      = new [] { "N" },
                    ColumnLoader = (row, column) => (row as RowDict)[column].Substring(0)
                }
            };

            var options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Discard
            };

            Table result = TableTasks.Load(input, options, new CancellationToken());

            var expectedRows = source.Rows.Where(row => row.N != null);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Columns, Is.EqualTo(new[] { "N" }));
            Assert.That(result.Count, Is.EqualTo(expectedRows.Count()));
            Assert.That(result.Errors.Count, Is.EqualTo(source.Rows.Where(row => row.N == null).Count()));

            foreach(var (resultRow, sourceRow) in result.Rows.Zip(expectedRows, (r, s) => (r, s)))
                Assert.That(resultRow.N, Is.EqualTo(sourceRow.N));
        }

        [Test]
        public void CustomDataLoadingCanIgnoreErrors()
        {
            Table source = TestData.Typed;

            var input = new LoadParameters
            {
                Format = LoadFormat.Custom,
                Custom = new LoadCustomParameters
                {
                    Data         = source.Rows,
                    Columns      = new [] { "N" },
                    ColumnLoader = (row, column) => (row as RowDict)[column].Substring(0)
                }
            };

            var options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Continue
            };

            Table result = TableTasks.Load(input, options, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Columns, Is.EqualTo(new[] { "N" }));
            Assert.That(result.Count, Is.EqualTo(source.Count));
            Assert.That(result.Errors.Count, Is.EqualTo(source.Rows.Where(row => row.N == null).Count()));

            foreach(var (resultRow, sourceRow) in result.Rows.Zip(source.Rows, (r, s) => (r, s)))
                Assert.That(resultRow.N, Is.EqualTo(sourceRow.N));
        }

        [Test]
        public void CustomDataLoadingCanCollectErrorsAndThenFail()
        {
            Table source = TestData.Typed;

            var input = new LoadParameters
            {
                Format = LoadFormat.Custom,
                Custom = new LoadCustomParameters
                {
                    Data         = source.Rows,
                    Columns      = new [] { "N" },
                    ColumnLoader = (row, column) => (row as RowDict)[column].Substring(0)
                }
            };

            var options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.ContinueAndFail
            };

            Action executeTask = () => TableTasks.Load(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == source.Rows.Where(row => row.N == null).Count())
            );
        }

        [Test]
        public void JsonDataIsLoadedCorrectly()
        {
            var rows = new []
            {
                new { A = 1, B = "foo" },
                new { A = 2, B = "bar" },
                new { A = 3, B = "baz" },
            };

            var data = JArray.FromObject(rows);

            var input = new LoadParameters
            {
                Format = LoadFormat.JSON,
                Json   = new LoadJsonParameters
                {
                    Columns = new [] { "A", "B" },
                    Data    = data
                }
            };

            Table result = TableTasks.Load(input, CommonOptions.Defaults, new CancellationToken());

            string[] expectedColumns = { "A", "B" };

            Assert.That(result is Table);
            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
            Assert.That(result.Rows.Count, Is.EqualTo(rows.Length));

            foreach(var (origRow, resRow) in rows.Zip(result.Rows, (o, r) => (o, r)))
            {
                Console.WriteLine(resRow.A);
                Console.WriteLine(origRow.A);
                Assert.That(origRow.A == resRow.A);
                Assert.That(origRow.B == resRow.B);
            }
        }

        [Test]
        public void JsonDataLoadingFailsOnErrorsByDefault()
        {
            var rows = new []
            {
                new { A = 1, B = "foo" },
                new { A = 2, B = "bar" },
                new { A = 3, B = "baz" },
            };

            var data = JArray.FromObject(rows);

            // Add a null to the end of the rows to cause an error
            data.Add(null);

            var input = new LoadParameters
            {
                Format = LoadFormat.JSON,
                Json   = new LoadJsonParameters
                {
                    Columns = new [] { "A", "B" },
                    Data    = data
                }
            };

            Action executeTask = () => TableTasks.Load(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.TypeOf<Table.Error>());
        }

        [Test]
        public void JsonDataLoadingCanIgnoreErrors()
        {
            var rows = new []
            {
                new { A = 1, B = "foo" },
                new { A = 2, B = "bar" },
                new { A = 3, B = "baz" },
            };

            var data = JArray.FromObject(rows);

            // Add a null to the rows to cause an error
            data.Add(null);

            var input = new LoadParameters
            {
                Format = LoadFormat.JSON,
                Json   = new LoadJsonParameters
                {
                    Columns = new [] { "A", "B" },
                    Data    = data
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Continue
            };

            Table result = TableTasks.Load(input, options, new CancellationToken());

            string[] expectedColumns = { "A", "B" };

            Assert.That(result is Table);
            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
            Assert.That(result.Rows.Count, Is.EqualTo(rows.Length + 1));

            foreach(var (origRow, resRow) in rows.Zip(result.Rows, (o, r) => (o, r)))
            {
                Assert.That(origRow.A == resRow.A);
                Assert.That(origRow.B == resRow.B);
            }

            Assert.That(result.Rows.Last().A == null && result.Rows.Last().B == null);
        }

        [Test]
        public void JsonDataLoadingCanDiscardErrors()
        {
            var rows = new []
            {
                new { A = 1, B = "foo" },
                new { A = 2, B = "bar" },
                new { A = 3, B = "baz" },
            };

            var data = JArray.FromObject(rows);

            // Add a null to the end of the rows to cause an error
            data.Insert(1, null);

            var input = new LoadParameters
            {
                Format = LoadFormat.JSON,
                Json   = new LoadJsonParameters
                {
                    Columns = new [] { "A", "B" },
                    Data    = data
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Discard
            };

            Table result = TableTasks.Load(input, options, new CancellationToken());

            string[] expectedColumns = { "A", "B" };

            Assert.That(result is Table);
            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
            Assert.That(result.Rows.Count, Is.EqualTo(rows.Length));

            foreach(var (origRow, resRow) in rows.Where(row => row != null).Zip(result.Rows, (o, r) => (o, r)))
            {
                Console.WriteLine(resRow.A);
                Console.WriteLine(origRow.A);
                Assert.That(origRow.A == resRow.A);
                Assert.That(origRow.B == resRow.B);
            }
        }

        [Test]
        public void JsonDataLoadingCanCollectErrorsAndFail()
        {
            var rows = new []
            {
                new { A = 1, B = "foo" },
                new { A = 2, B = "bar" },
                new { A = 3, B = "baz" },
            };

            var data = JArray.FromObject(rows);

            // Add a null to the end of the rows to cause an error
            data.Insert(1, null);

            var input = new LoadParameters
            {
                Format = LoadFormat.JSON,
                Json   = new LoadJsonParameters
                {
                    Columns = new [] { "A", "B" },
                    Data    = data
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.ContinueAndFail
            };

            Action executeTask = () => TableTasks.Load(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == 1)
            );
        }

        private static string XmlData =
            @"<?xml version='1.0' encoding='utf-8'?>
            <entityset>
                <entity id='987654321' name='FOO220417K'>
                    <template id='2399' name='Työasema' code='workstation'/>
                    <group code='workstations'/>
                    <attribute id='774219' name='Nimi' code='device_name'>
                        <value>FOO220417K</value>
                    </attribute>
                    <attribute id='774250' name='Sarjanumero' code='serial_number'>
                        <value>ABCDEF12345</value>
                    </attribute>
                    <attribute id='2408' name='Työaseman malli' code='device_model'>
                        <reference id='9563700' name='HP EliteBook 830 G5'/>
                    </attribute>
                    <attribute id='774535' name='Kustannuspaikka' code='cost_center'>
                        <reference id='790606' name='FOO Mumble'/>
                    </attribute>
                    <attribute id='2302' name='Levyt' code='drives'>
                        <reference id='9578833' name='C:'/>
                        <reference id='9840398' name='D:'/>
                        <reference id='9840393' name='G:'/>
                    </attribute>
                    <attribute id='2318' name='Näytönohjain' code='display_adapter'>
                        <value>Citrix Indirect Display Adapter</value>
                        <value>Intel(R) UHD Graphics 620</value>
                    </attribute>
                </entity>
                <entity id='-1' name='BROKEN' invalid=''> <!-- Invalid element for testing error handling -->
                    <template id='2399' name='Työasema' code='workstation'/>
                    <group code='workstations'/>
                    <attribute id='774219' name='Nimi' code='device_name'>
                        <value>BROKEN</value>
                    </attribute>
                    <attribute id='774250' name='Sarjanumero' code='serial_number'>
                        <value>XYZ547123</value>
                    </attribute>
                    <attribute id='2408' name='Työaseman malli' code='device_model'>
                        <reference id='9563700' name='HP EliteBook 830 G5'/>
                    </attribute>
                    <attribute id='774535' name='Kustannuspaikka' code='cost_center'>
                        <reference id='790606' name='NIL Null'/>
                    </attribute>
                    <attribute id='2302' name='Levyt' code='drives'>
                        <reference id='9578833' name='C:'/>
                        <reference id='9840398' name='D:'/>
                        <reference id='9840393' name='G:'/>
                    </attribute>
                    <attribute id='2318' name='Näytönohjain' cod='display_adapter'>
                        <value>Citrix Indirect Display Adapter</value>
                        <value>Intel(R) UHD Graphics 620</value>
                    </attribute>
                </entity>
                <entity id='123456789' name='BAR220417K'>
                    <template id='2399' name='Työasema' code='workstation'/>
                    <group code='workstations'/>
                    <attribute id='774219' name='Nimi' code='device_name'>
                        <value>BAR220417K</value>
                    </attribute>
                    <attribute id='774250' name='Sarjanumero' code='serial_number'>
                        <value>XYZ547123</value>
                    </attribute>
                    <attribute id='2408' name='Työaseman malli' code='device_model'>
                        <reference id='9563700' name='HP EliteBook 830 G5'/>
                    </attribute>
                    <attribute id='774535' name='Kustannuspaikka' code='cost_center'>
                        <reference id='790606' name='BAR Blah'/>
                    </attribute>
                    <attribute id='2302' name='Levyt' code='drives'>
                        <reference id='9578833' name='C:'/>
                        <reference id='9840398' name='D:'/>
                        <reference id='9840393' name='G:'/>
                    </attribute>
                    <attribute id='2318' name='Näytönohjain' code='display_adapter'>
                        <value>Citrix Indirect Display Adapter</value>
                        <value>Intel(R) UHD Graphics 620</value>
                    </attribute>
                </entity>
            </entityset>
        ";

        [Test]
        public void XmlDataLoadingWorks()
        {
            var expectedXmlResult = new []
            {
                new
                {
                    template        = "workstation",
                    device_name     = "FOO220417K",
                    serial_number   = "ABCDEF12345",
                    display_adapter = new [] { "Citrix Indirect Display Adapter", "Intel(R) UHD Graphics 620" }
                },
                new
                {
                    template        = "workstation",
                    device_name     = "BAR220417K",
                    serial_number   = "XYZ547123",
                    display_adapter = new [] { "Citrix Indirect Display Adapter", "Intel(R) UHD Graphics 620" }
                }
            };

            var input = new LoadParameters
            {
                Format = LoadFormat.XML,
                Xml = new LoadXmlParameters
                {
                    Columns  = new [] { "template", "device_name", "serial_number", "display_adapter" },
                    Data     = XmlData,
                    RowsPath = "//entity[not(@invalid)]",
                    ColumnSources = new XmlColumnSource[]
                    {
                        new XmlColumnSource
                        {
                            Type       = XmlColumnSourceType.SingleColumn,
                            ColumnName = "template",
                            ValuePath  = "./template/@code",
                            ValueType  = XmlColumnValueType.SingleValue
                        },
                        new XmlColumnSource
                        {
                            Type            = XmlColumnSourceType.MultipleColumns,
                            ColumnPath      = "attribute[count(value)=1]",
                            ColumnNamePath  = "@code",
                            ValuePath       = "value",
                            ValueType       = XmlColumnValueType.SingleValue
                        },
                        new XmlColumnSource
                        {
                            Type            = XmlColumnSourceType.MultipleColumns,
                            ColumnPath      = "attribute[count(value)>1]",
                            ColumnNamePath  = "@code",
                            ValuePath       = "value",
                            ValueType       = XmlColumnValueType.MultipleValues
                        }
                    }
                }
            };

            Table result = TableTasks.Load(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Count, Is.EqualTo(expectedXmlResult.Count()));

            foreach(var (resultRow, expectedRow) in result.Rows.Zip(expectedXmlResult, (r, e) => (r, e)))
            {
                Assert.That(resultRow.template, Is.EqualTo(expectedRow.template));
                Assert.That(resultRow.device_name, Is.EqualTo(expectedRow.device_name));
                Assert.That(resultRow.serial_number, Is.EqualTo(expectedRow.serial_number));
                Assert.That(resultRow.display_adapter, Is.EqualTo(expectedRow.display_adapter));
            }
        }

        [Test]
        public void XmlDataLoadingCanDiscardErroneousRows()
        {
            var expectedXmlResult = new []
            {
                new
                {
                    template        = "workstation",
                    device_name     = "FOO220417K",
                    serial_number   = "ABCDEF12345",
                    display_adapter = new [] { "Citrix Indirect Display Adapter", "Intel(R) UHD Graphics 620" }
                },
                new
                {
                    template        = "workstation",
                    device_name     = "BAR220417K",
                    serial_number   = "XYZ547123",
                    display_adapter = new [] { "Citrix Indirect Display Adapter", "Intel(R) UHD Graphics 620" }
                }
            };

            var input = new LoadParameters
            {
                Format = LoadFormat.XML,
                Xml = new LoadXmlParameters
                {
                    Columns  = new [] { "template", "device_name", "serial_number", "display_adapter" },
                    Data     = XmlData,
                    RowsPath = "//entity",
                    ColumnSources = new XmlColumnSource[]
                    {
                        new XmlColumnSource
                        {
                            Type       = XmlColumnSourceType.SingleColumn,
                            ColumnName = "template",
                            ValuePath  = "./template/@code",
                            ValueType  = XmlColumnValueType.SingleValue
                        },
                        new XmlColumnSource
                        {
                            Type            = XmlColumnSourceType.MultipleColumns,
                            ColumnPath      = "attribute[count(value)=1]",
                            ColumnNamePath  = "@code",
                            ValuePath       = "value",
                            ValueType       = XmlColumnValueType.SingleValue
                        },
                        new XmlColumnSource
                        {
                            Type            = XmlColumnSourceType.MultipleColumns,
                            ColumnPath      = "attribute[count(value)>1]",
                            ColumnNamePath  = "@code",
                            ValuePath       = "value",
                            ValueType       = XmlColumnValueType.MultipleValues
                        }
                    }
                }
            };

            var options = new CommonOptions { ErrorHandling = Table.ErrorHandling.Discard };

            Table result = TableTasks.Load(input, options, new CancellationToken());

            Assert.That(result.Count, Is.EqualTo(expectedXmlResult.Count()));
            Assert.That(result.Errors.Count, Is.EqualTo(1));

            foreach(var (resultRow, expectedRow) in result.Rows.Zip(expectedXmlResult, (r, e) => (r, e)))
            {
                Assert.That(resultRow.template, Is.EqualTo(expectedRow.template));
                Assert.That(resultRow.device_name, Is.EqualTo(expectedRow.device_name));
                Assert.That(resultRow.serial_number, Is.EqualTo(expectedRow.serial_number));
                Assert.That(resultRow.display_adapter, Is.EqualTo(expectedRow.display_adapter));
            }
        }

        [Test]
        public void XmlDataLoadingCanCollectErrorsAndThenFail()
        {
            var expectedXmlResult = new []
            {
                new
                {
                    template        = "workstation",
                    device_name     = "FOO220417K",
                    serial_number   = "ABCDEF12345",
                    display_adapter = new [] { "Citrix Indirect Display Adapter", "Intel(R) UHD Graphics 620" }
                },
                new
                {
                    template        = "workstation",
                    device_name     = "BAR220417K",
                    serial_number   = "XYZ547123",
                    display_adapter = new [] { "Citrix Indirect Display Adapter", "Intel(R) UHD Graphics 620" }
                }
            };

            var input = new LoadParameters
            {
                Format = LoadFormat.XML,
                Xml = new LoadXmlParameters
                {
                    Columns  = new [] { "template", "device_name", "serial_number", "display_adapter" },
                    Data     = XmlData,
                    RowsPath = "//entity",
                    ColumnSources = new XmlColumnSource[]
                    {
                        new XmlColumnSource
                        {
                            Type       = XmlColumnSourceType.SingleColumn,
                            ColumnName = "template",
                            ValuePath  = "./template/@code",
                            ValueType  = XmlColumnValueType.SingleValue
                        },
                        new XmlColumnSource
                        {
                            Type            = XmlColumnSourceType.MultipleColumns,
                            ColumnPath      = "attribute[count(value)=1]",
                            ColumnNamePath  = "@code",
                            ValuePath       = "value",
                            ValueType       = XmlColumnValueType.SingleValue
                        },
                        new XmlColumnSource
                        {
                            Type            = XmlColumnSourceType.MultipleColumns,
                            ColumnPath      = "attribute[count(value)>1]",
                            ColumnNamePath  = "@code",
                            ValuePath       = "value",
                            ValueType       = XmlColumnValueType.MultipleValues
                        }
                    }
                }
            };

            var options = new CommonOptions { ErrorHandling = Table.ErrorHandling.ContinueAndFail };

            Action executeTask = () => TableTasks.Load(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == 1)
            );
        }
    }

    [TestFixture]
    class AddColumnsTaskTests
    {
        [Test]
        public void AddColumnsReturnsANewTable()
        {
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "G", ValueSource = NewColumnValueSource.Constant, Value = 0 }
                }
            };

            Table result = TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void AddColumnsActuallyAddsTheColumns()
        {
            Table original = TestData.Typed;

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

            
            Table result = TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());


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
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "G", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                    new NewColumn { Name = "H", ValueSource = NewColumnValueSource.Constant, Value = 1 },
                }
            };


            Table result = TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());


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
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "copyOfA", ValueSource = NewColumnValueSource.Computed, ValueGenerator = row => row.A },
                    new NewColumn { Name = "copyOfB", ValueSource = NewColumnValueSource.Computed, ValueGenerator = row => row.B },
                }
            };


            Table result = TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());


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
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "X", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                    new NewColumn { Name = "X", ValueSource = NewColumnValueSource.Constant, Value = 1 },
                }
            };

            Action executeTask = () => TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void AddColumnsThrowsWhenAddingAnExistingColumn()
        {
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn { Name = "A", ValueSource = NewColumnValueSource.Constant, Value = 0 },
                }
            };

            Action executeTask = () => TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void AddColumnsFailsOnErrorsByDefault()
        {
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn
                    {
                        Name           = "firstOfN",
                        ValueSource    = NewColumnValueSource.Computed,
                        ValueGenerator = row => row.N[0]
                    },
                }
            };


            Action executeTask = () => TableTasks.AddColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.TypeOf<Table.Error>());
        }

        [Test]
        public void AddColumnsCanDiscardErroneousRows()
        {
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn
                    {
                        Name           = "firstOfN",
                        ValueSource    = NewColumnValueSource.Computed,
                        ValueGenerator = row => row.N[0]
                    },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Discard
            };

            Table result = TableTasks.AddColumns(input, options, new CancellationToken());

            Assert.That(
                original.Rows.Where(row => row.N != null).Zip(result.Rows, (orig, res) => res.firstOfN == orig.N[0]),
                Has.All.EqualTo(true)
            );
            Assert.That(result.Errors.Count() == original.Rows.Where(row => row.N == null).Count());
        }

        [Test]
        public void AddColumnsCanIgnoreErrors()
        {
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn
                    {
                        Name           = "firstOfN",
                        ValueSource    = NewColumnValueSource.Computed,
                        ValueGenerator = row => row.N[0]
                    },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Continue
            };

            Table result = TableTasks.AddColumns(input, options, new CancellationToken());

            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => (orig.N == null && res.firstOfN == null) || res.firstOfN == orig.N[0]),
                Has.All.EqualTo(true)
            );
            
            Assert.That(result.Errors.Count() == original.Rows.Where(row => row.N == null).Count());
        }

        [Test]
        public void AddColumnsCanFailOnErrorsAfterProcessingAllRows()
        {
            Table original = TestData.Typed;

            AddColumnsParameters input = new AddColumnsParameters
            {
                Data    = original,
                Columns = new NewColumn[]
                {
                    new NewColumn
                    {
                        Name           = "firstOfN",
                        ValueSource    = NewColumnValueSource.Computed,
                        ValueGenerator = row => row.N[0]
                    },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.ContinueAndFail
            };

            Action executeTask = () => TableTasks.AddColumns(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == original.Rows.Where(row => row.N == null).Count())
            );
        }

        
    }

    [TestFixture]
    class ChunkTaskTests
    {
        [Test]
        public void ChunkReturnsTablesOfTheCorrectSize()
        {
            Table source = TestData.Typed;

            foreach(var size in new[] { 6, 5 })
            {
                var input = new ChunkParameters
                {
                    Data = source,
                    Size = size
                };

                List<Table> result = TableTasks.Chunk(input, new CancellationToken());

                Table last = result.Last();

                foreach(var chunk in result)
                {
                    if(chunk != last)
                        Assert.That(chunk.Count, Is.EqualTo(size));
                    else
                        Assert.That(chunk.Count, Is.InRange(1, size));
                }
            }
        }

        [Test]
        public void ChunkResultHasTheCorrectColumns()
        {
            Table source = TestData.Typed;

            foreach(var size in new[] { 50, 6, 5 })
            {
                var input = new ChunkParameters
                {
                    Data = source,
                    Size = size
                };

                List<Table> result = TableTasks.Chunk(input, new CancellationToken());

                foreach(var chunk in result)
                    Assert.That(chunk.Columns, Is.EqualTo(source.Columns));
            }
        }

        [Test]
        public void ChunkResultContainsAllTheOriginalRows()
        {
            Table source = TestData.Typed;

            foreach(var size in new[] { 50, 6, 5 })
            {
                var input = new ChunkParameters
                {
                    Data = source,
                    Size = size
                };

                List<Table> result = TableTasks.Chunk(input, new CancellationToken());

                Table concatenated = TableBuilder
                                        .From(result.First())
                                        .Concatenate(result.Skip(1))
                                        .CreateTable();

                Assert.That(concatenated.Rows, Is.EqualTo(source.Rows));
            }
        }

        [Test]
        public void ChunkFailsWhenGivenAnInvalidChunkSize()
        {
            Table source = TestData.Typed;

            foreach(var size in new[] { 0, -11 })
            {
                var input = new ChunkParameters
                {
                    Data = source,
                    Size = size
                };

                Action executeTask = () => TableTasks.Chunk(input, new CancellationToken());

                Assert.That(executeTask, Throws.Exception);
            }
        }
    }


    [TestFixture]
    class ConcatenateTaskTests
    {
        [Test]
        public void ConcatenateReturnsANewTable()
        {
            Table fullTable = TestData.Typed;

            var rows    = TestData.Typed.Rows;
            var columns = TestData.Typed.Columns;

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = new dynamic []
                {
                    Table.From(columns, rows.Skip(0).Take(8)),
                    Table.From(columns, rows.Skip(8).Take(8)),
                    Table.From(columns, rows.Skip(16).Take(8)),
                }
            };

            Table result = TableTasks.Concatenate(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(fullTable));
        }

        [Test]
        public void ConcatenateReturnsTheCorrectResultWithASingleTable()
        {
            Table fullTable = TestData.Typed;

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = new Table[] { fullTable }
            };

            Table result = TableTasks.Concatenate(input, new CancellationToken());

            Assert.That(result, Is.Not.SameAs(fullTable));
            Assert.That(result.Columns, Is.EqualTo(fullTable.Columns));
            Assert.That(result.Rows, Is.EqualTo(fullTable.Rows));
        }

        [Test]
        public void ConcatenateReturnsTheCorrectResultWithMultipleTables()
        {
            Table fullTable = TestData.Typed;

            var rows    = TestData.Typed.Rows;
            var columns = TestData.Typed.Columns;

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = new dynamic []
                {
                    Table.From(columns, rows.Skip(0).Take(8)),
                    Table.From(columns, rows.Skip(8).Take(8)),
                    Table.From(columns, rows.Skip(16).Take(8)),
                }
            };

            Table result = TableTasks.Concatenate(input, new CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(fullTable.Columns));
            Assert.That(result.Rows, Is.EqualTo(fullTable.Rows));
        }

        [Test]
        public void ConcatenateThrowsWhenTableColumnsDoNotMatch()
        {
            var rows    = TestData.Typed.Rows;
            var columns = TestData.Typed.Columns;

            Table first  = Table.From(columns, rows.Skip(0).Take(10));
            Table second = Table.From(columns, rows.Skip(10).Take(10));

            Table incompatible = TableBuilder
                                    .From(second)
                                    .RenameColumns(new Dictionary<string, string> { { "A", "X" } })
                                    .CreateTable();

            ConcatenateParameters input = new ConcatenateParameters
            {
                Tables = new [] { first, incompatible }
            };

            Action executeTask = () => TableTasks.Concatenate(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class ConvertColumnsTaskTests
    {
        [Test]
        public void ConvertColumnsReturnsANewTable()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "B", Type = ColumnType.Boolean },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void ConvertColumnsToBooleanWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "B", Type = ColumnType.Boolean },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.B is bool));
        }

        [Test]
        public void ConvertColumnsUsingCustomConverterWorks()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "I", Type = ColumnType.Custom, Converter = x => x > 0 },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.I is bool));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.I == (orig.I > 0)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToDateTimeWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "D", Type = ColumnType.DateTime, DateTimeFormat = "dd.MM.yyyy" },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.D is DateTime));
            Assert.That(
                original.Rows.Zip(
                    result.Rows,
                    (orig, res) => res.D == DateTime.ParseExact(orig.D, "dd.MM.yyyy", null)
                ),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToDecimalWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "F", Type = ColumnType.Decimal },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.F is decimal));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.F == decimal.Parse(orig.F)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToDoubleWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "F", Type = ColumnType.Double },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.F is double));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.F == double.Parse(orig.F)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToFloatWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "F", Type = ColumnType.Float },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.F is float));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.F == float.Parse(orig.F)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToIntWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "I", Type = ColumnType.Int },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.I is int));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.I == int.Parse(orig.I)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToLongWorks()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "I", Type = ColumnType.Long },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.I is long));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.I == long.Parse(orig.I)),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToStringWorksWithNoFormat()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "I", Type = ColumnType.String },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.I is string));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.I == orig.I.ToString()),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsToStringWorksWithAFormat()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion
                    {
                        Column       = "I",
                        Type         = ColumnType.String,
                        StringFormat = "x"
                    },
                }
            };

            Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.I is string));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.I == orig.I.ToString("x")),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsThrowsWhenSpecifyingAnInvalidColumnName()
        {
            Table original = TestData.Untyped;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "X", Type = ColumnType.Int }
                }
            };

            Action executeTask = () => TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void ConvertColumnsFailsOnErrorsByDefault()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "N", Type = ColumnType.Custom, Converter = s => s[0] },
                }
            };

            Action executeTask = () => TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.TypeOf<Table.Error>());

            /*Table result = TableTasks.ConvertColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.I is bool));
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => res.I == (orig.I > 0)),
                Has.All.EqualTo(true)
            );*/
        }

        [Test]
        public void ConvertColumnsCanDiscardErroneousRows()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "N", Type = ColumnType.Custom, Converter = s => s[0] },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Discard
            };            

            Table result = TableTasks.ConvertColumns(input, options, new CancellationToken());

            Assert.That(result.Rows, Has.All.Matches<dynamic>(row => row.N != null));
            Assert.That(
                original.Rows.Where(row => row.N != null).Zip(result.Rows, (orig, res) => res.N == (orig.N[0])),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsCanContinueOnErrors()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "N", Type = ColumnType.Custom, Converter = s => s[0] },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Continue
            };

            Table result = TableTasks.ConvertColumns(input, options, new CancellationToken());

            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => (orig.I == res.I)),
                Has.All.EqualTo(true)
            );
            Assert.That(
                original.Rows.Zip(result.Rows, (orig, res) => (orig.N == null && res.N == null) || res.N == (orig.N[0])),
                Has.All.EqualTo(true)
            );
        }

        [Test]
        public void ConvertColumnsCanCollectErrorsAndThenFail()
        {
            Table original = TestData.Typed;

            ConvertColumnsParameters input = new ConvertColumnsParameters
            {
                Data        = original,
                Conversions = new ColumnConversion[]
                {
                    new ColumnConversion { Column = "N", Type = ColumnType.Custom, Converter = s => s[0] },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.ContinueAndFail
            };

            Action executeTask = () => TableTasks.ConvertColumns(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == original.Rows.Where(row => row.N == null).Count())
            );
        }
    }

    [TestFixture]
    class GroupByTaskTests
    {
        [Test]
        public void GroupByReturnsANewTable()
        {
            Table original = TestData.Typed;

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = new string[] { "B" },
                ResultColumn = "G",
                Grouping     = GroupingType.EntireRows
            };

            Table result = TableTasks.GroupBy(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void GroupByIncludesAllKeyColumnsInTheResult()
        {
            Table original = TestData.Typed;
            string[] keyColumns = { "B", "M" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.EntireRows
            };

            Table result = TableTasks.GroupBy(input, new CancellationToken());


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
            Table original = TestData.Typed;
            string[] keyColumns = { "B" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.EntireRows
            };

            Table result = TableTasks.GroupBy(input, new CancellationToken());

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
            Table original = TestData.Typed;
            string[] keyColumns = { "B" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.SelectedColumns,
                Columns      = new[] { "A", "B", "C" }
            };

            Table result = TableTasks.GroupBy(input, new CancellationToken());

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
            Table original = TestData.Typed;
            string[] keyColumns = { "B" };

            GroupByParameters input = new GroupByParameters
            {
                Data         = original,
                KeyColumns   = keyColumns,
                ResultColumn = "G",
                Grouping     = GroupingType.SingleColumn,
                Column       = "A"
            };

            Table result = TableTasks.GroupBy(input, new CancellationToken());

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
            Table original = TestData.Typed;
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

            Table result = TableTasks.GroupBy(input, new CancellationToken());

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
            Table original = TestData.Typed;

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = new[] { "X" },  // <---
                ResultColumn    = "G",
                Grouping        = GroupingType.EntireRows
            };

            Action executeTask = () => TableTasks.GroupBy(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void GroupByThrowsWhenASelectedColumnDoesNotExistInTheTable()
        {
            Table original = TestData.Typed;

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = new[] { "B" },
                ResultColumn    = "G",
                Grouping        = GroupingType.SelectedColumns,
                Columns         = new[] { "A", "X" }  // <---
            };

            Action executeTask = () => TableTasks.GroupBy(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void GroupByThrowsWhenTheSelectedSingleColumnDoesNotExistInTheTable()
        {
            Table original = TestData.Typed;

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = new[] { "B" },
                ResultColumn    = "G",
                Grouping        = GroupingType.SingleColumn,
                Column          = "X" // <---
            };

            Action executeTask = () => TableTasks.GroupBy(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void GroupByThrowsWhenResultColumnIsOneOfTheKeyColums()
        {
            Table original = TestData.Typed;
            string[] keyColumns = { "B", "M" };

            GroupByParameters input = new GroupByParameters
            {
                Data            = original,
                KeyColumns      = keyColumns,
                ResultColumn    = "M",  // <---
                Grouping        = GroupingType.EntireRows
            };

            Action executeTask = () => TableTasks.GroupBy(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class FilterTaskTests
    {
        [Test]
        public void FilterUsingRowFilterReturnsANewTable()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data       = original,
                FilterType = ProcessingType.Row,
                Filter     = row => row.B == true
            };
            
            Table filtered = TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(filtered is Table);
            Assert.That(filtered, Is.Not.SameAs(original));
        }

        [Test]
        public void FilterUsingColumnFilterReturnsANewTable()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "B",
                Filter       = B => B == true
            };

            Table filtered = TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(filtered is Table);
            Assert.That(filtered, Is.Not.SameAs(original));
        }

        [Test]
        public void FilterUsingRowFilterProducesCorrectRows()
        {
            FilterParameters input = new FilterParameters
            {
                Data       = TestData.Typed,
                FilterType = ProcessingType.Row,
                Filter     = row => row.B == true
            };

            Table filtered = TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(filtered.Rows, Has.All.Matches<dynamic>(row => row.B == true));
        }

        [Test]
        public void FilterUsingColumnFilterProducesCorrectRows()
        {
            FilterParameters input = new FilterParameters
            {
                Data         = TestData.Typed,
                FilterType   = ProcessingType.Column,
                FilterColumn = "B",
                Filter       = B => B == true
            };

            Table filtered = TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(filtered.Rows, Has.All.Matches<dynamic>(row => row.B == true));
        }

        [Test]
        public void FilterUsingRowFilterDoesNotAffectRowOrder()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data       = original,
                FilterType = ProcessingType.Row,    // Filter based on entire rows
                Filter     = row => true            // Accept all rows
            };

            Table filtered = TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(filtered.Rows, Is.EqualTo(original.Rows));
        }

        [Test]
        public void FilterUsingColumnFilterDoesNotAffectRowOrder()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "B",
                Filter       = B => true
            };

            Table filtered = TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(filtered.Rows, Is.EqualTo(original.Rows));
        }

        [Test]
        public void FilterFailsWhenFilterColumnDoesNotExist()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "doesNotExist",
                Filter       = doesNotExist => true
            };

            Action executeTask = () => TableTasks.Filter(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void FilterCanIgnoreErrors()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "doesNotExist",
                Filter       = doesNotExist => true
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Continue
            };

            Table result = TableTasks.Filter(input, options, new CancellationToken());

            Assert.That(result.Errors.Count(), Is.EqualTo(original.Count));
        }

        [Test]
        public void FilterCanCollectErrorsAndThenFail()
        {
            Table original = TestData.Typed;

            FilterParameters input = new FilterParameters
            {
                Data         = original,
                FilterType   = ProcessingType.Column,
                FilterColumn = "doesNotExist",
                Filter       = doesNotExist => true
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.ContinueAndFail
            };

            Action executeTask = () => TableTasks.Filter(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == original.Count)
            );
        }
    }

    [TestFixture]
    public class JoinTaskTests
    {
        private class JoinRows
        {
            public List<string>       columns;
            public string[]           key;
            public List<object>[] matched;
            public List<object>[] duplicateMatches;
            public List<object>[] unmatched;
        };

        private static readonly JoinRows left = new JoinRows
        {
            columns = new List<string> { "A", "B", "V1", "V2", "V3" },
            key     = new [] { "A", "B" },
            matched = new []
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
            duplicateMatches = new []
            {
                new List<object> {    "India",   "Delta", true, 0,     "Brown" },
                new List<object> {     "Mike",   "Delta", true, 123,   "Olive" },
            },
            unmatched = new []
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
            matched = new []
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
            duplicateMatches = new []
            {
                new List<object> {    "Mike",   "Delta", 0.48, "#b93587" },
                new List<object> {  "Quebec",    "Lima",  0.3, "#4bf785" },
                new List<object> { "Foxtrot", "Juliett", 0.18, "#ae3c3c" }
            },
            unmatched = new []
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

            Table result = TableTasks.Join(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(leftTable));
            Assert.That(result, Is.Not.SameAs(rightTable));
        }

        [Test]
        public void JoinsWork()
        {
            Table leftMatched    = Table.From(left.columns, left.matched);
            Table leftUnmatched  = Table.From(left.columns, left.unmatched);
            Table leftDuplicates = Table.From(left.columns, left.duplicateMatches);

            Table leftTable = TableBuilder
                                .From(leftMatched)
                                .Concatenate(new []{ leftUnmatched, leftDuplicates })
                                .CreateTable();

            Table rightMatched   = Table.From(right.columns, right.matched);
            Table rightUnmatched = Table.From(right.columns, right.unmatched);
            Table rightDuplicates = Table.From(right.columns, right.duplicateMatches);

            Table rightTable = TableBuilder
                                .From(rightMatched)
                                .Concatenate(new []{ rightUnmatched, rightDuplicates })
                                .CreateTable();

            var testCases = new []
            {
                new
                {
                    JoinType          = JoinType.Inner,
                    ExpectedLeftRows  = leftMatched.Rows.Concat(leftDuplicates.Rows),
                    ExpectedRightRows = rightMatched.Rows.Concat(rightDuplicates.Rows)
                },
                new
                {
                    JoinType          = JoinType.LeftOuter,
                    ExpectedLeftRows  = leftTable.Rows,
                    ExpectedRightRows = rightMatched.Rows.Concat(rightDuplicates.Rows)
                },
                new
                {
                    JoinType          = JoinType.FullOuter,
                    ExpectedLeftRows  = leftTable.Rows,
                    ExpectedRightRows = rightTable.Rows
                }
            };

            foreach(var testCase in testCases)
            {
                JoinParameters input = new JoinParameters
                {
                    JoinType = testCase.JoinType,
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

                Table result = TableTasks.Join(input, new CancellationToken());

                foreach(var row in result.Rows)
                {
                    Assert.That(row.left != null || row.right != null);

                    if(row.left != null && row.right != null)
                        Assert.That(row.left.A == row.right.X && row.left.B == row.right.Y);
                }

                var leftJoinedRows = result.Rows
                                        .Where(row => row.left != null)
                                        .Select(row => row.left)
                                        .Distinct();

                var rightJoinedRows = result.Rows
                                        .Where(row => row.right != null)
                                        .Select(row => row.right)
                                        .Distinct();

                Assert.That(leftJoinedRows, Is.EquivalentTo(testCase.ExpectedLeftRows));
                Assert.That(rightJoinedRows, Is.EquivalentTo(testCase.ExpectedRightRows));
            }
        }

        [Test]
        public void JoinWorksWhenAllLeftRowsHaveASingleMatch()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinType[] joins = { JoinType.Inner, JoinType.LeftOuter, JoinType.FullOuter };

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

                Table result = TableTasks.Join(input, new CancellationToken());

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
            var rightRows = right.matched.Concat(right.duplicateMatches);

            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, rightRows);

            JoinType[] joins = { JoinType.Inner, JoinType.LeftOuter, JoinType.FullOuter };

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

                Table result = TableTasks.Join(input, new CancellationToken());

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
        public void FullOuterJoinProducesAllRowsFromBothTables()
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
                JoinType = JoinType.FullOuter,
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

            Table result = TableTasks.Join(input, new CancellationToken());

            var leftJoinedRows = result.Rows
                                    .Where(row => row.left != null)
                                    .Select(row => row.left);

            var rightJoinedRows = result.Rows
                                    .Where(row => row.right != null)
                                    .Select(row => row.right);

            Assert.That(leftJoinedRows, Is.EquivalentTo(leftTable.Rows));
            Assert.That(rightJoinedRows, Is.EquivalentTo(rightTable.Rows));
        }

        [Test]
        public void FullOuterJoinProducesAllRowsFromBothTablesWhenThereAreDuplicateMatches()
        {
            Table leftMatched    = Table.From(left.columns, left.matched);
            Table leftUnmatched  = Table.From(left.columns, left.unmatched);
            Table leftDuplicates = Table.From(left.columns, left.duplicateMatches);

            Table leftTable = TableBuilder
                                .From(leftMatched)
                                .Concatenate(new []{ leftUnmatched, leftDuplicates })
                                .CreateTable();

            Table rightMatched   = Table.From(right.columns, right.matched);
            Table rightUnmatched = Table.From(right.columns, right.unmatched);
            Table rightDuplicates = Table.From(right.columns, right.duplicateMatches);

            Table rightTable = TableBuilder
                                .From(rightMatched)
                                .Concatenate(new []{ rightUnmatched, rightDuplicates })
                                .CreateTable();

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.FullOuter,
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

            Table result = TableTasks.Join(input, new CancellationToken());

            var leftJoinedRows = result.Rows
                                    .Where(row => row.left != null)
                                    .Select(row => row.left)
                                    .Distinct();

            var rightJoinedRows = result.Rows
                                    .Where(row => row.right != null)
                                    .Select(row => row.right)
                                    .Distinct();

            Assert.That(leftJoinedRows, Is.EquivalentTo(leftTable.Rows));
            Assert.That(rightJoinedRows, Is.EquivalentTo(rightTable.Rows));
        }

        [Test]
        public void InnerJoinDoesNotProduceLeftRowsThatHaveNoMatch()
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

            Table result = TableTasks.Join(input, new CancellationToken());

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

            Table result = TableTasks.Join(input, new CancellationToken());

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

            Table result = TableTasks.Join(input, new CancellationToken());

            var resultMatchedRows = result.Rows.Where(row => row.right != null);

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

                Table result = TableTasks.Join(input, new CancellationToken());

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
        public void ConditionalParametersAreIgnoredWhenTheyAreNotUsed()
        {
            Table leftTable = Table.From(left.columns, left.matched);
            Table rightTable = Table.From(right.columns, right.matched);

            JoinParameters input = new JoinParameters
            {
                JoinType = JoinType.LeftOuter,
                Left = new JoinTable
                {
                    Data          = leftTable,
                    KeyColumns    = left.key,
                    ResultType    = JoinResult.AllColumns,
                    ResultColumn  = ""
                },
                Right = new JoinTable
                {
                    Data          = rightTable,
                    KeyColumns    = right.key,
                    ResultType    = JoinResult.SelectColumns,
                    ResultColumns = new [] { "V4" },
                    ResultColumn  = ""
                }
            };

            Action executeTask = () => TableTasks.Join(input, new CancellationToken());

            Assert.That(executeTask, Throws.Nothing);
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

                Table result = TableTasks.Join(input, new CancellationToken());

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
        public void RightRowColumnsCanBeExpandedWithLeftOuterJoin()
        {
            Table leftTable = Table.From(left.columns, left.unmatched);

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
                    JoinType = JoinType.LeftOuter,
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

                Table result = TableTasks.Join(input, new CancellationToken());

                Assert.That(result.Columns, Is.EqualTo(testCase.ExpectedColumns));
                // Check that each row has the columns in the new column order
                foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
                {
                    var keys = row.Select(x => x.Key);
                    var nonNullKeys = row.Where(x => x.Value != null).Select(x => x.Key);

                    Assert.That(keys, Is.EqualTo(testCase.ExpectedColumns));
                    Assert.That(nonNullKeys, Is.EqualTo(new[] { "left" }));
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

            Table result = TableTasks.Join(input, new CancellationToken());

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

            Action executeTask = () => TableTasks.Join(input, new CancellationToken());

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

            Action executeTask = () => TableTasks.Join(input, new CancellationToken());

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

            Action executeTask = () => TableTasks.Join(input, new CancellationToken());

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

            Action executeTask = () => TableTasks.Join(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class RemoveDuplicatesTaskTests
    {
        [Test]
        public void RemoveDuplicatesReturnsANewTable()
        {
            Table original = TestData.Typed;

            RemoveDuplicatesParameters input = new RemoveDuplicatesParameters
            {
                Data = original,
                Key  = RemoveDuplicatesKey.EntireRows
            };

            Table result = TableTasks.RemoveDuplicates(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void RemoveDuplicatesWorksWithSingleColumnKey()
        {
            Table original = TestData.Typed;

            RemoveDuplicatesParameters input = new RemoveDuplicatesParameters
            {
                Data       = original,
                Key        = RemoveDuplicatesKey.SelectedColumns,
                KeyColumns = new [] { "E" }
            };

            Table result = TableTasks.RemoveDuplicates(input, new CancellationToken());

            Assert.That(result.Count > 0);
            Assert.That(result.Rows.Select(row => row.E), Is.Unique);
            Assert.That(result.Rows.Select(row => row.E), Is.EqualTo(original.Rows.Select(row => row.E).Distinct()));
            Assert.That(result.Rows, Is.SubsetOf(original.Rows));
        }

        [Test]
        public void RemoveDuplicatesWorksWithMultiColumnKey()
        {
            Table original = TestData.Typed;

            RemoveDuplicatesParameters input = new RemoveDuplicatesParameters
            {
                Data       = original,
                Key        = RemoveDuplicatesKey.SelectedColumns,
                KeyColumns = new [] { "B", "E" }
            };

            Table result = TableTasks.RemoveDuplicates(input, new CancellationToken());

            Assert.That(result.Count > 0);
            Assert.That(result.Rows.Select(row => (row.B, row.E)), Is.Unique);
            Assert.That(result.Rows.Select(row => (row.B, row.E)), Is.EqualTo(original.Rows.Select(row => (row.B, row.E)).Distinct()));
            Assert.That(result.Rows, Is.SubsetOf(original.Rows));
        }
    }


    [TestFixture]
    class RenameColumnsTaskTests
    {
        private static readonly ColumnRename[] renamings =
        {
            new ColumnRename { Column = "F", NewName = "W" },
            new ColumnRename { Column = "D", NewName = "Z" },
            new ColumnRename { Column = "C", NewName = "Y" },
            new ColumnRename { Column = "A", NewName = "X" },
        };

        private static readonly ColumnRename[] partlyInvalidRenamings =
        {
            new ColumnRename { Column = "F", NewName = "W" },
            new ColumnRename { Column = "D", NewName = "Z" },
            new ColumnRename { Column = "C", NewName = "Y" },
            new ColumnRename { Column = "A", NewName = "X" },
            new ColumnRename { Column = "INVALID", NewName = "?" }
        };

        [Test]
        public void RenameColumnsReturnsANewTable()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                = original,
                Renamings           = renamings,
                PreserveColumnOrder = true,
                DiscardOtherColumns = false
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void ColumnsAreRenamed()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Renamings                = renamings,
                PreserveColumnOrder      = true,
                DiscardOtherColumns      = false,
                IgnoreInvalidColumnNames = false
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            string[] expectedColumns = { "X", "B", "Y", "Z", "E", "W", "I", "M", "N", "U" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void ColumnsCanBeOrderedAccoringToTheColumnMapping()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Renamings                = renamings,
                PreserveColumnOrder      = false,
                DiscardOtherColumns      = false,
                IgnoreInvalidColumnNames = false
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            string[] expectedColumns = { "W", "B", "Z", "Y", "E", "X", "I", "M", "N", "U" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void OtherColumnsCanBeDiscardedWhilePreservingColumnOrder()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Renamings                = renamings,
                PreserveColumnOrder      = true,
                DiscardOtherColumns      = true, // <---
                IgnoreInvalidColumnNames = false
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            string[] expectedColumns = { "X", "Y", "Z", "W" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void OtherColumnsCanBeDiscardedWhileNotPreservingColumnOrder()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Renamings                = renamings,
                PreserveColumnOrder      = false, // <---
                DiscardOtherColumns      = true,  // <---
                IgnoreInvalidColumnNames = false
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(renamings.Select(m => m.NewName)));
        }

        [Test]
        public void RenamingsCanBeProvidedAsJson()
        {
            Table original = TestData.Typed;

            JObject jsonRenamings = new JObject
            {
                { "F", "W" },
                { "D", "Z" },
                { "C", "Y" },
                { "A", "X" },
            };

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Format                   = RenameFormat.JSON,
                JsonRenamings            = jsonRenamings.ToString(),
                PreserveColumnOrder      = true,
                DiscardOtherColumns      = false,
                IgnoreInvalidColumnNames = false
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            string[] expectedColumns = { "X", "B", "Y", "Z", "E", "W", "I", "M", "N", "U" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }

        [Test]
        public void ByDefaultRenameColumnsThrowsWhenEncounteringAnInvalidColumnName()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Renamings                = partlyInvalidRenamings,
                PreserveColumnOrder      = true,
                DiscardOtherColumns      = false,
                IgnoreInvalidColumnNames = false
            };

            Action executeTask = () => TableTasks.RenameColumns(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void RenameColumnsCanIgnoreInvalidColumnNames()
        {
            Table original = TestData.Typed;

            RenameColumnsParameters input = new RenameColumnsParameters
            {
                Data                     = original,
                Renamings                = partlyInvalidRenamings,
                PreserveColumnOrder      = true,
                DiscardOtherColumns      = false,
                IgnoreInvalidColumnNames = true   // <--
            };

            Table result = TableTasks.RenameColumns(input, new CancellationToken());

            string[] expectedColumns = { "X", "B", "Y", "Z", "E", "W", "I", "M", "N", "U" };

            Assert.That(result.Columns, Is.EqualTo(expectedColumns));
        }
    }

    [TestFixture]
    class ReorderColumnsTaskTests
    {
        [Test]
        public void ReorderColumnsReturnsANewTable()
        {
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = original.Columns.Reverse<string>().ToArray(),
                DiscardOtherColumns = false
            };

            Table reordered = TableTasks.ReorderColumns(input, new CancellationToken());

            Assert.That(reordered is Table);
            Assert.That(reordered, Is.Not.SameAs(original));
        }

        [Test]
        public void ResultHasColumnsInTheSpecifiedOrder()
        {
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = original.Columns.Reverse<string>().ToArray(),
                DiscardOtherColumns = false
            };

            Table reordered = TableTasks.ReorderColumns(input, new CancellationToken());

            Assert.That(reordered.Columns, Is.EqualTo(original.Columns.Reverse<string>()));
        }

        [Test]
        public void ResultRowsAreInColumnOrder()
        {
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = original.Columns.Reverse<string>().ToArray(),
                DiscardOtherColumns = false
            };

            Table reordered = TableTasks.ReorderColumns(input, new CancellationToken());

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in reordered.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(original.Columns.Reverse<string>()));
            }
        }

        [Test]
        public void OrderOfUnspecifiedColumnsDoesNotChange()
        {
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "C", "E", "B" },
                DiscardOtherColumns = false
            };

            string[] expectedColumnOrder = { "A", "C", "E", "D", "B", "F", "I", "M", "N", "U" };

            Table reordered = TableTasks.ReorderColumns(input, new CancellationToken());

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
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "C", "E", "B" },
                DiscardOtherColumns = true
            };

            string[] expectedColumns = { "C", "E", "B" };

            Table reordered = TableTasks.ReorderColumns(input, new CancellationToken());

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
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "B", "A", "B" },
                DiscardOtherColumns = false
            };

            Action executeTask = () => TableTasks.ReorderColumns(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        public void ReorderColumnsThrowsWhenColumnOrderContainsAnInvalidColumn()
        {
            Table original = TestData.Typed;

            ReorderColumnsParameters input = new ReorderColumnsParameters
            {
                Data                = original,
                ColumnOrder         = new string[] { "A", "B", "X" },
                DiscardOtherColumns = false
            };

            Action executeTask = () => TableTasks.ReorderColumns(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class SelectColumnsTaskTests
    {
        [Test]
        public void SelectColumnsReturnsANewTable()
        {
            Table original = TestData.Typed;

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data                = original,
                Action              = SelectColumnsAction.Keep,
                Columns             = new string[] { "A", "B" },
                PreserveColumnOrder = false
            };

            Table result = TableTasks.SelectColumns(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void SpecificColumnsCanBeSelectedInTheSpecifiedOrder()
        {
            Table original = TestData.Typed;

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data                = original,
                Action              = SelectColumnsAction.Keep,
                Columns             = selectedColumns,
                PreserveColumnOrder = false
            };

            Table result = TableTasks.SelectColumns(input, new CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(selectedColumns));
        }

        [Test]
        public void SpecificColumnsCanBeSelectedInTheirOriginalOrder()
        {
            Table original = TestData.Typed;

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data                = original,
                Action              = SelectColumnsAction.Keep,
                Columns             = selectedColumns,
                PreserveColumnOrder = true
            };

            Table result = TableTasks.SelectColumns(input, new CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(original.Columns.Where(c => selectedColumns.Contains(c))));
        }

        [Test]
        public void SpecificColumnsCanBeDiscarded()
        {
            Table original = TestData.Typed;

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data    = original,
                Action  = SelectColumnsAction.Discard,
                Columns = selectedColumns
            };

            Table result = TableTasks.SelectColumns(input, new CancellationToken());

            Assert.That(result.Columns, Is.EqualTo(original.Columns.Where(c => !selectedColumns.Contains(c))));
        }

        [Test]
        public void ResultRowsAreInColumnOrder()
        {
            Table original = TestData.Typed;

            string[] selectedColumns =  new string[] { "B", "A" };

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data                = original,
                Action              = SelectColumnsAction.Keep,
                Columns             = selectedColumns,
                PreserveColumnOrder = false
            };

            Table result = TableTasks.SelectColumns(input, new CancellationToken());

            // Check that each row has the columns in the new column order
            foreach(IEnumerable<KeyValuePair<string, dynamic>> row in result.Rows)
            {
                var keys = row.Select(x => x.Key);

                Assert.That(keys, Is.EqualTo(selectedColumns));
            }
        }

        public void SelectColumnsThrowsWhenColumnOrderHasDuplicates()
        {
            Table original = TestData.Typed;

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data                = original,
                Action              = SelectColumnsAction.Keep,
                Columns             = new string[] { "B", "A", "B" },
                PreserveColumnOrder = false
            };

            Action executeTask = () => TableTasks.SelectColumns(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        public void SelectColumnsThrowsWhenColumnOrderContainsAnInvalidColumn()
        {
            Table original = TestData.Typed;

            SelectColumnsParameters input = new SelectColumnsParameters
            {
                Data                = original,
                Action              = SelectColumnsAction.Keep,
                Columns             = new string[] { "A", "B", "X" },
                PreserveColumnOrder = false
            };

            Action executeTask = () => TableTasks.SelectColumns(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class SortTaskTests
    {
        [Test]
        public void SortReturnsANewTable()
        {
            Table original = TestData.Typed;

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Ascending }
                }
            };

            Table result = TableTasks.Sort(input, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void SortingASingleColumnAscendingWorks()
        {
            Table original = TestData.Typed;

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Ascending }
                }
            };

            Table result = TableTasks.Sort(input, new CancellationToken());

            Assert.That(result.Rows.Select(row => row.E), Is.Ordered.Ascending);
        }

        [Test]
        public void SortingASingleColumnDescendingWorks()
        {
            Table original = TestData.Typed;

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Descending }
                }
            };

            Table result = TableTasks.Sort(input, new CancellationToken());

            Assert.That(result.Rows.Select(row => row.E), Is.Ordered.Descending);
        }

        [Test]
        public void SortingMultipleColumnsWorks()
        {
            Table original = TestData.Typed;

            SortParameters input = new SortParameters
            {
                Data            = original,
                SortingCriteria = new SortingCriterion[]
                {
                    new SortingCriterion { Column = "E", Order = Order.Descending },
                    new SortingCriterion { Column = "A", Order = Order.Ascending  }
                }
            };

            Table result = TableTasks.Sort(input, new CancellationToken());

            Assert.That(
                result.Rows.Select(row => new { row.E, row.A }),
                Is.Ordered.Descending.By("E").Then.Ascending.By("A")
            );
        }

        [Test]
        public void SortThrowsWhenAnInvalidColumnIsSpecified()
        {
            Table original = TestData.Typed;

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

            Action executeTask = () => TableTasks.Sort(input, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }
    }

    [TestFixture]
    class TransformColumnsTaskTests
    {
        [Test]
        public void TransformColumnsReturnsANewTable()
        {
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "A", TransformType = ProcessingType.Row, Transform = row => row.A * 10 },
                }
            };

            Table result = TableTasks.TransformColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(result is Table);
            Assert.That(result, Is.Not.SameAs(original));
        }

        [Test]
        public void TransformCanBeDoneUsingRows()
        {
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "A", TransformType = ProcessingType.Row, Transform = row => row.A * 10 },
                }
            };

            Table result = TableTasks.TransformColumns(input, CommonOptions.Defaults, new CancellationToken());

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
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "A", TransformType = ProcessingType.Column, Transform = A => A * 10 },
                }
            };

            Table result = TableTasks.TransformColumns(input, CommonOptions.Defaults, new CancellationToken());

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
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "X", TransformType = ProcessingType.Row, Transform = row => null },
                }
            };

            Action executeTask = () => TableTasks.TransformColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void TransformColumnsFailsOnErrorsByDefault()
        {
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "N", TransformType = ProcessingType.Row, Transform = row => row.N[0] },
                }
            };

            Action executeTask = () => TableTasks.TransformColumns(input, CommonOptions.Defaults, new CancellationToken());

            Assert.That(executeTask, Throws.Exception);
        }

        [Test]
        public void TransformColumnsCanDiscardErroneousRows()
        {
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "N", TransformType = ProcessingType.Row, Transform = row => row.N.Substring(0) },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Discard
            };

            Table result = TableTasks.TransformColumns(input, options, new CancellationToken());

            Assert.That(result.Rows, Is.EquivalentTo(original.Rows.Where(row => row.N != null)));
            Assert.That(result.Errors.Count() == original.Rows.Where(row => row.N == null).Count());
        }

        [Test]
        public void TransformColumnsCanContinueAfterErrors()
        {
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "N", TransformType = ProcessingType.Row, Transform = row => row.N.Substring(0) },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.Continue
            };

            Table result = TableTasks.TransformColumns(input, options, new CancellationToken());

            Assert.That(result.Rows, Is.EquivalentTo(original.Rows));
            Assert.That(result.Errors.Count() == original.Rows.Where(row => row.N == null).Count());
        }

        [Test]
        public void TransformColumnsCanFailOnErrorsAfterProcessingAllRows()
        {
            Table original = TestData.Typed;

            TransformColumnsParameters input = new TransformColumnsParameters
            {
                Data       = original,
                Transforms = new ColumnTransform[]
                {
                    new ColumnTransform { Column = "N", TransformType = ProcessingType.Row, Transform = row => row.N.Substring(0) },
                }
            };

            CommonOptions options = new CommonOptions
            {
                ErrorHandling = Table.ErrorHandling.ContinueAndFail
            };

            Action executeTask = () => TableTasks.TransformColumns(input, options, new CancellationToken());

            Assert.That(
                executeTask,
                Throws
                    .TypeOf<Table.FailedOperationException>()
                    .With.Property("Errors")
                    .Matches<dynamic>(errors => errors.Count == original.Rows.Where(row => row.N == null).Count())
            );
        }

        
    }
}
