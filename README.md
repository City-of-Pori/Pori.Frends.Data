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

## Filter

Filter rows of a table into a new table.

## RenameColumns

Rename columns of a table into a new table.

## ReorderColumns

Reorder columns of a table into a new table.

## SelectColumns

Keep or discard specific columns of a table into a new table.

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
| 0.0.1   | Initial version |
