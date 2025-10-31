# GitHub Copilot Instructions for MyPocketPal

## Project Overview

MyPocketPal is a REST API application built with ASP.NET Core 6.0 for managing personal finances. The application provides endpoints for user management, transaction tracking, category management, and settings configuration.

## Technology Stack

- **Framework**: ASP.NET Core 6.0 (Web API)
- **Database**: Microsoft SQL Server
- **Authentication**: Cookie-based authentication with ASP.NET Core Identity
- **API Documentation**: Swagger/OpenAPI
- **Language**: C# with nullable reference types enabled

## Project Structure

```
WebApplication_REST/
├── Controllers/          # API controllers
│   ├── UserController.cs
│   ├── TransactionController.cs
│   ├── CategoryController.cs
│   └── SettingController.cs
├── Models/              # Data models
│   ├── User.cs
│   ├── Transaction.cs
│   ├── TransactionGet.cs
│   ├── Category.cs
│   └── Settings.cs
├── Program.cs           # Application entry point and configuration
└── appsettings.json     # Configuration settings
```

## Coding Standards and Guidelines

### General Principles

1. **Minimal Changes**: Make the smallest possible changes to achieve the goal
2. **Database Access**: Use ADO.NET with SqlConnection and SqlCommand for database operations
3. **Async/Await**: Use async/await patterns for database operations
4. **Error Handling**: Wrap database operations in try-catch blocks and return appropriate HTTP status codes
5. **Authentication**: Use cookie-based authentication for protected endpoints

### Code Style

- Use descriptive variable names
- Follow C# naming conventions (PascalCase for classes/methods, camelCase for parameters/local variables)
- Keep methods focused and concise
- Add XML documentation comments for public APIs when appropriate
- Handle nullable reference types properly

### API Design

- Controllers should be decorated with `[Route("api/[controller]")]` and `[ApiController]`
- Use appropriate HTTP verbs: GET (read), POST (create), PUT (update), DELETE (delete)
- Return proper HTTP status codes:
  - 200 OK for successful operations
  - 201 Created for successful resource creation
  - 400 Bad Request for validation errors
  - 401 Unauthorized for authentication failures
  - 404 Not Found when resource doesn't exist
  - 500 Internal Server Error for unexpected errors

### Database Operations

- Always use parameterized queries to prevent SQL injection
- Use `using` statements for IDisposable resources (SqlConnection, SqlCommand, SqlDataReader)
- Close connections explicitly or rely on `using` statement disposal
- Connection string is retrieved from configuration: `configuration.GetConnectionString("MyDbConnection")`

### Authentication and Security

- Use `PasswordHasher<IdentityUser>` for password hashing
- Never store plain text passwords
- Use Claims-based authentication for user identity
- Session management is configured with a 30-minute timeout
- CORS is configured to allow specific origins from configuration

### Models

- Models use Guid for primary keys
- Properties should follow C# naming conventions
- Consider adding `required` modifier or nullable annotations for non-nullable properties

## Common Patterns

### Database Query Pattern

```csharp
using (SqlConnection connection = new SqlConnection(_connectionString))
{
    await connection.OpenAsync();
    string sqlQuery = "SELECT * FROM TableName WHERE Id = @Id";
    
    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
    {
        command.Parameters.AddWithValue("@Id", id);
        
        using (SqlDataReader reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                // Process results
            }
        }
    }
}
```

### Controller Action Pattern

```csharp
[HttpGet]
public async Task<ActionResult<ReturnType>> MethodName([FromQuery] parameters)
{
    try
    {
        // Implementation
        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Error message: " + ex.Message);
    }
}
```

## Building and Testing

### Build the Project

```bash
dotnet build
```

The project targets .NET 6.0 (currently out of support - consider upgrading to .NET 8 LTS or .NET 9).

### Run the Application

```bash
cd WebApplication_REST
dotnet run
```

The application will start with Swagger UI available in development mode.

## Current Known Issues

- The project targets .NET 6.0, which is out of support. Consider upgrading to a supported version.
- Multiple nullable reference type warnings exist in the codebase. When making changes, address nullable warnings in modified code.
- Some methods have async signatures but don't use await (e.g., `GetUserStatus`)

## Database Schema

The application expects the following tables:
- **Users**: Id (uniqueidentifier), Username (nvarchar), Email (nvarchar), Password (nvarchar)
- **Transactions**: Transaction data with category relationships
- **Categories**: Category definitions for transactions
- **Settings**: User-specific settings including currency preferences

## Important Notes

- The connection string in `appsettings.json` should be updated for different environments
- CORS origins should be configured appropriately for production
- Cookie settings include `SameSite.None` for cross-origin scenarios - ensure this matches your deployment
- Session state is stored in memory - consider using distributed cache for production

## When Adding New Features

1. Create or modify models in the Models folder
2. Add controller methods following existing patterns
3. Use parameterized SQL queries for all database operations
4. Implement proper error handling and return appropriate status codes
5. Test with Swagger UI in development mode
6. Ensure nullable reference type warnings are addressed
7. Follow async/await patterns for I/O operations
