# Serilog.Enrichers.Correlation

An upgraded version of [serilog-enrichers-correlation-id](https://github.com/ekmsystems/serilog-enrichers-correlation-id) with modernized features, enhanced functionality, and support for the latest .NET versions.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

## Overview

Serilog.Enrichers.Correlation enriches Serilog log events with correlation IDs to track requests across distributed systems and microservices. This package provides multiple enrichment strategies to suit different architectural needs.

## What's New in This Version

This upgraded version includes several improvements over the original package:

### 🚀 Modern .NET Support
- **Multi-targeting**: Supports .NET 8.0, .NET 9.0, and .NET 10.0
- **Updated dependencies**: Uses the latest Serilog 4.3.0 and ASP.NET Core packages

### ✨ Enhanced Features
- **Thread-safe operations**: Improved concurrency handling with `SemaphoreSlim` for reliable multi-threaded scenarios
- **Distributed tracing integration**: Automatic correlation ID generation from `SpanId` and `TraceId` when available
- **Flexible correlation ID sources**: Support for both Activity-based tracing and custom HTTP headers
- **Smart ID generation**: Falls back to GUID generation when distributed tracing is not available

### 🔧 Improved Implementation
- **Better exception handling**: Exceptions during enrichment are logged as properties instead of causing failures
- **Optimized performance**: Uses `ConcurrentDictionary` for efficient correlation ID caching
- **Enhanced testability**: Comprehensive test coverage with NUnit

### 📦 New Dependency Injection Support
- **Fluent service registration**: `WithCorrelationId()` and `WithCorrelationIdHeader()` extension methods for `IServiceCollection`
- **Multiple configuration options**: Support for programmatic, configuration-based, and appsettings.json configuration

## Installation

Install the package via NuGet:

```bash
dotnet add package Serilog.Enrichers.Correlation
```

Or using Package Manager:

```powershell
Install-Package Serilog.Enrichers.Correlation
```

## Usage

### Basic Setup

The simplest way to use the enricher is through the fluent configuration API:

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .CreateLogger();
```

### ASP.NET Core Integration

#### Option 1: Service Collection Registration (Recommended)

Register the enricher with dependency injection:

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add HttpContextAccessor (required)
builder.Services.AddHttpContextAccessor();

// Register correlation ID enricher
builder.Services.WithCorrelationIdHeader();

// Configure Serilog
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

var app = builder.Build();
app.Run();
```

#### Option 2: Direct Configuration

Configure the enricher directly in the logger configuration:

```csharp
builder.Services.AddSerilog((services, lc) => lc
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationIdHeader()
    .WriteTo.Console()
);
```

#### Option 3: Configuration from appsettings.json

Add the enricher through your `appsettings.json`:

```json
{
  "Serilog": {
    "Using": ["Serilog.Enrichers.Correlation", "Serilog.Sinks.Console"],
    "MinimumLevel": "Information",
    "Enrich": ["WithCorrelationIdHeader", "FromLogContext"],
    "WriteTo": ["Console"]
  }
}
```

Then load it in your application:

```csharp
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
);
```

## Available Enrichers

### 1. WithCorrelationId()

Generates correlation IDs using distributed tracing (Activity SpanId and TraceId) or creates a GUID if tracing is unavailable.

**Usage:**

```csharp
// In LoggerConfiguration
.Enrich.WithCorrelationId()

// In IServiceCollection
services.WithCorrelationId();

// With custom header key
.Enrich.WithCorrelationId("x-request-id")
```

**Features:**
- Automatically extracts SpanId and TraceId from the current Activity
- Thread-safe correlation ID management
- Persistent correlation IDs across async operations
- Falls back to GUID generation when distributed tracing is not available

### 2. WithCorrelationIdHeader()

Extracts correlation IDs from HTTP request headers and adds them to response headers.

**Usage:**

```csharp
// In LoggerConfiguration (default header: "x-correlation-id")
.Enrich.WithCorrelationIdHeader()

// With custom header key
.Enrich.WithCorrelationIdHeader("x-request-id")

// In IServiceCollection
services.WithCorrelationIdHeader("x-correlation-id");
```

**Features:**
- Reads correlation ID from incoming request headers
- Automatically adds correlation ID to response headers
- Falls back to SpanId-TraceId if no header is present
- Configurable header key

## How It Works

### Correlation ID Generation Strategy

1. **HTTP Header** (when using `WithCorrelationIdHeader`):
   - First checks the incoming request for the specified header
   - If not found, checks the response headers
   
2. **Distributed Tracing** (when using `WithCorrelationId`):
   - Extracts `SpanId` and `TraceId` from the current `LogEvent`
   - Combines them as `{SpanId}-{TraceId}`
   
3. **Fallback**:
   - Generates a new GUID if no correlation ID is available

### Thread Safety

The enrichers use:
- `SemaphoreSlim` for controlled concurrent access
- `ConcurrentDictionary` for efficient correlation ID caching
- Proper async/await patterns in tests to verify multi-threaded behavior

## Configuration Examples

### Minimal Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .CreateLogger();
```

### Production Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.WithCorrelationIdHeader("x-request-id");

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Services(services)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] " +
        "{SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                       "{CorrelationId} - {Message:lj}{NewLine}{Exception}")
);

var app = builder.Build();
app.Run();
```

### Custom Output Template

Include the correlation ID in your output templates:

```csharp
outputTemplate: "[{Timestamp:HH:mm:ss} {CorrelationId} {Level:u3}] {Message:lj}{NewLine}{Exception}"
```

This will output logs like:

```
[14:32:45 abc123-def456 INF] Processing weather forecast request
[14:32:45 abc123-def456 INF] Weather forecast returned successfully
```

## Testing

The package includes comprehensive unit tests demonstrating:

- Correlation ID property creation
- Persistent correlation IDs across multiple log events
- Thread-safe async operations
- Header extraction and injection
- Fallback scenarios

Run the tests:

```bash
dotnet test
```

## Sample Application

A sample ASP.NET Core web application is included in the `samples/Logger-Sample-Web` directory, demonstrating:

- Three different configuration approaches
- Integration with minimal APIs
- Output formatting with correlation IDs

To run the sample:

```bash
cd samples/Logger-Sample-Web
dotnet run
```

Then navigate to `https://localhost:5001/weatherforecast` to see correlation IDs in action.

## Migration from Original Package

If you're migrating from `Serilog.Enrichers.CorrelationId`:

### Breaking Changes
- Package name changed to `Serilog.Enrichers.Correlation`
- Namespace remains `Serilog` for extension methods
- Minimum .NET version is now .NET 8.0

### API Compatibility
The public API remains largely compatible:
- ✅ `WithCorrelationId()` - works the same way
- ✅ `WithCorrelationIdHeader()` - works the same way
- ✅ `IServiceCollection` extensions - new in this version

### Steps to Migrate

1. Update package reference:
   ```xml
   <PackageReference Include="Serilog.Enrichers.Correlation" Version="1.0.0" />
   ```

2. Update using statements if needed (namespace is mostly the same)

3. Verify your application targets .NET 8.0 or later

4. Test your application - the behavior should be identical or improved

## Requirements

- .NET 8.0, .NET 9.0, or .NET 10.0
- Serilog 4.3.0 or later
- Microsoft.AspNetCore.Http 2.3.0 or later (for ASP.NET Core scenarios)

## Contributing

Contributions are welcome! Please feel free to submit issues, fork the repository, and send pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Credits

This package is an upgraded version of the original [serilog-enrichers-correlation-id](https://github.com/ekmsystems/serilog-enrichers-correlation-id) by ekmsystems, which is no longer maintained. Thanks to the original authors for their foundational work.

## Support

For issues, questions, or contributions, please use the GitHub issue tracker.

---

**Note:** The original `Serilog.Enrichers.CorrelationId` package was archived and is no longer maintained. This upgraded version provides continued support with modern .NET compatibility and enhanced features.