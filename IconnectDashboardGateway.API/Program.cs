using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Infrastructure.Connection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Connection string from registry (singleton cache)
builder.Services.AddSingleton<IRegistryConnectionStringProvider, RegistryConnectionStringProvider>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
