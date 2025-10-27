# Countries API

A RESTful API service that provides information about countries, including their demographics, currencies, and exchange rates. Built with .NET 9 and C# 13.0.

## Features

- Get information about countries with filtering options (region, currency)
- Search countries by name
- Automatic data refresh from external sources
- Exchange rate information
- Status monitoring
- Rate limiting
- CORS support
- Summary image generation
- Swagger/OpenAPI documentation

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- MySQL Server
- Visual Studio 2022 or later (recommended)

## Dependencies

The project uses the following main packages:
- Entity Framework Core (for MySQL)
- ASP.NET Core
- Swagger/OpenAPI
- Microsoft.Extensions.Http
- System.Drawing.Common (for image generation)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/King-Mikaelson/CountriesAPI.git
cd CountriesAPI
```

2. Install the required .NET packages:
```bash
dotnet restore
```

3. Update the database:
```bash
dotnet ef database update
```

## Configuration

The application requires the following configuration settings. Create a `user-secrets` file or update `appsettings.json` with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=your_database;User=your_user;Password=your_password;"
  }
}
```

### Environment Variables

Set up the following environment variables or add them to your user secrets:
- `ConnectionStrings:DefaultConnection`: MySQL database connection string

## Running the Application

1. Start the application:
```bash
dotnet run
```

2. The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

3. Access Swagger documentation at:
```
https://localhost:5001/swagger
```

## API Endpoints

- `GET /countries` - Get all countries (with optional filters)
- `GET /countries/{name}` - Get a specific country by name
- `POST /countries/refresh` - Refresh country data from external sources
- `DELETE /countries/{name}` - Delete a country
- `GET /status` - Get API status
- `GET /countries/image` - Get summary image

## Rate Limiting

The API implements rate limiting with the following rules:
- 10 requests per 10 seconds window
- No queue for excess requests

## Error Handling

The API uses standard HTTP status codes and returns error responses in the following format:
```json
{
    "error": "Error message",
    "details": "Additional details (optional)"
}
```

## Development

To run the application in development mode with hot reload:
```bash
dotnet watch run
```

## Security

- HTTPS redirection is enabled
- CORS is configured to allow all origins (can be restricted in production)
- Rate limiting is implemented to prevent abuse

## License

[MIT License](LICENSE)