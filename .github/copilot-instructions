# TriageCat — Copilot Instructions for TriageCat-fun

## Project architecture

TriageCat is a photo-triage application consisting of:

- React 18.2 and TypeScript frontend, built with Vite.
- Fluent UI `@fluentui/react-components` v9.56.
- C# Azure Functions using the .NET 10 isolated worker.
- Azure SQL database or SQLite database, abstracted through a database layer
- Azure Blob Storage for images

## C# conventions

- Use PascalCase for C# types, properties, methods, and other applicable members.
- Exception: JSON wire types in `Types/Wire/` preserve camelCase member names.
- Target .NET 10.
- Use Azure Functions isolated worker, without ASP.NET Core integration.
- Follow existing HTTP endpoint and authentication patterns.

## General development requirements

- Prefer modifications consistent with existing architecture over introducing new frameworks.
- Maintain separation between:
  - functions
  - model implementation
  - database layer and database specific implementations
  - wire types in Types/Wire/**
  - model types in Types/Model/**
- Use async/await for asynchronous operations.
- Avoid unnecessary dependencies.
- Preserve compatibility with the existing project.
- Consult the root README.md for additional project architecture and implementation details.
