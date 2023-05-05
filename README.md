# Pori.Frends.Data
Frends tasks for data manipulation inside a process.

- [Installing](#installing)
- [Tables](#tables)
- [Tasks](#tasks)
     - [Load](#Load)
     - [AddColumns](#addcolumns)
     - [Chunk](#chunk)
     - [Concatenate](#concatenate)
     - [ConvertColumns](#convertcolumns)
     - [Filter](#filter)
     - [GroupBy](#groupby)
     - [Join](#join)
     - [RemoveDuplicates](#removeduplicates)
     - [RenameColumns](#renamecolumns)
     - [ReorderColumns](#reordercolumns)
     - [SelectColumns](#selectcolumns)
     - [Serialize](#serialize)
     - [Sort](#sort)
     - [TransformColumns](#transformcolumns)
- [Building](#building)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View by using `Import Task NuGet`
button in Administration > Tasks.

# General usage

Data manipulation using the `Pori.Frends.Data` package is in general done in four steps.

1. **Read and parse** input data (CSV, JSON, etc.) using other Frends tasks
   (`Frends.File.Read`, `Frends.Csv.Create`, etc.).
2. **Load** the data as a [`Table`](#tables) using the [`Load`](#Load) task.
3. **Process** and modify data using the other tasks in this package.
4. **Convert** the resulting table data into a specific format (CSV, JSON, etc.)
   using the [methods](#methods) of the `Pori.Frends.Data.Table` class together
   with other Frends tasks (for example `Frends.Csv.Create`).

Table data that will be used by other Frends processes can also be serialized
for easy loading later using the [`Serialize`](#serialize) task. This removes
the need for the first step and simplifies processes.

# Tables

All tasks either produce or operate on instances of the `Pori.Frends.Data.Table`
class. In task descriptions, the class name `Table` is used.

Each table has a number of named columns. The column names can be accessed using
the `Columns` property. The rows (actual data) of a table can be accessed using
the `Rows` property which is an `IEnumerable<dynamic>` where each item is
an instance of `ExpandoObject` representing the table row. Normally, these
properties do not have to be accessed directly in processes as all tasks expect
table instances.

Table columns do not have explicit data types associated with them and different
rows of the table can contain different types of values in a single column. If
needed, data type validation has to be done manually (for example, using the
`ConvertColumns` task).

## Properties

| Property | Type                   | Description                                                                                                                                             |
| -------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Columns  | `List<string>`         | Names of the columns in the table.                                                                                                                      |
| Rows     | `IEnumerable<dynamic>` | The rows of the table, each of which is a .NET [`ExpandoObject`](https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.expandoobject). Column values can be accessed as properties using the column names (i.e. `row.columnName`). |
| Errors   | `IEnumerable<Error>`   | A list of errors encountered while creating this table.                                                                                                 |

## Methods

Table instances have methods for converting table data into different formats
accepted by other Frends tasks.

| Method        | Return type          | Description                                                                                                                                                                                                                                                               |
| ------------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ToCsvRows()` | `List<List<object>>` | Returns the rows of the table in a format  suitable for use with the `Frends.Csv.Create` task (in combination with the table's `Columns` property).                                                                                                                       |
| `ToJson()`    | `JToken`             | Returns the data of the table as a `JArray` containing the rows of the table as instances of `JObject`.                                                                                                                                                                       |
| `ToXml()`     | `string`             | Returns the data of the table as an XML string. By default the result includes an XML declaration. This can be changed using the `declaration` parameter. The contents of the XML declaration can be changed with the parameters `version`, `encoding`, and `standalone`. |


# Tasks

| Task                                  | Description                                                                                                       |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| [Load](#Load)                         | Load data as a [`Table`](#tables)                                                                                 |
| [AddColumns](#addcolumns)             | Add one or more columns to a table.                                                                               |
| [Chunk](#chunk)                       | Split a table into tables of with a given number of rows.                                                         |
| [Concatenate](#concatenate)           | Concatenate two or more tables into a single table.                                                               |
| [ConvertColumns](#convertcolumns)     | Convert the values in one or more columns of a table.                                                             |
| [Filter](#filter)                     | Filter the rows of a table using a function.                                                                      |
| [GroupBy](#groupby)                   | Group the rows of a table based on the values of one or more columns.                                             |
| [Join](#join)                         | Join data from two table based on the values of one or more columns. Supports inner, (left) outer, and full outer joins. |
| [RemoveDuplicates](#removeduplicates) | Remove duplicate rows from a table based on the values of one or more columns.                                    |
| [RenameColumns](#renamecolumns)       | Rename one or more columns of a table.                                                                            |
| [ReorderColumns](#reordercolumns)     | Change the order of the columns of a table.                                                                       |
| [SelectColumns](#selectcolumns)       | Select or discard one or more columns of a table.                                                                 |
| [Serialize](#serialize)               | Serialize table data for easy loading in another process.                                                |
| [Sort](#sort)                         | Sort the rows of a table based on the values of one or more columns, or using a custom sorting function.          |
| [TransformColumns](#transformcolumns) | Transform the values of one or more table columns using a function.                                               |

> **Note!** Unless otherwise noted, all tasks return a new table object with new
> row objects.

> **Note!** Due to the way the Frends code generation currently works with
> parameters of type `IEnumerable<T>`, those parameters must be given array
> values.

- - -

## Load

Load data into a new table. Supported formats include CSV, JSON, XML. Returns an instance of [`Table`](#tables).

### Input

#### Format independent parameters

| Parameter | Type                        | Description                                 |
| --------- | --------------------------- | ------------------------------------------- |
| Format    | [`LoadFormat`](#loadformat) | The format of the data to load into a table |

##### LoadFormat

| Value        | Description                                                                             |
| ------------ | --------------------------------------------------------------------------------------- |
| `CSV`        | Load CSV data.                                                                          |
| `JSON`       | Load JSON data.                                                                         |
| `XML`        | Load XML data.                                                                          |
| `Rows`       | Load rows from another table or tables into a new table.                                |
| `Serialized` | Load table data that was previously serialized using the `Serialize` task.              |
| `Custom`     | Load table data from list of rows using a custom function for extracting column values. |

#### CSV

CSV data must first loaded using the `Frends.Csv.Create` task. This data can then be loaded into a table.

| Parameter | Type            | Description                                                        |
| --------- | --------------- | ------------------------------------------------------------------ |
| Data      | see description | The CSV data as an object produced by the `Frends.Csv.Create` task |

##### JSON

JSON data must (currently) be first converted to `JToken` and then loaded into a
table. Column names must be specified to enable loading empty tables.

| Parameter | Type                  | Description                                                                                                                               |
| --------- | --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Data      | `JToken`              | The JSON data as a JArray of JObjects.                                                                                                    |
| Columns   | `IEnumerable<string>` | The properties from the row objects to load as columns of the table. Missing values in source objects produce `null` as the column value. |

#### XML

XML data can be loaded flexibly, regardless of the specific structure of the data. There are some requirements though:

 - Each row and its data must be contained in a separate XML element
 - Each column and its name and data must be contained in a separete XML element


| Parameter     | Type                                      | Description                                                      |
| ------------- | ----------------------------------------- | ---------------------------------------------------------------- |
| Data          | `string`                                  | The XML data as a string.                                        |
| RowsPath      | `string`                                  | An XPath expression that for selecting table rows from the data. |
| Columns       | `IEnumerable<string>`                     | Names of the columns to include in the resulting table.          |
| ColumnSources | [`XmlColumnSource`](#xmlcolumnsource)`[]` | Definitions for loading the table columns from the data.         |

##### XmlColumnSource

Defines how to load table columns from XML data.

| Parameter      | Type                                | Description                                                                                                                                          |
| -------------- | ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| Type           | `SingleColumn` or `MultipleColumns` | Whether to define loading for a single named column or multiple columns.                                                                             |
| ValueType      | `SingleValue` or `MultipleValues`   | Whether the column contains a single value or multiple values for each row.                                                                          |
| ColumnPath     | `string`                            | An XPath expression for extracting the column data from the row data. Must be relative to the row element.                                           |
| ColumnName     | `string`                            | If `Type` is `SingleColumn`, the name of the column to load.                                                                                         |
| ColumnNamePath | `string`                            | If `Type` is `MultipleColumns`, an XPath expression for extracting the name of the column. Must be relative to the element found using `ColumnPath`. |
| ValuePath      | `string`                            | An XPath expression for extracting the column value(s) from the row element. Must be relative to the element found using `ColumnPath`.               |

#### Rows

| Parameter | Type                   | Description                                                                                                         |
| --------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------- |
| Data      | `IEnumerable<dynamic>` | Table rows to load into a new table. Loaded rows are copied and not loaded as is.                                   |
| Columns   | `IEnumerable<string>`  | Names of the columns to include in the resulting table. Columns missing from the source rows get a value of `null`. |

#### Serialized

| Parameter | Type               | Description                                                                       |
| --------- | ------------------ | --------------------------------------------------------------------------------- |
| Source    | `File` or `String` | Whether to load the serialized table from a file or from a string.                |
| Path      | `string`           | If `Source` is `File`, the path to the file containing the serialized table data. |
| Data      | `string`           | If `Source` is `String`, the serialized table data as a string.                   |

#### Custom

| Parameter    | Type                             | Description                                                                                                                                                                                         |
| ------------ | -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data         | `IEnumerable<dynamic>`           | Custom data to load as rows of a new table.                                                                                                                                                         |
| Columns      | `IEnumerable<string>`            | Names of the columns to load.                                                                                                                                                                       |
| ColumnLoader | `Func<dynamic, string, dynamic>` | A function for loading column values for each table row. Receives as input a single object from `Data` as well as the name of the column to load. Must return the appropriate value for the column. |

- - -

## AddColumns

Add one or more columns to a table.

### Input

| Parameter | Type                          | Description                                          |
| --------- | ----------------------------- | ---------------------------------------------------- |
| Data      | [`Table`](#tables)            | The source data as a table.                          |
| Columns   | [`NewColumn`](#newcolumn)`[]` | Definitions for the new columns to add to the table. |

#### NewColumn

| Property       | Type                                          | Description                                                                                                                                                                                  |
| -------------- | --------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Name           | `string`                                      | Name of the column to add to the table.                                                                                                                                                      |
| ValueSource    | `Constant`, `Computed` or `ComputedWithIndex` | Whether to use a constant value for all rows or compute a value for each row using a custom function (optionally using the row index in addition to the row data).                           |
| Value          | `dynamic`                                     | If `ValueSource` is `Constant`, the constant value to use for the new column for all rows.                                                                                                   |
| ValueGenerator | `Func<dynamic, dynamic>`                      | If `ValueSource` is `Computed`, a function to compute the value for the new column for each row. Receives the row object as input and should return a value for the new column for that row. |

- - -

## Chunk

Splits a single table into fixed sized separate tables. Returns the resulting tables as a
`List<`[`Table`](#tables)`>`.

### Input

| Parameter | Type               | Description                                            |
| --------- | ------------------ | ------------------------------------------------------ |
| Data      | [`Table`](#tables) | The source data as a table.                            |
| Size      | `int`              | The size (number of rows) of each table in the result. |


## Concatenate

Concatenate the rows of multiple tables into a single table. All tables must
have exactly the same columns. Rows from the source tables are used as is and
are not copied.

### Input

| Parameter | Type                                | Description                |
| --------- | ----------------------------------- | -------------------------- |
| Tables    | `IEnumerable<`[`Table`](#tables)`>` | The tables to concatenate. |

- - -

## ConvertColumns

Convert values of a one or more columns of the source table to a specific data type.

### Input

| Parameter   | Type                                        | Description                                  |
| ----------- | ------------------------------------------- | -------------------------------------------- |
| Data        | [`Table`](#tables)                          | The source data as a table.                  |
| Conversions | [`ColumnConversion`](#columnconversion)`[]` | The conversion to perform on the table data. |

#### ColumnConversion

| Property       | Type                            | Description                                                                                                                                                   |
| -------------- | ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Column         | `string`                        | The name of the column whose values are converted.                                                                                                            |
| Type           | [`ColumnType`](#columntype)`[]` | The datatype to convert the column values to.                                                                                                                 |
| DateTimeFormat | `string`                        | If `Type` is `DateTime`, the date format the data is in the source table.                                                                                     |
| StringFormat   | `dynamic`                       | If `Type` is `String`, the value to pass to the `ToString()` method of the current value.                                                                     |
| Converter      | `Func<dynamic, dynamic>`        | If `Type` is `Custom`, a function to perform the custom conversion. Receives the current value of the column as input and should produce the converted value. |

#### ColumnType

| Value      | Description                                                                                                 |
| ---------- | ----------------------------------------------------------------------------------------------------------- |
| `Boolean`  | Convert to a boolean.                                                                                       |
| `DateTime` | Convert to a DateTime object using a given date format.                                                     |
| `Decimal`  | Convert to a Decimal value.                                                                                 |
| `Double`   | Convert to a double.                                                                                        |
| `Float`    | Convert to a float.                                                                                         |
| `Int`      | Convert to a int.                                                                                           |
| `Long`     | Convert to a long.                                                                                          |
| `String`   | Convert to a string, optionally using a value to be passed to the `ToString()` method of the current value. |
| `Custom`   | Convert the current value using a custom conversion function. See also the `TransformColumns` task.         |

- - -

## Filter

Filter rows of a table into a new table. Rows from the source table are used as is and are not copied.

### Input

| Parameter    | Type                                                 | Description                                                                                                                                                                                                                                                          |
| ------------ | ---------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data         | [`Table`](#tables)                                   | The source data as a table.                                                                                                                                                                                                                                          |
| FilterType   | `Row`, `Column`, `RowWithIndex` or `ColumnWithIndex` | Whether to filter row based on the value of a single column or the entire row and whether to use the row index when filtering.                                                                                                                                       |
| FilterColumn | `string`                                             | If `FilterType` is `Column` or `ColumnWithIndex`, the name of the column to use for filtering the rows.                                                                                                                                                              |
| Filter       | `Func<dynamic, bool>`                                | The function to use for filtering table rows. Rows for which the function returns `true` are include in the resulting table. Depending on the value of `FilterType`, the input of the function is either the entire row object or the value of the specified column. |

### Examples

#### Filter rows using the entire row

When filtering rows by multiple columns, choose `FilterType` as `Row`. The
filter function then receives the entire row object as its argument.


```c#
device => device.type != "laptop" && device.isProvisioned == true
```


#### Filter rows by the value of specific column

When filtering using only a single column, the filter function can be simplified
by specifying `FilterType` to `Column` and `FilterColumn` to the name of the
column to filter by.

```c#
deviceType => deviceType != "laptop"
```

Here the name used for the column in the filter function (`deviceType`) does not
have to match the actual name of the column specified in `FilterColumn`.

- - -

## GroupBy

Group rows of a table based on the values of one or more columns. The resulting
table contains a single row for each unique combination of the specified key
columns. For each of these rows, additional columns are included based on the
value of the `Grouping` parameter.

### Input

| Parameter    | Type                                 | Description                                                                                                                                                                                        |
| ------------ | ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data         | [`Table`](#tables)                   | The source data as a table.                                                                                                                                                                        |
| KeyColumns   | `IEnumerable<string>`                | The columns to use as the key for the grouping.                                                                                                                                                    |
| ResultColumn | `string`                             | The name for the column that will contain the grouped rows/values in the resulting table.                                                                                                          |
| Grouping     | [`GroupingType`](#enum-groupingtype) | How the grouped rows are stored in the resulting table.                                                                                                                                            |
| Column       | `string`                             | If `Grouping` is `SingleColumn`, the name of the column whose values are included in the resulting table for each grouped row.                                                                     |
| Columns      | `IEnumerable<string>`                | If `Grouping` is `SelectedColumns`, the names of the columns to include in the resulting table from the grouped rows.                                                                              |
| ComputeValue | `Func<dynamic, dynamic>`             | If `Grouping` is `Computed`, a function that should produce appropriate values for each set of grouped rows. Receives the grouped rows as input and should produce the value to use in the result. |

#### `enum` GroupingType

| Value             | Contents of the `ResultColumn` column                                                                                       |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `EntireRows`      | Entire grouped rows as a table.                                                                                             |
| `SingleColumn`    | The values of the column specified in the `Column` parameter as a `IEnumerable<dynamic>`.                                   |
| `SelectedColumns` | The grouped rows as a table, but with only the columns specified in the `Columns` parameter include from the original rows. |
| `Computed`        | The value produced by the function in `ComputeValue` parameter.                                                             |

- - -

## Join

Join the rows of two tables on the values of one or more columns.

Each row of the resulting table contains data from one row for each table in the
join. That is, if a row in the left table matches multiple rows in the right
table, the result contains multiple rows for the row from the left table.

### Input

| Parameter | Type                      | Description                                                    |
| --------- | ------------------------- | -------------------------------------------------------------- |
| JoinType  | [`JoinType`](#jointype)   | The type of join to perform (inner, outer or full outer join). |
| Left      | [`JoinTable`](#jointable) | Definitions for the left side of the join.                     |
| Right     | [`JoinTable`](#jointable) | Definitions for the right side of the join.                    |

#### JoinTable

| Property      | Type                        | Description                                                                                                          |
| ------------- | --------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| Data          | [`Table`](#tables)          | The table to use in the join.                                                                                        |
| KeyColumns    | `IEnumerable<string>`       | The names of the columns to use as the join key.                                                                     |
| ResultType    | [`JoinResult`](#joinresult) | How the matching rows are included in the resulting table.                                                           |
| ResultColumn  | `string`                    | If `ResultType` is `Row`, the name for the column that will contain the matching rows from the source table.         |
| ResultColumns | `IEnumerable<string>`       | If `ResultType` is `SelectColumns`, the names of the columns from the source table to include for the matching rows. |

#### `enum` JoinType

| Value       | Description                                                                                                                                               |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Inner`     | Include only matching pairs of rows in the resulting table.                                                                                               |
| `Outer`     | Include all rows from the `Left.Data` table and oly matching rows from the `Right.Data` table. For rows that do not a have a match, null values are used. |
| `FullOuter` | Include all rows from both tables in the resulting table. For rows that do not a have a match in the other table, null values are used.                   |


#### `enum` JoinResult

| Value           | Description                                                                                                        |
| --------------- | ------------------------------------------------------------------------------------------------------------------ |
| `Row`           | Include the entire matching row as a value of a separate column in the resulting table.                            |
| `AllColumns`    | Include all columns from the original table as columns of the resulting table.                                     |
| `SelectColumns` | Include specific columns from the original table as columns of the resulting table.                                |
| `DiscardKey`    | Include all columns from the original table, except those used as the join key, as columns of the resulting table. |

- - -

## RemoveDuplicates

Remove duplicate rows from a table as a new table. Duplicate rows are determined
either using the values of specified columns or the entire row. Duplicate rows
are removed regardless of whether they appear consecutively in the original
table. The order of the rows in the resulting table is undetermined.

### Input

| Parameter  | Type                              | Description                                                                              |
| ---------- | --------------------------------- | ---------------------------------------------------------------------------------------- |
| Data       | [`Table`](#tables)                | The source data as a table.                                                              |
| Key        | `EntireRows` or `SelectedColumns` | Whether to use the entire row or selected columns for determining the duplicate rows.    |
| KeyColumns | `IEnumerable<string>`             | If `Key` is `SelectedColumns`, the names of the columns to determine the duplicate rows. |

- - -

## RenameColumns

Rename columns of a table into a new table.

### Input

| Parameter                | Type                                | Description                                                                                                                                                                                  |
| ------------------------ | ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data                     | [`Table`](#tables)                  | The source data as a table.                                                                                                                                                                  |
| Format                   | `Manual` or `JSON`                  | Whether the renamings are specified manually or as a JSON object.                                                                                                                            |
| Renamings                | [`ColumnRename`](#columnrename)`[]` | If `Format` is `Manual`, the columns to rename and their corresponding names in the resulting table.                                                                                         |
| JsonRenamings            | `string`                            | If `Format` is `JSON`, the columns to rename and their corresponding names in the resulting table as a string representing a JSON object.                                                    |
| PreserveColumnOrder      | `bool`                              | Whether to preserve the original order of renamed columns in the resulting table or re-order according to the order they are specified in `Renamings` / `JsonRenamings`. Defaults to `true`. |
| DiscardOtherColumns      | `bool`                              | Whether to discard any columns of the original table that are not specified in the renamings. Defaults to `false`.                                                                           |
| IgnoreInvalidColumnNames | `bool`                              | Whether to ignore any column names not in the original table or treat them as an error. Defaults to `false`.                                                                                 |

#### ColumnRename

| Property | Type     | Description                       |
| -------- | -------- | --------------------------------- |
| Column   | `string` | The name of the column to rename. |
| NewName  | `string` | The new name for the column.      |

- - -

## ReorderColumns

Reorder columns of a table into a new table. Only the relative order of the
specified columns is changed. The position of any other columns is unchanged in
the resulting table.

### Input

| Parameter           | Type                  | Description                                                                                                               |
| ------------------- | --------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| Data                | [`Table`](#tables)    | The source data as a table.                                                                                               |
| ColumnOrder         | `IEnumerable<string>` | The new order for the columns of the table.                                                                               |
| DiscardOtherColumns | `bool`                | Whether to discard any columns of the original table that are not specified in the new column order. Defaults to `false`. |

### Output

A new [`Table`](#tables), where columns have been reordered.

- - -

## SelectColumns

Keep or discard specific columns of a table into a new table.

### Input

| Parameter           | Type                  | Description                                                                                                                                                             |
| ------------------- | --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data                | [`Table`](#tables)    | The source data as a table.                                                                                                                                             |
| Action              | `Keep` or `Discard`   | Whether to keep or discard the columns that are specified in `Columns`.                                                                                                 |
| Columns             | `IEnumerable<string>` | If `Action` is `Keep`, the names of the columns to keep in the resulting table. If `Action` is `Discard`, the names of the columns to discard from the resulting table. |
| PreserveColumnOrder | `bool`                | If `Action` is `Keep`, specifies whether to preserve the original order of the columns. Defaults to `false`.                                                            |

- - -

## Serialize

Serialize a single table to a file or string so that it can be easily loaded later using the `Load` task.

### Input

| Parameter | Type               | Description                                                      |
| --------- | ------------------ | ---------------------------------------------------------------- |
| Data      | [`Table`](#tables) | The data to serialize, as a table.                               |
| Target    | `File` or `String` | Whether to serialize the table into a file or as a string value. |
| Path      | `string`           | If `Target` is `File`, the path where the table is serialized.   |

### Result

When `Target` is `File`, the result is the normalized path where the table was
serialized. When `Target` is `String` the result is the serialized table data as
a string value.

- - -

## Sort

Sort the rows of a table by one or more columns.

### Input

| Parameter       | Type                                        | Description                                                                                                                  |
| --------------- | ------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| Data            | [`Table`](#tables)                          | The source data as a table.                                                                                                  |
| SortingCriteria | [`SortingCriterion`](#sortingcriterion)`[]` | Specifies the columns according which to sort the rows of the table, and whether to sort the column ascending or descending. |


#### SortingCriterion

| Property | Type                        | Description                                                                           |
| -------- | --------------------------- | ------------------------------------------------------------------------------------- |
| Column   | `string`                    | The name of the column to sort rows by.                                               |
| Order    | `Ascending` or `Descending` | Whether to sort the column in ascending or descending order. Defaults to `Ascending`. |

- - -

## TransformColumns

Transform the values of one or more columns in a table. This a more general version of the `ConvertColumns` task.

### Input

| Parameter  | Type                                      | Description                                    |
| ---------- | ----------------------------------------- | ---------------------------------------------- |
| Data       | [`Table`](#tables)                        | The source data as a table.                    |
| Transforms | [`ColumnTransform`](#columntransform)`[]` | The transform to perform on the table columns. |

#### ColumnTransform

| Property         | Type                                                 | Description                                                                                                                                                                                                                                                                                                                                                                           |
| ---------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Column           | `string`                                             | The name of the column to transform.                                                                                                                                                                                                                                                                                                                                                  |
| TransformType    | `Row`, `Column`, `RowWithIndex` or `ColumnWithIndex` | Whether to transform the values of the column using only the current value of the column or the entire row and whether to use the row indices in the transformation. Defaults to `Column`.                                                                                                                                                                                            |
| Transform        | `Func<dynamic, dynamic>`                             | If `TransformType` is `Row` or `Column`, the function that performs the desired transformation. Depending on the value of `TransformType`, the function receives as input either the current value of the column being transform or the entire row object. The function should return the new value for the column.                                                                   |
| IndexedTransform | `Func<dynamic, int, dynamic>`                        | If `TransformType` is `RowWithIndex` or `ColumnWithIndex`, the function that performs the desired transformation. Depending on the value of `TransformType`, the function receives as input either the current value of the column being transform or the entire row object, as well as the index of the row being processed. The function should return the new value for the column. |


### Examples

#### Transform values of a column using only the current value of the column

Assuming that a column contains string values that should be converted to
lowercase, the transformation can be done using the lambda function:

```c#
current => current.ToLower()
```

#### Transform values of a column using the current data of the whole row

Construct a default value from other columns when the value of a column is `null`:

```c#
row => row.displayName ?? $"{row.firstName} {row.lastName}"
```


# Building

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

# Change Log

| Version | Changes                                        |
| ------- | ---------------------------------------------- |
| 0.90.0  | Development version with all tasks implemented |
| 0.0.1   | Initial version                                |
