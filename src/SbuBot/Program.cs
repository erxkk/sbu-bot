using System;
using System.IO;

using Disqord.Bot.Hosting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SbuBot;

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
        .ConfigureServices(
            (ctx, services) => services
                .AddDbContextFactory<DbContext>()
                .AddSingleton(new SbuBotConfiguration(ctx.Configuration))
                .AddSingleton(typeof(ILogger<>), typeof(ShortContextLogger<>))
        )
        .ConfigureDiscordBot<SbuBot.SbuBot>(
            (ctx, bot) =>
            {
                bot.Token = ctx.Configuration["Discord:Token"];
                bot.Prefixes = new[] { "sbu" };
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