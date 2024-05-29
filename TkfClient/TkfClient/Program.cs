using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using StackExchange.Redis;
using TkfClient;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Tinkoff Client";
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IDatabaseAsync>(cfg =>
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect("192.168.1.100:6379");
            return connectionMultiplexer.GetDatabase();
        });
        // Подписка на свечи
        services.AddHostedService<StreamService>();
        // Синхронизация тикеров
        services.AddHostedService<SyncSharesService>();
        // Сихронизация свечей
        // services.AddHostedService<SyncCandlesService>();
        services.AddInvestApiClient((_, settings) => context.Configuration.Bind(settings));
        services.AddDbContextFactory<AppContext>(options =>
        {
            options.UseNpgsql(context.Configuration.GetConnectionString("db"));
        });
        services.AddSingleton<DbRepository>();

    }).ConfigureLogging((context, builder) =>
    {
        builder.AddNLog();
    });

IHost host = builder.Build();
host.Run();