using System;
using System.IO;

using Disqord;
using Disqord.Bot.Hosting;

using EFCoreSecondLevelCacheInterceptor;

using HumanTimeParser.Core.TimeConstructs;
using HumanTimeParser.English;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SbuBot;
using SbuBot.Logging;
using SbuBot.Models;

using Serilog;
using Serilog.Events;

try
{
    IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(
            (_, config) => config.SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables("DOTNET_")
                .AddEnvironmentVariables("BOT_")
                .Build()
        )
        .UseSerilog(
            (ctx, _, logging) => logging
                .MinimumLevel.Verbose()
                .Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumCollectionCount(5)
                .Destructure.ToMaximumStringLength(50)
                .Destructure.ByTransforming<Snowflake>(snowflake => snowflake.RawValue)
                .Destructure
                .ByTransforming<SbuReminder>(reminder => new { Id = reminder.MessageId, Owner = reminder.OwnerId })
                .WriteTo.File(
                    $"{ctx.Configuration["Log:Path"]}/log.txt",
                    ctx.HostingEnvironment.IsProduction() ? LogEventLevel.Debug : LogEventLevel.Verbose,
                    "[{Timestamp:HH:mm:ss:fff zzz} {Level:u3} : {SourceContext}] {Message:l}{NewLine}{Exception}",
                    rollingInterval:
                    RollingInterval.Day
                )
                .WriteTo.Console(
                    ctx.HostingEnvironment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Verbose,
                    "[{Timestamp:HH:mm:ss} {Level:u3} : {SourceContext}] {Message:l}{NewLine}{Exception}"
                )
        )
        .ConfigureServices(
            (_, services) => services
                .AddHttpClient()
                .AddDbContext<SbuDbContext>()
                .AddEFSecondLevelCache(
                    cache => cache
                        .CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                        .DisableLogging()
                )
                .AddSingleton<SbuConfiguration>()
                .AddSingleton(typeof(ILogger<>), typeof(ShortContextLogger<>))
                .AddSingleton(_ => new EnglishTimeParser(new(new("en-US"), ClockType.TwentyFourHour)))
        )
        .ConfigureDiscordBot<SbuBot.SbuBot>(
            (ctx, bot) =>
            {
                bot.Token = ctx.Configuration["Discord:Token"];
                bot.Prefixes = new[] { ctx.HostingEnvironment.IsProduction() ? SbuGlobals.DEFAULT_PREFIX : "dev" };
            }
        );

    using (IHost host = hostBuilder.Build())
    {
        using (IServiceScope scope = host.Services.CreateScope())
        {
            SbuDbContext db = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
            await db.Database.MigrateAsync();
        }

        await host.RunAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadKey();
}