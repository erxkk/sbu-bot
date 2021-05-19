using System;

using Disqord.Bot.Hosting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SbuBot;

try
{
    IHostBuilder hostBuilder = new HostBuilder()
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