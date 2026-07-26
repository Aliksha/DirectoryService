using System.Globalization;
using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Exceptions;

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
//    .CreateBootstrapLogger();

// 1. Проверяем, запущены ли мы внутри интеграционных тестов (xUnit/testhost)
bool isTesting = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.FullName!.Contains("Microsoft.AspNetCore.Mvc.Testing") || a.FullName!.Contains("testhost"));

if (!isTesting)
{
    // Инициализируем Serilog только для обычного запуска приложения
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .CreateBootstrapLogger();
}

try
{
    //Log.Information("Starting web application");

    //var builder = WebApplication.CreateBuilder(args);

    //// переключение на полную конфигурацию (из appsettings)
    //builder.Host.UseSerilog((context, services, configuration) =>
    //{
    //    configuration
    //        .ReadFrom.Configuration(context.Configuration)
    //        .Enrich.FromLogContext()
    //        .Enrich.WithExceptionDetails()
    //        .Enrich.WithProperty("ServiceName", "DirectoryService");
    //});

    if (!isTesting)
        Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // настраиваем Serilog для Host
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService");

        // подключаем службы логгера только если это НЕ тесты
        if (!isTesting)
        {
            configuration.ReadFrom.Services(services);
        }
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers()
     .AddApplicationPart(typeof(DirectoryService.Presenters.PositionController).Assembly);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseExceptionMiddleware();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
    }

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // for resetting buffers
    Log.CloseAndFlush();
}

namespace DirectoryService.Web
{
    public partial class Program;
}
