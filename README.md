# Pori.Frends.Data
Frends tasks for data manipulation inside a process.

- [Installing](#installing)
- [Tasks](#tasks)
     - [Load](#Load)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View by using `Import Task NuGet` button in Administration > Tasks.

# Tasks

## Load

Load data into a new table.

## AddColumns

Add one or more columns to a table.

## Concatenate

Concatenate the rows of multiple tables into a new table.

## ConvertColumns

Convert values of a table column to a specific data type.

## Filter

Filter rows of a table into a new table.

## GroupBy

Group rows of a table based on the values of one or more columns.

## Join

Join the rows of two tables on the values of one or more columns.

## RemoveDuplicates

Remove duplicate rows from a table as a new table.

## RenameColumns

Rename columns of a table into a new table.

## ReorderColumns

Reorder columns of a table into a new table.

## SelectColumns

Keep or discard specific columns of a table into a new table.

## Sort

Sort the rows of a table by one or more columns.

## TransformColumns

Transform the values of one or more columns in a table.

# Building

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

# Change Log

| Version | Changes |
| ------- | ------- |
| 0.14.0  | Development version with all tasks implemented |
| 0.0.1   | Initial version |
