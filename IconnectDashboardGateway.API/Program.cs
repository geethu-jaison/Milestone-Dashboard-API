using IconnectDashboardGateway.API.Middleware;
using IconnectDashboardGateway.Application.Interfaces.Auth;
using IconnectDashboardGateway.Application.Interfaces.Camera;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.DataAccess;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Application.Interfaces.Repositories;
using IconnectDashboardGateway.Application.Interfaces.Server;
using IconnectDashboardGateway.Application.Interfaces.Site;
using IconnectDashboardGateway.Application.Services;
using IconnectDashboardGateway.Infrastructure.Auth;
using IconnectDashboardGateway.Infrastructure.Connection;
using IconnectDashboardGateway.Infrastructure.DataAccess;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Connection string from registry (singleton cache)
builder.Services.AddSingleton<IRegistryConnectionStringProvider, RegistryConnectionStringProvider>();
builder.Services.AddScoped<IAppLogger, AppLogger>();// App logger service for logging
builder.Services.AddScoped<ICameraService, CameraService>();//camera service
builder.Services.AddScoped<ICameraRepository, CameraRepository>();//camera repository for db access
builder.Services.AddScoped<IServerRepository, ServerRepository>();//server repository for db access
builder.Services.AddScoped<IServerService, ServerService>();// server service
builder.Services.AddScoped<ISiteService, SiteService>(); // site service
builder.Services.AddScoped<ISiteRepository, SiteRepository>();// site repository for db access
builder.Services.AddScoped<BearerTokenMiddleware>();// Auth middleware to validate bearer tokens on incoming requests
// auth and token related services DI's
builder.Services.AddScoped<ISiteAuthService,SiteAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenStore, TokenStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseRouting();
app.UseMiddleware<BearerTokenMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
