﻿using System;
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
using SbuBot.Services;

using Serilog;

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
                .ByTransforming<SchedulerService.Entry>(entry => new { entry.Id, Remaining = entry.RecurringCount })
                .Destructure
                .ByTransforming<SbuReminder>(reminder => new { reminder.Id, Owner = reminder.OwnerId })
                .WriteTo.File(
                    $"{ctx.Configuration["Log:Path"]}/log.txt",
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
                        .CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
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