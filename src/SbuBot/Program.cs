using System;
using System.IO;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

using EFCoreSecondLevelCacheInterceptor;

using HumanTimeParser.Core.TimeConstructs;
using HumanTimeParser.English;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SbuBot;
using SbuBot.Logging;
using SbuBot.Models;

using Serilog;

try
{
    IHostBuilder hostBuilder = new HostBuilder()
        .ConfigureAppConfiguration(
            (ctx, config) => config.SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables("DOTNET_")
                .AddYamlFile("config.yaml")
                .Build()
        )
        .UseSerilog(
            (ctx, services, logging) => logging
                .MinimumLevel.Verbose()
                .Destructure.ToMaximumDepth(3)
                .Destructure.ToMaximumCollectionCount(5)
                .Destructure.ToMaximumStringLength(50)
                .Destructure.ByTransforming<Snowflake>(snowflake => snowflake.RawValue)
                .WriteTo.File(
                    "logs/log.txt",
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss:fff zzz} {Level:u3} : {SourceContext}] {Message:l}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day
                )
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3} : {SourceContext}] {Message:l}{NewLine}{Exception}"
                )
        )
        .ConfigureServices(
            (ctx, services) => services
                .AddHttpClient()
                .AddDbContext<SbuDbContext>()
                .AddEFSecondLevelCache(
                    cache => cache
                        .CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(15))
                        .DisableLogging()
                )
                .AddSingleton(new SbuBotConfiguration(ctx.Configuration))
                .AddSingleton(typeof(ILogger<>), typeof(ShortContextLogger<>))
                .AddSingleton(new EnglishTimeParser(new(new("en-US"), ClockType.TwentyFourHour)))
        )
        .ConfigureDiscordBot<SbuBot.SbuBot>(
            (ctx, bot) =>
            {
                bot.Token = ctx.Configuration["Discord:Token"];
                bot.Prefixes = new[] { SbuGlobals.DEFAULT_PREFIX };
                bot.Intents = GatewayIntents.Recommended;
            }
        );

    using IHost host = hostBuilder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadKey();
}