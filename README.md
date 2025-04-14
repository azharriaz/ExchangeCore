﻿## Technologies
* [ASP.NET Core 8](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-8.0)
* [MediatR](https://github.com/jbogard/MediatR)
* [Mapster](https://github.com/MapsterMapper/Mapster)
* [FluentValidation](https://fluentvalidation.net/)
* [Serilog](https://serilog.net/))
* [OpenTelemetry](https://serilog.net/))
* [HybridCache](https://serilog.net/))

## Running the Application

1. Clone this repository to your local machine using git clone.

2. Open the solution in your preferred IDE (e.g., Visual Studio, Visual Studio Code).

3. Ensure that the required NuGet packages are restored.

4. Application settings are already configured to use the https://api.frankfurter.app/.

5. Build the solution to ensure all projects are compiled successfully.

6. Run the application using the IISExpress profile or by executing dotnet run in the terminal/command prompt from the root of the solution.

7. Once the application is running, you can access the following endpoints:

# Fetch User Token:

7.0 Endpoint: POST /api/auth/loing
    Example:'
        curl --location 'https://localhost:5000/api/Auth/login' \
        --header 'accept: */*' \
        --header 'Content-Type: application/json' \
        --data-raw '{
             "username": "user",
             "password": "User@123"
        }'
# Retrieve Latest Exchange Rates:

7.1 Endpoint: GET /currencies/latest?baseCurrency={baseCurrency}
    Example: GET http://localhost:5000/currencies/latest?baseCurrency=EUR

# Convert Currency:

7.2 Endpoint: GET /currencies/convert
    Example: https://localhost:5000/api/currencies/convert?amount=10&fromCurrency=GBP&toCurrency=EUR

# Fetch Admin Token:

7.0 Endpoint: POST /api/auth/loing
    Example:'
        curl --location 'https://localhost:5000/api/Auth/login' \
        --header 'accept: */*' \
        --header 'Content-Type: application/json' \
        --data-raw '{
             "username": "admin",
             "password": "Admin@123"
        }'

# Get Historical Rates:

7.3 Endpoint: Get /currencies/history
    Example: https://localhost:5000/api/currencies/history?baseCurrency=GBP&startDate=2024-01-11&endDate=2024-01-31&pageNumber=1&pageSize=30

## Postman collection is attached in the same directory to help understand usage of API.

## Running Tests
1. Navigate to the test project directory (CodingChallenge.UnitTests) in your terminal/command prompt.

2. Execute dotnet test to run all unit tests.

3. Review the test results to ensure all tests pass successfully.

4. Note: Tests are only covered for first two apis (latest, convert) and pending for Historical Exchange Rate api.

```

## Overview

### Domain

This will contain all entities, enums, exceptions, interfaces, types and logic specific to the domain layer.

### Application

This layer contains all application logic. It is dependent on the domain layer, but has no dependencies on any other layer or project. This layer defines interfaces that are implemented by outside layers. For example, if the application need to access a notification service, a new interface would be added to application and an implementation would be created within infrastructure.

### Infrastructure

This layer contains classes for accessing external resources such as file systems, web services, smtp, and so on. These classes should be based on interfaces defined within the application layer.

### WebApi

This layer is a web api application based on ASP.NET 9.0.x. This layer depends on both the Application and Infrastructure layers, however, the dependency on Infrastructure is only to support dependency injection. Therefore only *Program.cs* should reference Infrastructure.

### Possible future enhancements
** Logging can be extended to customize the tracing.
** Currently interfaces are placed inside the Application layer due to dependency over MediatR which needs to handle in Domain layer.
** Hybrid Cache can be offloaded from handler to add some pattern i.e. Decorator etc. Distributed cache to be configured.
** Test cases are to be refactored further.
** Aspire or real time UI (Grafana) to be configured to properly see to Traces/Logs.

=======