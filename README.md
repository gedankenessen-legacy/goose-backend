# Goose Backend

This Solution represends the backend part of the Goose-Project. The Solution is seperated into projects which seperate things of concerns. E.g the business models will be implemented in the `Goose.Domain` project, the corosponding tests can be implemented in the `Goose.Tests.Domain.UnitTests` project.

## Technologies

* ASP.NET Core 5
* AutoMapper
* NUnit
* Swagger
* MongoDB
* Docker for Hosting

## Getting Started

1. Clone this repository
2. (Optional if no database access is requiered) Ask @pheim1 for application secrets in order to access the database
3. Start coding & have fun

## Code Style Guide

The general [C# Coding Conventions](https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) **should** be followed. Feel free to discuse the guidlines, if you are not satisfied with it or want to append it.

### The following rules must be respected

* Make use of documenting your code, especially functions you created. [Document your code with XML comments](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc)
* Respect the [Allman style](https://de.wikipedia.org/wiki/Einr%C3%BCckungsstil#Allman_/_BSD).
* Avoid more than one empty line.
* Try using #region/#endregion for seperations if it make senese.
* Use `nameOf(...)` instead of the string name "...", in order to allow the rename function and the IntelliSense take use of it.
* Instead of program long if statemends, make use of design patterns, e.g. [Strategy Pattern](https://refactoring.guru/design-patterns/strategy) in combination with [Factory Method](https://refactoring.guru/design-patterns/factory-method) or so.
