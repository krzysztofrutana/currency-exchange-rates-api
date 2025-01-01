using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace CurrencyExchangeRates.Api.Extensions;

public static class SerilogExtensions
{
    public static void AddSerilogLogger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((_, cfg) =>
        {
            cfg.Enrich.FromLogContext();
            cfg.ReadFrom.Configuration(configuration);
            cfg.WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs/ex_.log"), rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Error, retainedFileCountLimit: 7)
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs/info_.log"), rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information, retainedFileCountLimit: 7);
        });
    }
}