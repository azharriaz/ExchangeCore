using ExchangeCore.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureAppServices();

var app = builder.Build();

app.ConfigurePipeline();

app.Run();
