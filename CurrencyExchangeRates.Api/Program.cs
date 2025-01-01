using System;
using CurrencyExchangeRates.Api.Extensions;
using CurrencyExchangeRates.Data;
using CurrencyRates.Common.Extensions;
using CurrencyRates.Nbp;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddConfiguration();

    builder.Services.AddSerilogLogger(builder.Configuration);

    builder.Services.AddOpenApi();

    builder.Services.RegisterHangfire(builder.Configuration);
    builder.Services.RegisterDataModule(builder.Configuration);
    builder.Services.RegisterNbpCurrencyExchangeRates();

    builder.Services.AddMediatR(configuration =>
    {
        configuration.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddWindowsService(configure =>
    {
        configure.ServiceName = "CurrencyExchangeRates";
    });
    
    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Currency Exchange Rate API");
        });
    }

    app.MapControllers();

    var hangfireOptions = app.Services.GetRequiredService<IOptions<HangfireOptions>>();

    app.UseHangfireDashboard(
        pathMatch: hangfireOptions.Value.Path,
        options: HangfireExtension.GetDashboardOptions(hangfireOptions.Value));

    HangfireExtension.AddScheduleJobs(app.Services);

    app.Services.EnsureDatabaseCreated();

    app.Run();
    
    Log.Information("Running application");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application error");
}
finally
{
    Log.Information("Application exited");

    await Log.CloseAndFlushAsync();
}
