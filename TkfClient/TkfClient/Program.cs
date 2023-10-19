using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using TkfClient;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Tinkoff Client";
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<TkfService>();
        services.AddInvestApiClient((_, settings) => context.Configuration.Bind(settings));
    }).ConfigureLogging((context, builder) =>
    {
        builder.AddNLog();
    });

IHost host = builder.Build();
host.Run();