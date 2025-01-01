using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CurrencyRates.Common.Hangfire;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.InMemory;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyRates.Common.Extensions;

public class HangfireOptions
{
    public const string SectionName = "Hangfire";
    
    public string Path { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
}

public static class HangfireExtension
{
    public static void RegisterHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HangfireOptions>(configuration.GetSection(HangfireOptions.SectionName));
        
        services.AddHangfire((sp, cfg) => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage(new InMemoryStorageOptions()
            {
                CommandTimeout = TimeSpan.FromMinutes(5),
                MaxExpirationTime = TimeSpan.FromDays(2)
            })
            .UseActivator(new ServiceProviderActivator(sp.GetRequiredService<IServiceScopeFactory>()))
        );

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
        });


        var scheduleJobsTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IScheduleJob).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
            .ToArray();

        foreach (var jobType in scheduleJobsTypes)
        {
            services.AddScoped(typeof(IScheduleJob), jobType);
            services.AddScoped(jobType);
        }
    }

    public static DashboardOptions GetDashboardOptions(HangfireOptions hangfireOptions)
    {
        var options = new DashboardOptions();
        options.Authorization =
        [
            new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions()
            {
                RequireSsl = false,
                SslRedirect = false,
                LoginCaseSensitive = true,
                Users = new[]
                {
                    new BasicAuthAuthorizationUser()
                    {
                        Login = hangfireOptions.Login,
                        PasswordClear = hangfireOptions.Password
                    }
                }
            })
        ];
            
        return options;
    }

    public static void AddScheduleJobs(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var jobs = scope.ServiceProvider.GetServices<IScheduleJob>();

        foreach (var job in jobs)
        {
            var descriptionAttribute = job.GetType().GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            var description = descriptionAttribute?.Description ?? job.GetType().Name;
            
            RecurringJob.AddOrUpdate(description, () => job.Execute(), () => job.CronExpression);
        }
    }
}

public class ServiceProviderActivator : AspNetCoreJobActivator
{
    protected IServiceScopeFactory ScopeFactory { get; }

    #region ServiceProviderActivator()
    public ServiceProviderActivator(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {
        ScopeFactory = serviceScopeFactory;
    }
    #endregion

    #region ActivateJob()
    public override object ActivateJob(Type jobType)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region BeginScope()
    public override JobActivatorScope BeginScope(JobActivatorContext context)
    {
        throw new NotImplementedException();
    }

    public override JobActivatorScope BeginScope(PerformContext context)
    {
        return new ServiceProviderJobActivatorScope(ScopeFactory.CreateAsyncScope(), context);
    }
    #endregion
}

public class ServiceProviderJobActivatorScope : JobActivatorScope
{
    protected AsyncServiceScope Scope { get; }

    #region ServiceProviderJobActivatorScope()
    public ServiceProviderJobActivatorScope(AsyncServiceScope scope, PerformContext context)
    {
        Scope = scope;
    }
    #endregion

    #region Resolve()
    public override object Resolve(Type type)
    {
        return ActivatorUtilities.CreateInstance(Scope.ServiceProvider, type);
    }
    #endregion

    #region DisposeScope()
    public override void DisposeScope()
    {
        Scope.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
    }
    #endregion
}

