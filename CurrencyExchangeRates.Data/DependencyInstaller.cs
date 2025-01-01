using CurrencyExchangeRates.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchangeRates.Data;

public static class DependencyInstaller
{
    public static void RegisterDataModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterDbContext(configuration);
    }

    private static void RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionString:DefaultDatabase"];
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlServer(connectionString, cfg =>
            {
                cfg.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName);
            });
        });
    }

    public static void EnsureDatabaseCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        context.Database.Migrate();
    }
}